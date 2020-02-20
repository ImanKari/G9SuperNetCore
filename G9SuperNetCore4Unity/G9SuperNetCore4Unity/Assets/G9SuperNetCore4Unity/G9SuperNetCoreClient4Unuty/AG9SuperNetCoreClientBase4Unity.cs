using System;
using System.Net.Sockets;
using System.Threading;
using G9Common.Abstract;
using G9Common.Enums;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Enums;
using G9SuperNetCoreClient.Helper;

// ReSharper disable once CheckNamespace
namespace G9SuperNetCoreClient.AbstractClient
{
    // ReSharper disable once InconsistentNaming
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        #region Internal Client Method

        /// <summary>
        ///     Receive call back
        ///     Handler for receive data
        /// </summary>
        /// <param name="asyncResult">Receive result from server</param>

        #region ReceiveCallback

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            Socket client = null;
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                _stateObject = (G9SuperNetCoreStateObjectClient) asyncResult.AsyncState;
                client = _stateObject.WorkSocket;

                // Read data from the remote device.  
                var bytesRead = client.EndReceive(asyncResult);
                // If don't receive byte return
                if (bytesRead <= 0) return;

                // Field for save send and receive packet
                G9SendAndReceivePacket receivePacket;

                // Specified packet is authorization
                var isAuthorizationCommand = !_mainAccountUtilities.Account.Session.IsAuthorization &&
                                             _stateObject.Buffer[1] == (byte) G9PacketDataType.Authorization;

                // in position 1 specified packet data type
                // if it's == Authorization, not encrypted
                if (isAuthorizationCommand)
                    // unpacking request - Decrypt data if need
                    receivePacket = _packetManagement.UnpackingRequestByData(_stateObject.Buffer);
                else
                    // unpacking request - Decrypt data if need
                    receivePacket = _packetManagement.UnpackingRequestByData(EnableSslConnection
                        ? _encryptAndDecryptDataWithCertificate.DecryptDataWithCertificate(_stateObject.Buffer, 0)
                        : _stateObject.Buffer);

                var receiveBytes = (ushort) _stateObject.Buffer.Length;

                // Plus receive bytes and packet
                TotalReceiveBytes += receiveBytes;
                TotalReceivePacket++;

                // Plus receive bytes and packet in session
                _mainAccountUtilities.SessionHandler.Core_PlusSessionTotalReceiveBytes(receiveBytes);

                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.INFO))
                    _logging.LogInformation(
                        $"{LogMessage.SuccessUnpackingReceiveData}\n{LogMessage.PacketType}: {receivePacket.PacketType.ToString()}\n{LogMessage.Command}: {receivePacket.Command}\n{LogMessage.Body}: '{Configuration.EncodingAndDecoding.GetString(receivePacket.Body)}'\n{LogMessage.PacketRequestId}: {receivePacket.RequestId}",
                        $"{G9LogIdentity.CLIENT_RECEIVE}", LogMessage.SuccessfulOperation);

                // Clear Data
                Array.Clear(_stateObject.Buffer, 0, _stateObject.Buffer.Length);

                // Handle receive packet
                HelperPacketHandler(ref receivePacket, ref isAuthorizationCommand);

                // Get the rest of the data.  
                client.BeginReceive(_stateObject.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                    ReceiveCallback, _stateObject);
            }
            catch (Exception ex)
            {
                #region Exception

                // Clear Data
                if (_stateObject != null)
                    Array.Clear(_stateObject.Buffer, 0, _stateObject.Buffer.Length);

                if (!_mainAccountUtilities.Account.Session.IsAuthorization || ex is SocketException exception && (exception.ErrorCode == 10054 || exception.ErrorCode == 10060))
                {
                    //Error 10060: 'An existing connection was forcibly closed by the remote host'
                    // A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond
                    // If IsAuthorization is false and receive exception => call disconnect
                    // Run event disconnect
                    OnDisconnectedHandler(_mainAccountUtilities.Account, DisconnectReason.DisconnectedFromServer);
                }
                else
                {
                    if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                        _logging.LogException(ex, LogMessage.FailClientReceive, G9LogIdentity.CLIENT_RECEIVE,
                            LogMessage.FailedOperation);

                    OnErrorHandler(ex, ClientErrorReason.ErrorInReceiveData);

                    if (ex.Message.Contains("Cannot access a disposed object."))
                        // Run event disconnect
                        OnDisconnectedHandler(_mainAccountUtilities.Account,
                            DisconnectReason.DisconnectedFromServer);
                    else
                        try
                        {
                            // Get the rest of the data.  
                            client?.BeginReceive(_stateObject.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                                ReceiveCallback, _stateObject);
                        }
                        catch
                        {
                            // Run event disconnect
                            OnDisconnectedHandler(_mainAccountUtilities.Account,
                                DisconnectReason.DisconnectedFromServer);
                        }
                }

                #endregion
            }
        }

        #endregion

        /// <summary>
        ///     Helper for handle receive packet
        /// </summary>
        /// <param name="receivePacket">Access to receive packet</param>
        /// <param name="isAuthorizationCommand">Specified command is authorization</param>

        #region HelperPacketHandler

        private void HelperPacketHandler(ref G9SendAndReceivePacket receivePacket, ref bool isAuthorizationCommand)
        {
            #region Handle single and multi package

            if (receivePacket.PacketType == G9PacketType.MultiPacket)
            {
                if (_stateObject.MultiPacketCollection.ContainsKey(receivePacket.RequestId))
                {
                    _stateObject.MultiPacketCollection[receivePacket.RequestId]
                        .AddPacket(
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                                    receivePacket.Body.Span[0], receivePacket.Body.ToArray()
#else
                            receivePacket.Body[0], receivePacket.Body
#endif
                        );
                    if (_stateObject.MultiPacketCollection[receivePacket.RequestId].FillAllPacket)
                    {
                        // Change request body
                        receivePacket.ChangePackageBodyByMultiPackage(
                            _stateObject.MultiPacketCollection[receivePacket.RequestId]);

                        // if authorization request => wait to finish progress
                        // Progress packet
                        _commandHandler.G9CallHandler(receivePacket, _mainAccountUtilities.Account,
                            isAuthorizationCommand);
                    }
                }
                else
                {
                    if (
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                                    receivePacket.Body.Span[0]
#else
                        receivePacket.Body[0]
#endif
                        == 0)
                    {
                        _stateObject.MultiPacketCollection.Add(receivePacket.RequestId,
                            new G9PacketSplitHandler(receivePacket.RequestId,
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                                    receivePacket.Body.Span[1]
#else
                                receivePacket.Body[1]
#endif
                            ));
                        _stateObject.MultiPacketCollection[receivePacket.RequestId]
                            .AddPacket(0,
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                                    receivePacket.Body.ToArray()
#else
                                receivePacket.Body
#endif
                            );
                    }
                }
            }
            else
            {
                // Progress packet
                _commandHandler.G9CallHandler(receivePacket, _mainAccountUtilities.Account);
            }

            #endregion
        }

        #endregion

        /// <summary>
        ///     Send data
        ///     Handle for send
        /// </summary>
        /// <param name="clientSocket">Specify client socket</param>
        /// <param name="data">Specify data for send</param>
        /// <param name="sendFoAuthorization">
        ///     <para>Specified send for authorization</para>
        ///     <para>If set true, no wait for authorization</para>
        /// </param>
        /// <returns>return WaitHandle for begin send</returns>

        #region Send

        private WaitHandle Send(Socket clientSocket,
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
            ReadOnlySpan<byte>
#else
            byte[]
#endif
                data, bool sendFoAuthorization = false)
        {
            // Wait for authorization
            while (!sendFoAuthorization && !_mainAccountUtilities.Account.Session.IsAuthorization)
                Thread.Sleep(9);

            // Set log
            if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                _logging.LogEvent($"{LogMessage.RequestSendData}\n{LogMessage.DataLength}: {data.Length}",
                    $"{G9LogIdentity.CLIENT_SEND_DATA}", LogMessage.SuccessfulOperation);

            // Specify array data for send
            var arrayDataForSend =
                    // in position 1 specified packet data type
                    // if it's == Authorization, not encrypted
                    !_mainAccountUtilities.Account.Session.IsAuthorization &&
                    data[1] == (byte) G9PacketDataType.Authorization
                        ?
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                     data.ToArray()
#else
                        data
#endif
                        // check enable or disable ssl connection for encrypt
                        : EnableSslConnection
                            ? _encryptAndDecryptDataWithCertificate.EncryptDataByCertificate(
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                     data.ToArray()
#else
                                data
#endif
                                , 0)
                            :
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                     data.ToArray()
#else
                            data
#endif
                ;

            // Begin sending the data to the remote device.  
            return clientSocket.BeginSend(arrayDataForSend, 0, arrayDataForSend.Length, 0, SendCallback, clientSocket)
                ?.AsyncWaitHandle;
        }

        #endregion

        #endregion
    }
}