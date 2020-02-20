using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using G9Common.Abstract;
using G9Common.CommandHandler;
using G9Common.Enums;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.PacketManagement;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Enums;
using G9SuperNetCoreClient.Helper;
using G9SuperNetCoreClient.Logging;

namespace G9SuperNetCoreClient.AbstractClient
{
    // ReSharper disable once InconsistentNaming
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        #region Internal Client Method

        /// <summary>
        ///     <para>Constructor</para>
        ///     <para>Initialize requirement And Set config</para>
        /// </summary>
        /// <param name="clientConfig">Specify client configuration</param>
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        /// <param name="customLogging">Specified custom logging system</param>
        /// <param name="privateKeyForSslConnection">
        ///     <para>
        ///         Notice: This is not a certificate password, it is a private, shared key between the client and the server for
        ///         secure connection (SSL)
        ///     </para>
        ///     <para>Specified custom private key</para>
        /// </param>
        /// <param name="clientUniqueIdentity">
        ///     <para>Specify a unique identity string from client</para>
        ///     <para>Used for ssl connection</para>
        /// </param>

        #region G9SuperNetCoreClientBase

        protected AG9SuperNetCoreClientBase(G9ClientConfig clientConfig, Assembly commandAssembly,
            IG9Logging customLogging, string privateKeyForSslConnection, string clientUniqueIdentity)
        {
            // Set logging system
            _logging = customLogging ?? new G9LoggingClient();

            // Set configuration
            Configuration = clientConfig;

            // Initialize main account utilities
            _mainAccountUtilities =
                new G9AccountUtilities<TAccount, G9ClientAccountHandler, G9ClientSessionHandler>
                {
                    Account = new TAccount()
                };

            // Initialize account and session
            var session = new TSession();
            session.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(_mainAccountUtilities.SessionHandler =
                new G9ClientSessionHandler
                {
                    // Set send command sync
                    Session_SendCommandByName = SendCommandByName,
                    // Set send command async
                    Session_SendCommandByNameAsync = SendCommandByNameAsync,
                    // Set session encoding
                    Session_GetSessionEncoding = () => Configuration.EncodingAndDecoding
                }, 0, IPAddress.Any);
            _mainAccountUtilities.Account.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(
                _mainAccountUtilities.AccountHandler = new G9ClientAccountHandler(), session);

            // Initialize packet management
            _packetManagement = new G9PacketManagement(Configuration.CommandSize, Configuration.BodySize,
                Configuration.EncodingAndDecoding, _logging);

            // Set packet size
            _packetSize = _packetManagement.MaximumPacketSize;

            // Initialize state object
            _stateObject =
                new G9SuperNetCoreStateObjectClient(_packetSize, _mainAccountUtilities.Account.Session.SessionId);

            // Set log
            if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                _logging.LogEvent(LogMessage.CreateAndInitializeClient, G9LogIdentity.CREATE_CLIENT,
                    LogMessage.SuccessfulOperation);

            // Initialize command handler
            _commandHandler = new G9CommandHandler<TAccount>(commandAssembly, _logging, Configuration.CommandSize,
                OnUnhandledCommandHandler);

            // ######################## Add default command ########################
            // G9 Echo Command
            _commandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9EchoCommand),
                G9EchoCommandReceiveHandler, null);
            // G9 Test Send Receive
            _commandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9TestSendReceive),
                G9TestSendReceiveCommandReceiveHandler, null);
            // G9 Ping Command
            _commandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9PingCommand),
                G9PingCommandReceiveHandler, null);
            // G9 Authorization Command
            _commandHandler.AddCustomCommand<byte[]>(nameof(G9ReservedCommandName.G9Authorization),
                AuthorizationReceiveHandler, null);

            // Set reconnect try count - use when client disconnected
            _reconnectTryCount = Configuration.ReconnectTryCount;

            // Set private key
            if (string.IsNullOrEmpty(privateKeyForSslConnection)) return;
            // Set private key
            _privateKey = privateKeyForSslConnection;
            // Set client unique identity
            _clientIdentity = string.IsNullOrEmpty(clientUniqueIdentity)
                ? _clientIdentity = Guid.NewGuid().ToString("N")
                : _clientIdentity = clientUniqueIdentity.Length < 16
                    ? clientUniqueIdentity + Guid.NewGuid().ToString("N")
                    : clientUniqueIdentity;
        }

        #region Constructor Overloads

        /// <summary>
        ///     <para>Constructor</para>
        ///     <para>Initialize requirement And Set config</para>
        /// </summary>
        /// <param name="clientConfig">Specify client configuration</param>
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        protected AG9SuperNetCoreClientBase(G9ClientConfig clientConfig, Assembly commandAssembly)
            : this(clientConfig, commandAssembly, null, null, null)
        {
        }

        /// <summary>
        ///     <para>Constructor</para>
        ///     <para>Initialize requirement And Set config</para>
        /// </summary>
        /// <param name="clientConfig">Specify client configuration</param>
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        /// <param name="customLogging">Specified custom logging system</param>
        protected AG9SuperNetCoreClientBase(G9ClientConfig clientConfig, Assembly commandAssembly,
            IG9Logging customLogging)
            : this(clientConfig, commandAssembly, customLogging, null, null)
        {
        }

        /// <summary>
        ///     <para>Constructor</para>
        ///     <para>Initialize requirement And Set config</para>
        /// </summary>
        /// <param name="clientConfig">Specify client configuration</param>
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        /// <param name="privateKeyForSslConnection">
        ///     <para>
        ///         Notice: This is not a certificate password, it is a private, shared key between the client and the server for
        ///         secure connection (SSL)
        ///     </para>
        ///     <para>Specified custom private key</para>
        /// </param>
        /// <param name="clientUniqueIdentity">
        ///     <para>Specify a unique identity string from client</para>
        ///     <para>Used for ssl connection</para>
        /// </param>
        protected AG9SuperNetCoreClientBase(G9ClientConfig clientConfig, Assembly commandAssembly,
            string privateKeyForSslConnection, string clientUniqueIdentity)
            : this(clientConfig, commandAssembly, null, privateKeyForSslConnection, clientUniqueIdentity)
        {
        }

        #endregion

        #endregion

        /// <summary>
        ///     Connection call back
        ///     Handle connection
        /// </summary>
        /// <param name="asyncResult">Receive call back result</param>

        #region ConnectCallback

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Retrieve the socket from the state object.  
                _clientSocket = (Socket) asyncResult.AsyncState;

                // Complete the connection.  
                _clientSocket.EndConnect(asyncResult);

                // Signal that the connection has been made.  
                _connectDone.Set();

                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                {
                    var ipEndPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
                    _logging.LogEvent(
                        $"{LogMessage.SuccessClientConnection}\n{LogMessage.IpAddress}: {ipEndPoint?.Address}\n{LogMessage.Port}: {ipEndPoint?.Port}",
                        G9LogIdentity.CLIENT_CONNECTED, LogMessage.SuccessfulOperation);
                }

                // Run event on connected
                OnConnectedHandler(_mainAccountUtilities.Account);

                // Listen for receive
                Receive(_clientSocket);

                // Set reconnect try count - use when client disconnected
                _reconnectTryCount = Configuration.ReconnectTryCount;

                // Call authorization
                SendCommandByNameAsyncWithCustomPacketDataType(nameof(G9ReservedCommandName.G9Authorization),
                    string.IsNullOrEmpty(_privateKey)
                        ? new byte[0]
                        : Configuration.EncodingAndDecoding.EncodingType.GetBytes(_clientIdentity),
                    G9PacketDataType.Authorization, isAuthorization: true);
            }
            catch (Exception ex)
            {
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailClinetConnection, G9LogIdentity.CLIENT_CONNECTED,
                        LogMessage.FailedOperation);
                // Run event on connected error
                OnErrorHandler(ex, ClientErrorReason.ClientConnectedError, true);
            }
        }

        #endregion

        /// <summary>
        ///     Receive call back
        ///     Handler receive data
        /// </summary>
        /// <param name="clientSocket"></param>

        #region Receive

        private void Receive(Socket clientSocket)
        {
            try
            {
                _stateObject.WorkSocket = clientSocket;

                // Begin receiving the data from the remote device.  
                clientSocket.BeginReceive(_stateObject.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                    ReceiveCallback, _stateObject);

                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                    _logging.LogEvent($"{LogMessage.ReceiveClientFromServer}", G9LogIdentity.CLIENT_RECEIVE,
                        LogMessage.SuccessfulOperation);
            }
            catch (Exception e)
            {
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(e, LogMessage.FailClientReceive, G9LogIdentity.CLIENT_RECEIVE,
                        LogMessage.FailedOperation);

                OnErrorHandler(e, ClientErrorReason.ErrorInReceiveData);
            }
        }

        #endregion

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
                    // Error 10060: 'An existing connection was forcibly closed by the remote host'
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

        /// <summary>
        ///     Send call back
        ///     Handle call back after send
        /// </summary>
        /// <param name="asyncResult"></param>

        #region SendCallback

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var client = (Socket) asyncResult.AsyncState;

                // Complete sending the data to the remote device.  
                var bytesSent = (ushort) client.EndSend(asyncResult);

                // Signal that all bytes have been sent.  
                _sendDone.Set();

                // Plus send bytes and packet
                TotalSendBytes += bytesSent;
                TotalSendPacket++;

                // Plus send bytes and packet in session
                _mainAccountUtilities.SessionHandler.Core_PlusSessionTotalSendBytes(bytesSent);

                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.INFO))
                    _logging.LogInformation(
                        $"{LogMessage.SuccessRequestSendData}\n{LogMessage.DataLength}: {bytesSent}",
                        G9LogIdentity.CLIENT_SEND_DATA, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailRequestSendData, G9LogIdentity.CLIENT_SEND_DATA,
                        LogMessage.FailedOperation);

                OnErrorHandler(ex, ClientErrorReason.ErrorSendDataToServer);
            }
        }

        #endregion

        #endregion
    }
}