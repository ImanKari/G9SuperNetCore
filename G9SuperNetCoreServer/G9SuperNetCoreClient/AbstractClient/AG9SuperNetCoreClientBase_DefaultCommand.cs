using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using G9Common.Enums;
using G9Common.HelperClass;
using G9Common.LogIdentity;
using G9Common.Resource;
using G9Common.ServerClient;
using G9LogManagement.Enums;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Enums;

namespace G9SuperNetCoreClient.AbstractClient
{
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession> : AG9ServerClientCommon<TAccount>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        #region Default Command Methods

        #region Default Ping Command

        /// <summary>
        ///     Ping Command Handler
        /// </summary>
        private void G9PingCommandReceiveHandler(string receiveData, TAccount account, Guid requestId,
            Action<string, CommandSendType> sendDataForThisCommand)
        {
            if (DateTime.TryParse(receiveData, out _))
            {
                sendDataForThisCommand(receiveData, CommandSendType.Asynchronous);
            }
            else if (ushort.TryParse(receiveData, out var receivePingResult))
            {
                _mainAccountUtilities.SessionHandler.Core_SetPing(receivePingResult);
                if (_logging.CheckLoggingIsActive(LogsType.INFO))
                    _logging.LogInformation(account.Session.GetSessionInfo(), G9LogIdentity.CLIENT_PING,
                        LogMessage.ClientPing);
            }
        }

        #endregion

        #region Default Echo Command

        /// <summary>
        ///     Echo Command Handler
        /// </summary>
        private void G9EchoCommandReceiveHandler(string receiveData, TAccount account, Guid requestId,
            Action<string, CommandSendType> sendDataForThisCommand)
        {
            if (_logging.CheckLoggingIsActive(LogsType.INFO))
                _logging.LogInformation($"{LogMessage.CommandEcho}\n{LogMessage.ReceiveData}: {receiveData}",
                    G9LogIdentity.ECHO_COMMAND,
                    LogMessage.SuccessfulOperation);
        }

        #endregion

        #region Default Test Send Receive

        /// <summary>
        ///     Field for save text counter
        /// </summary>
        private int _testCounter;

        /// <summary>
        ///     Test Send Receive Command Handler
        /// </summary>
        private void G9TestSendReceiveCommandReceiveHandler(string receiveData, TAccount account, Guid requestId,
            Action<string, CommandSendType> sendDataForThisCommand)
        {
            // Send receive data again
            sendDataForThisCommand(receiveData, CommandSendType.Asynchronous);

            // Set log
            if (_logging.CheckLoggingIsActive(LogsType.INFO))
                _logging.LogInformation(
                    $"{LogMessage.CommanTestSendReceive}\n{LogMessage.ReceiveData}: {receiveData}\n{LogMessage.TestNumber}: {_testCounter++}",
                    G9LogIdentity.TEST_SEND_RECEIVE, LogMessage.SuccessfulOperation);
        }

        #endregion

        #region Default Authorization Command

        /// <summary>
        ///     Authorization Command Handler
        /// </summary>
        private void AuthorizationReceiveHandler(byte[] receiveData, TAccount account, Guid requestId,
            Action<byte[], CommandSendType> sendDataForThisCommand)
        {
            const string privateKeyEmptyError = "Private Key Is Empty";
            try
            {
                if (receiveData.Length == 1)
                {
                    if ((DisconnectReason) receiveData[0] == DisconnectReason.AuthorizationIsSuccess)
                        // Success - Server and client without ssl
                    {
                        EnableSslConnection = false;
                    }
                    else if ((DisconnectReason) receiveData[0] ==
                             DisconnectReason.AuthorizationFailClientIsSslButServerWithoutSsl)
                    {
                        OnDisconnectedHandler(account, (DisconnectReason) receiveData[0]);

                        if (_logging.CheckLoggingIsActive(LogsType.ERROR))
                            // Set log
                            _logging.LogError(
                                $"{LogMessage.AuthorizationFail}\n{LogMessage.Reason}: {((DisconnectReason) receiveData[0]).ToString()}\n{LogMessage.ServerWithOutCertificate}\n{account.Session.GetSessionInfo()}",
                                G9LogIdentity.AUTHORIZATION_FAIL, LogMessage.FailedOperation);
                    }
                    else if ((DisconnectReason) receiveData[0] ==
                             DisconnectReason.AuthorizationFailServerIsSslButClientWithoutSsl)
                    {
                        OnDisconnectedHandler(account, (DisconnectReason) receiveData[0]);

                        if (_logging.CheckLoggingIsActive(LogsType.ERROR))
                            // Set log
                            _logging.LogError(
                                $"{LogMessage.AuthorizationFail}\n{LogMessage.Reason}: {((DisconnectReason) receiveData[0]).ToString()}\n{LogMessage.ClientWithOutCertificate}\n{account.Session.GetSessionInfo()}",
                                G9LogIdentity.AUTHORIZATION_FAIL, LogMessage.FailedOperation);
                    }
                    else
                    {
                        OnDisconnectedHandler(account, (DisconnectReason) receiveData[0]);

                        if (_logging.CheckLoggingIsActive(LogsType.ERROR))
                            // Set log
                            _logging.LogError(
                                $"{LogMessage.AuthorizationFail}\n{LogMessage.Reason}: {((DisconnectReason) receiveData[0]).ToString()}\n{account.Session.GetSessionInfo()}",
                                G9LogIdentity.AUTHORIZATION_FAIL, LogMessage.FailedOperation);
                    }
                }
                else
                {
                    // Check private key
                    if (string.IsNullOrEmpty(_privateKey))
                        throw new Exception(privateKeyEmptyError);

                    // Specified key
                    var md5ExtraPass32Char = requestId.ToByteArray()
                        .Concat(Configuration.EncodingAndDecoding.EncodingType.GetBytes(_clientIdentity)).ToArray()
                        .GenerateMd5();

                    // Set encryption
                    _encryptAndDecryptDataWithCertificate = new G9EncryptAndDecryptDataWithCertificate(
                        new G9SslCertificate(_privateKey,
                            new X509Certificate2(receiveData,
                                $"{G9SslCertificate.GenerateNewPrivateKey(_privateKey)}{md5ExtraPass32Char}")), false);

                    // Set packet size
                    _packetSize = _packetManagement.MaximumPacketSizeInSslMode();

                    // Initialize again state object with new packet size
                    _stateObject.ChangeBufferSize(_packetSize);

                    // Enable ssl
                    EnableSslConnection = true;
                }

                // Set enable authorization
                _mainAccountUtilities.SessionHandler.Core_AuthorizationClient();

                // Send answer for authorization
                SendCommandByNameWithCustomPacketDataType(nameof(G9ReservedCommandName.G9Authorization),
                    new[] {(byte) DisconnectReason.AuthorizationIsSuccess},
                    G9PacketDataType.Authorization);

                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                    _logging.LogEvent(
                        $"{LogMessage.AuthorizationSuccess}\n{account.Session.GetSessionInfo()}",
                        G9LogIdentity.AUTHORIZATION_SUCCESS, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                DisconnectReason reason;
                if (ex.Message == "The specified network password is not correct.")
                {
                    reason = DisconnectReason.AuthorizationFailPrivateKeyNotCorrect;
                }
                else if (ex.Message == "Cannot find the requested object.")
                {
                    reason = DisconnectReason.AuthorizationFailCertificateIsDamage;
                }
                else if (ex.Message == privateKeyEmptyError)
                {
                    reason = DisconnectReason.AuthorizationFailPrivateKeyIsEmpty;
                }
                else
                {
                    reason = DisconnectReason.AuthorizationFailUnknownError;
                    OnErrorHandler(ex, ClientErrorReason.ErrorInAuthorization);
                }

                // Send answer for authorization
                SendCommandByNameWithCustomPacketDataType(nameof(G9ReservedCommandName.G9Authorization),
                    new[] {(byte) reason}, G9PacketDataType.Authorization);

                // Run like a task because if disconnect can't send disconnect reason to server
                Task.Run(async () =>
                {
                    await Task.Delay(399);
                    // Run event disconnect
                    OnDisconnectedHandler(_mainAccountUtilities.Account, reason);
                });

                if (_logging.CheckLoggingIsActive(LogsType.ERROR))
                    // Set log
                    _logging.LogError(
                        $"{LogMessage.AuthorizationFail}\n{LogMessage.Reason}: {reason.ToString()}\n{account.Session.GetSessionInfo()}",
                        G9LogIdentity.AUTHORIZATION_FAIL, LogMessage.FailedOperation);
            }
        }

        #endregion

        #endregion
    }
}