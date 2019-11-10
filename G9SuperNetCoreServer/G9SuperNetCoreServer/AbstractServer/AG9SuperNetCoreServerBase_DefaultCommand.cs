using System;
using System.Globalization;
using System.Linq;
using G9Common.Enums;
using G9Common.HelperClass;
using G9Common.LogIdentity;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Enums;

namespace G9SuperNetCoreServer.AbstractServer
{
    public abstract partial class AG9SuperNetCoreServerBase<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        #region Default Command Methods

        #region Default Ping Command

        /// <summary>
        ///     Ping Command Handler
        /// </summary>
        private void G9PingCommandReceiveHandler(string receiveData, TAccount account, Guid requestId,
            Action<string, CommandSendType> sendDataForThisCommand)
        {
            if (DateTime.TryParse(receiveData, out var receiveDateTime))
            {
                var ping = (ushort) (DateTime.Now - receiveDateTime).TotalMilliseconds;
                _core.GetAccountUtilitiesBySessionId(account.Session.SessionId).SessionHandler
                    .Core_SetPing(ping);
                sendDataForThisCommand(ping.ToString(CultureInfo.InvariantCulture), CommandSendType.Asynchronous);
                if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                    _core.Logging.LogInformation(account.Session.GetSessionInfo(), G9LogIdentity.CLIENT_PING,
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
            if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                _core.Logging.LogInformation($"{LogMessage.CommandEcho}\n{LogMessage.ReceiveData}: {receiveData}",
                    G9LogIdentity.ECHO_COMMAND,
                    LogMessage.SuccessfulOperation);
            Console.WriteLine($"{LogMessage.CommandEcho}: {LogMessage.ReceiveData}: {receiveData}");
            sendDataForThisCommand(receiveData, CommandSendType.Asynchronous);
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
            // Check if disable test mode return
            if (!account.Session.EnableTestSendReceiveMode) return;

            // if enable => send receive data
            sendDataForThisCommand(receiveData, CommandSendType.Asynchronous);

            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                _core.Logging.LogInformation(
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
            // Specify client connected without ssl connection
            if (receiveData.Length == 0)
            {
                if (_core.EnableSslConnection)
                {
                    SendCommandByNameWithCustomPacketDataType(account.Session.SessionId,
                        nameof(G9ReservedCommandName.G9Authorization),
                        new[] {(byte) DisconnectReason.AuthorizationFailServerIsSslButClientWithoutSsl},
                        G9PacketDataType.Authorization, requestId);

                    if (_core.Logging.CheckLoggingIsActive(LogsType.ERROR))
                        // Set log
                        _core.Logging.LogError(
                            $"{LogMessage.AuthorizationFail}\n{LogMessage.Reason}: {DisconnectReason.AuthorizationFailServerIsSslButClientWithoutSsl.ToString()}\n{LogMessage.ClientWithOutCertificate}\n{account.Session.GetSessionInfo()}",
                            G9LogIdentity.AUTHORIZATION_FAIL, LogMessage.FailedOperation);
                }
                else
                {
                    // Set enable authorization
                    _core.GetAccountUtilitiesBySessionId(account.Session.SessionId).SessionHandler
                        .Core_AuthorizationClient();
                    // Set log
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
                        _core.Logging.LogEvent(
                            $"{LogMessage.AuthorizationSuccess}\n{account.Session.GetSessionInfo()}",
                            G9LogIdentity.AUTHORIZATION_SUCCESS, LogMessage.SuccessfulOperation);
                }
            }
            // Receive 1 byte => Authorization answer from client
            else if (receiveData.Length == 1)
            {
                if ((DisconnectReason) receiveData[0] == DisconnectReason.AuthorizationIsSuccess)
                {
                    // Set enable authorization
                    _core.GetAccountUtilitiesBySessionId(account.Session.SessionId).SessionHandler
                        .Core_AuthorizationClient();
                    // Set log
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
                        _core.Logging.LogEvent(
                            $"{LogMessage.AuthorizationSuccess}\n{account.Session.GetSessionInfo()}",
                            G9LogIdentity.AUTHORIZATION_SUCCESS, LogMessage.FailedOperation);
                }
                else
                {
                    OnDisconnectedHandler(account, (DisconnectReason) receiveData[0]);

                    if (_core.Logging.CheckLoggingIsActive(LogsType.ERROR))
                        // Set log
                        _core.Logging.LogError(
                            $"{LogMessage.AuthorizationFail}\n{LogMessage.Reason}: {((DisconnectReason) receiveData[0]).ToString()}\n{account.Session.GetSessionInfo()}",
                            G9LogIdentity.AUTHORIZATION_FAIL, LogMessage.FailedOperation);
                }
            }
            // Receive identity key from client
            else
            {
                if (!_core.EnableSslConnection)
                {
                    SendCommandByNameWithCustomPacketDataType(account.Session.SessionId,
                        nameof(G9ReservedCommandName.G9Authorization),
                        new[] {(byte) DisconnectReason.AuthorizationFailClientIsSslButServerWithoutSsl},
                        G9PacketDataType.Authorization, requestId);

                    if (_core.Logging.CheckLoggingIsActive(LogsType.ERROR))
                        // Set log
                        _core.Logging.LogError(
                            $"{LogMessage.AuthorizationFail}\n{LogMessage.Reason}: {DisconnectReason.AuthorizationFailClientIsSslButServerWithoutSsl.ToString()}\n{LogMessage.ServerWithOutCertificate}\n{account.Session.GetSessionInfo()}",
                            G9LogIdentity.AUTHORIZATION_FAIL, LogMessage.FailedOperation);
                }
                else
                {
                    SendCommandByNameWithCustomPacketDataType(account.Session.SessionId,
                        nameof(G9ReservedCommandName.G9Authorization),
                        _core.EncryptAndDecryptDataWithCertificate.GetCertificateByCertificateNumber(
                            account.Session.CertificateNumber,
                            requestId.ToByteArray().Concat(receiveData).ToArray().GenerateMd5()),
                        G9PacketDataType.Authorization, requestId);
                }
            }
        }

        #endregion

        #endregion
    }
}