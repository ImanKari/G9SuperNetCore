using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using G9Common.Abstract;
using G9Common.CommandHandler;
using G9Common.Enums;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.LogIdentity;
using G9Common.PacketManagement;
using G9Common.Resource;
using G9Common.ServerClient;
using G9LogManagement.Enums;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Enums;
using G9SuperNetCoreClient.Helper;
using G9SuperNetCoreClient.Logging;

// ReSharper disable once CheckNamespace
namespace G9SuperNetCoreClient.AbstractClient
{
    // ReSharper disable once InconsistentNaming
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession> : AG9ServerClientCommon<TAccount>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        #region Internal Client Method

        /// <summary>
        ///     <para>Constructor</para>
        ///     <para>Initialize requirement And Set config</para>
        /// </summary>
        /// <param name="clientConfig">Specify client configuration</param>
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
        /// <param name="commandAssemblies">
        ///     <para>Specified command assemblies (find command in specified assembly)</para>
        ///     <para>If set null, mean all assembly access</para>
        /// </param>
        /// <param name="customAccount">If need set account with custom initialized account, Set it.</param>
        /// <param name="customSession">If need set session with custom initialized session, Set it.</param>

        #region G9SuperNetCoreClientBase

        protected AG9SuperNetCoreClientBase(G9ClientConfig clientConfig, IG9Logging customLogging = null,
            string privateKeyForSslConnection = null, string clientUniqueIdentity = null, Assembly[] commandAssemblies = null,
            TAccount customAccount = null, TSession customSession = null)
        {
            // Set command assemblies
            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
            commandAssemblies = commandAssemblies ?? AppDomain.CurrentDomain.GetAssemblies();

            // Set logging system
            _logging = customLogging ?? new G9LoggingClient();

            // Set configuration
            Configuration = clientConfig;

            // Initialize main account utilities
            _mainAccountUtilities =
                new G9AccountUtilities<TAccount, G9ClientAccountHandler, G9ClientSessionHandler>
                {
                    Account = customAccount ?? new TAccount()
                };

            // Initialize account and session
            var session = customSession ?? new TSession();
            session.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(_mainAccountUtilities.SessionHandler =
                new G9ClientSessionHandler
                {
                    // Set send command sync
                    Session_SendCommandByName = SendCommandByName,
                    // Set send command async
                    Session_SendCommandByNameAsync = SendCommandByNameAsync,
                    // Set session encoding
                    Session_GetSessionEncoding = () => Configuration.EncodingAndDecoding,
                    // Set account 
                    Core_SetAccount = () => _mainAccountUtilities.Account
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
            _commandHandler = new G9CommandHandler<TAccount>(commandAssemblies, _logging, Configuration.CommandSize,
                OnUnhandledCommandHandler);

            // Set command call back
            CommandHandlerCallback = _commandHandler;

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

                // Run event on connected
                OnConnectedHandler(_mainAccountUtilities.Account);

                // Listen for receive
                Receive(_clientSocket);

                if (IsSocketConnected(_clientSocket))
                {
                    // Set log
                    if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                    {
                        var ipEndPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
                        _logging.LogEvent(
                            $"{LogMessage.SuccessClientConnection}\n{LogMessage.IpAddress}: {ipEndPoint?.Address}\n{LogMessage.Port}: {ipEndPoint?.Port}",
                            G9LogIdentity.CLIENT_CONNECTED, LogMessage.SuccessfulOperation);
                    }

                    // Set reconnect try count - use when client disconnected
                    _reconnectTryCount = Configuration.ReconnectTryCount;

                    // Signal that the connection has been made.  
                    _connectDone.Set();

                    // Call authorization
                    SendCommandByNameAsyncWithCustomPacketDataType(nameof(G9ReservedCommandName.G9Authorization),
                        string.IsNullOrEmpty(_privateKey)
                            ? new byte[0]
                            : Configuration.EncodingAndDecoding.EncodingType.GetBytes(_clientIdentity),
                        G9PacketDataType.Authorization, isAuthorization: true);
                }
                else
                {
                    throw new Exception("Client can't connect to server!");
                }
            }
            catch (Exception ex)
            {
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailClinetConnection, G9LogIdentity.CLIENT_CONNECTED,
                        LogMessage.FailedOperation);
                // Run event on connected error
                OnErrorHandler(ex, ClientErrorReason.ClientConnectedError);
            }
        }

        #endregion

        /// <summary>
        ///     Check socket is connected
        /// </summary>
        /// <param name="socketForCheck">Socket for check connected</param>
        /// <returns>Return true if connected</returns>

        #region IsSocketConnected

        private bool IsSocketConnected(Socket socketForCheck)
        {
            try
            {
                // Check socket is connected
                return socketForCheck != null && !(socketForCheck.Poll(1000, SelectMode.SelectRead) &&
                                                   socketForCheck.Available == 0 ||
                                                   !socketForCheck.Connected);
            }
            catch
            {
                // Ignore
                return false;
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