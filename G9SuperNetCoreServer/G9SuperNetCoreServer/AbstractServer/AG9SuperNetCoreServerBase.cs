using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreCommon.HelperClass;
using G9SuperNetCoreCommon.Interface;
using G9SuperNetCoreCommon.LogIdentity;
using G9SuperNetCoreCommon.Packet;
using G9SuperNetCoreCommon.PacketManagement;
using G9SuperNetCoreCommon.Resource;
using G9SuperNetCoreCommon.ServerClient;
using G9LogManagement.Enums;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Config;
using G9SuperNetCoreServer.Core;
using G9SuperNetCoreServer.Enums;
using G9SuperNetCoreServer.HelperClass;

namespace G9SuperNetCoreServer.AbstractServer
{
    // ReSharper disable once InconsistentNaming
    public abstract partial class AG9SuperNetCoreServerBase<TAccount, TSession> : AG9ServerClientCommon<TAccount> 
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        #region Internal Server Methods

        /// <summary>
        ///     Constructor
        ///     Initialize Requirement
        /// </summary>
        /// <param name="superNetCoreConfig">Server config</param>
        /// <param name="commandAssemblies">Specified command assembly (find command in specified assembly)</param>
        /// <param name="customLogging">Specified custom logging system</param>
        /// <param name="sslCertificate">Specified object of G9SslCertificate for manage ssl connection</param>

        #region G9SuperNetCoreServerBase

        protected AG9SuperNetCoreServerBase(G9ServerConfig superNetCoreConfig, Assembly[] commandAssemblies = null,
            IG9Logging customLogging = null, G9SslCertificate sslCertificate = null)
        {
            // Set assemblies
            commandAssemblies ??= AppDomain.CurrentDomain.GetAssemblies();

            // Initialize core
            _core = new G9Core<TAccount, TSession>(superNetCoreConfig, commandAssemblies, SendCommandByName,
                SendCommandByNameAsync, OnSessionReceiveRequestOverTheLimitInSecondHandler, OnUnhandledCommandHandler,
                OnDisconnectedHandler,
                customLogging, sslCertificate);

            // Set command handler call back
            CommandHandlerCallback = _core.CommandHandler;

            // ######################## Add default command ########################
            // G9 Echo Command
            _core.CommandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9EchoCommand),
                G9EchoCommandReceiveHandler, null);
            // G9 Test Send Receive
            _core.CommandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9TestSendReceive),
                G9TestSendReceiveCommandReceiveHandler, null);
            // G9 Ping Command
            _core.CommandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9PingCommand),
                G9PingCommandReceiveHandler, null);
            // G9 Ping Command
            _core.CommandHandler.AddCustomCommand<byte[]>(nameof(G9ReservedCommandName.G9Authorization),
                AuthorizationReceiveHandler, null);

            // Initialize packet management
            _packetManagement = new G9PacketManagement(_core.Configuration.CommandSize, _core.Configuration.BodySize,
                _core.Configuration.EncodingAndDecoding, _core.Logging);

            // Set packet size
            _packetSize = _core.EnableSslConnection
                ? _packetManagement.MaximumPacketSizeInSslMode()
                : _packetManagement.MaximumPacketSize;

            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
                _core.Logging.LogEvent(LogMessage.CreateAndInitializeServer, G9LogIdentity.CREATE_SERVER,
                    LogMessage.SuccessfulOperation);
        }

        #endregion

        /// <summary>
        ///     Accept call back
        ///     connection handler
        /// </summary>
        /// <param name="asyncResult">received connection</param>

        #region AcceptCallback

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            uint sessionId = 0;
            try
            {
                // Signal the main thread to continue.  
                _listenerAcceptManualResetEvent.Set();

                // Get the socket that handles the client request.  
                var listener = (Socket) asyncResult.AsyncState;
                var handler = listener.EndAccept(asyncResult);

                // Try to accept connection
                var (accept, account) = _core.TryAcceptConnection(handler);

                // Return if not accepted
                if (!accept) return;

                // Create the state object for receive 
                var stateReceive = new G9SuperNetCoreStateObjectServer(_packetSize, account.Session.SessionId)
                {
                    WorkSocket = handler
                };
                sessionId = stateReceive.SessionIdentity;

                // Run event on connected
                OnConnectedHandler(account);

                // Ready for begin receive
                handler.BeginReceive(stateReceive.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                    ReadCallback, stateReceive);

                // Set log
                if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
                    _core.Logging.LogEvent(
                        $"{LogMessage.SuccessClientConnection}\n{LogMessage.BufferSize}: {_packetManagement.MaximumPacketSize}\n{LogMessage.ClientSessionIdentity}: {sessionId}",
                        G9LogIdentity.SERVER_ACCEPT_CALLBACK, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex,
                        $"{LogMessage.FailClinetConnection}\n{LogMessage.ClientSessionIdentity}: {(sessionId == 0 ? "NONE" : sessionId.ToString())}",
                        G9LogIdentity.SERVER_ACCEPT_CALLBACK, LogMessage.FailedOperation);

                // Run event on connected error
                OnErrorHandler(ex, ServerErrorReason.ClientConnectedError);
            }
        }

        #endregion

        /// <summary>
        ///     Read call back
        ///     Handle receive data
        /// </summary>
        /// <param name="asyncResult">receive result</param>

        #region ReadCallback

        private void ReadCallback(IAsyncResult asyncResult)
        {
            // Specify session id
            uint sessionId = 0;
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            var state = (G9SuperNetCoreStateObjectServer) asyncResult.AsyncState;
            try
            {
                sessionId = state.SessionIdentity;

                var accountUtilities = _core.GetAccountUtilitiesBySessionId(sessionId);

                // When account is null => return
                if (accountUtilities is null) return;

                // Read data from the client socket.   
                var bytesRead = (ushort) state.WorkSocket.EndReceive(asyncResult);

                if (bytesRead > 0)
                {
                    // Plus total receive bytes and packet
                    TotalReceiveBytes += bytesRead;
                    TotalReceivePacket++;

                    // Initialize receive packet
                    HelperReceivePacket(out var receivePacket, ref accountUtilities, ref state);

                    // Plus receive bytes for session
                    accountUtilities.SessionHandler.Core_PlusSessionTotalReceiveBytes(bytesRead);

                    // Set log
                    if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                        _core.Logging.LogInformation(
                            $"{LogMessage.SuccessUnpackingReceiveData}\n{LogMessage.PacketType}: {receivePacket.PacketType.ToString()}\n{LogMessage.Command}: {receivePacket.Command}\n{LogMessage.Body}: '{_core.Configuration.EncodingAndDecoding.GetString(receivePacket.Body)}'\n{LogMessage.PacketRequestId}: {receivePacket.RequestId}",
                            G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.SuccessfulOperation);

                    // Clear Data
                    Array.Clear(state.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize);

                    // Handle receive packet
                    HelperPacketHandler(ref receivePacket, ref accountUtilities, ref state);

                    // Listen and get other packet
                    state.WorkSocket.BeginReceive(
                        state.Buffer,
                        0,
                        AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                        ReadCallback, state);
                }

                // Set log
                if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
                    _core.Logging.LogEvent(
                        $"{LogMessage.SuccessClientReceive}\n{LogMessage.DataLength}: {bytesRead}\n{LogMessage.ClientSessionIdentity}: {sessionId}",
                        G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                #region Exception

                if (state?.Buffer != null)
                    // Clear Data
                    Array.Clear(state.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize);

                if (ex is SocketException exception && (exception.ErrorCode == 10054 || exception.ErrorCode == 10060))
                {
                    //Error 10060: 'An existing connection was forcibly closed by the remote host'
                    // A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond
                    // Run event disconnect
                    OnDisconnectedHandler(_core.GetAccountUtilitiesBySessionId(sessionId).Account,
                        DisconnectReason.DisconnectedFromClient);
                }
                else
                {
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex,
                            $"{LogMessage.FailClinetConnection}\n{LogMessage.ClientSessionIdentity}: {(sessionId == 0 ? "NONE" : sessionId.ToString())}",
                            G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.FailedOperation);

                    // Run event on connected error
                    OnErrorHandler(ex, ServerErrorReason.ErrorReceiveDataFromClient);

                    try
                    {
                        // Listen and get other packet
                        state?.WorkSocket.BeginReceive(
                            state.Buffer ??
                            throw new InvalidOperationException(
                                "state.Buffer is null for BeginReceive in block finally!"),
                            0,
                            AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                            ReadCallback, state);
                    }
                    catch
                    {
                        // Reject
                        // Run event disconnect
                        OnDisconnectedHandler(_core.GetAccountUtilitiesBySessionId(sessionId)?.Account,
                            DisconnectReason.DisconnectedFromClient);
                    }
                }

                #endregion
            }
        }

        #endregion

        /// <summary>
        ///     Helper for receive packet data
        /// </summary>
        /// <param name="receivePacket">Field for save receive packet</param>
        /// <param name="accountUtilities">Access to account utilities</param>
        /// <param name="state">Access to state</param>

        #region HelperReceivePacket

        private void HelperReceivePacket(out G9SendAndReceivePacket receivePacket,
            ref G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler> accountUtilities,
            ref G9SuperNetCoreStateObjectServer state)
        {
            // in position 1 specified packet data type
            // if it's == Authorization, not encrypted
            if (!accountUtilities.Account.Session.IsAuthorization &&
                state.Buffer[1] == (byte) G9PacketDataType.Authorization)
                // unpacking request - Decrypt data if need
            {
                receivePacket = _packetManagement.UnpackingRequestByData(state.Buffer);
            }
            else
            {
                // unpacking request - Decrypt data if need
                receivePacket = _packetManagement.UnpackingRequestByData(_core.EnableSslConnection
                    ? _core.EncryptAndDecryptDataWithCertificate.DecryptDataWithCertificate(state.Buffer,
                        accountUtilities.Account.Session.CertificateNumber)
                    : state.Buffer);

                // Ignore for auth command
                // Set last command (check ping automatically when set last command)
                accountUtilities.SessionHandler.Core_SetLastCommand(receivePacket.Command);
            }
        }

        #endregion


        /// <summary>
        ///     Helper for handle receive packet
        /// </summary>
        /// <param name="receivePacket">Access to receive packet</param>
        /// <param name="accountUtilities">Access to account utilities</param>
        /// <param name="state">Access to state</param>

        #region HelperPacketHandler

        private void HelperPacketHandler(ref G9SendAndReceivePacket receivePacket,
            ref G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler> accountUtilities,
            ref G9SuperNetCoreStateObjectServer state)
        {
            #region Handle single and multi package

            if (receivePacket.PacketType == G9PacketType.MultiPacket)
            {
                if (state.MultiPacketCollection.ContainsKey(receivePacket.RequestId))
                {
                    state.MultiPacketCollection[receivePacket.RequestId]
                        .AddPacket(
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                            receivePacket.Body.Span[0], receivePacket.Body.ToArray()
#else
                                    receivePacket.Body[0], receivePacket.Body
#endif
                        );
                    if (state.MultiPacketCollection[receivePacket.RequestId].FillAllPacket)
                    {
                        // Change request body
                        receivePacket.ChangePackageBodyByMultiPackage(
                            state.MultiPacketCollection[receivePacket.RequestId]);

                        // if authorization request => wait to finish progress
                        // Progress packet
                        _core.CommandHandler.G9CallHandler(receivePacket, accountUtilities.Account);
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
                        state.MultiPacketCollection.Add(receivePacket.RequestId,
                            new G9PacketSplitHandler(receivePacket.RequestId,
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                                receivePacket.Body.Span[1]
#else
                                        receivePacket.Body[1]
#endif
                            ));
                        state.MultiPacketCollection[receivePacket.RequestId]
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
                _core.CommandHandler.G9CallHandler(receivePacket, accountUtilities.Account);
            }

            #endregion
        }

        #endregion

        /// <summary>
        ///     Send data to client
        /// </summary>
        /// <param name="handler">Socket handler for send</param>
        /// <param name="account">Specified account</param>
        /// <param name="byteData">Specify byte data for send</param>
        /// <returns>return WaitHandle for begin send</returns>

        #region Send

        private WaitHandle Send(Socket handler, TAccount account, byte[] byteData)
        {
            // Create the state object for sent
            var stateSend =
                new G9SuperNetCoreStateObjectServer(_packetSize, account.Session.SessionId)
                {
                    WorkSocket = handler,
                    // Set buffer for send - Encrypt if need
                    Buffer =
                        // in position 1 specified packet data type
                        // if it's == Authorization, not encrypted
                        !account.Session.IsAuthorization && byteData[1] == (byte) G9PacketDataType.Authorization
                            ? byteData
                            // check enable or disable ssl connection for encrypt
                            : _core.EnableSslConnection
                                ? _core.EncryptAndDecryptDataWithCertificate.EncryptDataByCertificate(byteData,
                                    account.Session.CertificateNumber)
                                : byteData
                };

            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
                _core.Logging.LogEvent($"{LogMessage.RequestSendData}\n{LogMessage.DataLength}: {byteData.Length}",
                    G9LogIdentity.SERVER_SEND_DATA, LogMessage.SuccessfulOperation);

            // Begin sending the data to the remote device.  
            return handler.BeginSend(stateSend.Buffer, 0, stateSend.Buffer.Length, 0,
                    SendCallback, stateSend)
                ?.AsyncWaitHandle;
        }

        #endregion

        /// <summary>
        ///     Send call back
        ///     When send success
        /// </summary>
        /// <param name="asyncResult">Send result</param>

        #region SendCallback

        private void SendCallback(IAsyncResult asyncResult)
        {
            // Retrieve the socket from the state object.  
            var state = (G9SuperNetCoreStateObjectServer) asyncResult.AsyncState;

            try
            {
                // Complete sending the data to the remote device.  
                var bytesSent = (ushort) state.WorkSocket.EndSend(asyncResult);

                // Plus send bytes for session
                _core.GetAccountUtilitiesBySessionId(state.SessionIdentity).SessionHandler
                    .Core_PlusSessionTotalSendBytes(bytesSent);

                // Plus total send bytes and packet
                TotalSendBytes += bytesSent;
                TotalSendPacket++;

                // Set log
                if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                    _core.Logging.LogInformation(
                        $"{LogMessage.SuccessRequestSendData}\n{LogMessage.DataLength}: {bytesSent}",
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailRequestSendData, G9LogIdentity.SERVER_SEND_DATA,
                        LogMessage.FailedOperation);

                // Run event on connected error
                OnErrorHandler(ex, ServerErrorReason.ErrorSendDataToClient);
            }
        }

        #endregion

        #endregion
    }
}