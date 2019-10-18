using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using G9Common.Abstract;
using G9Common.Enums;
using G9Common.Interface;
using G9Common.JsonHelper;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.PacketManagement;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Config;
using G9SuperNetCoreServer.Core;
using G9SuperNetCoreServer.Enums;
using G9SuperNetCoreServer.HelperClass;

namespace G9SuperNetCoreServer.AbstractServer
{
    public abstract partial class AG9SuperNetCoreServerBase<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        #region Fields And Properties

        /// <summary>
        ///     Access to core
        /// </summary>
        private readonly G9Core<TAccount, TSession> _core;

        /// <summary>
        ///     Specify main socket listener for server
        /// </summary>
        private Socket _mainSocketListener;

        /// <summary>
        ///     Use thread signal.
        /// </summary>
        private readonly ManualResetEvent _listenerAcceptManualResetEvent = new ManualResetEvent(false);

        /// <summary>
        ///     Access to packet management
        /// </summary>
        private readonly G9PacketManagement _packetManagement;

        #region Send And Receive Bytes

        /// <summary>
        ///     Save total send bytes
        /// </summary>
        private long _totalSendBytes;

        /// <summary>
        ///     Access to total send bytes
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public long TotalSendBytes => _totalSendBytes;

        /// <summary>
        ///     Save total receive bytes
        /// </summary>
        private long _totalReceiveBytes;

        /// <summary>
        ///     Access to total receive bytes
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public long TotalReceiveBytes => _totalReceiveBytes;

        #endregion

        #endregion

        #region Methods

        #region Internal Server Methods

        /// <summary>
        ///     Constructor
        ///     Initialize Requirement
        /// </summary>
        /// <param name="superNetCoreConfig">Server config</param>
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        /// <param name="customLogging">Specified custom logging system</param>

        #region G9SuperNetCoreServerBase

        protected AG9SuperNetCoreServerBase(G9ServerConfig superNetCoreConfig, Assembly commandAssembly,
            IG9Logging customLogging = null)
        {
            // Initialize core
            _core = new G9Core<TAccount, TSession>(superNetCoreConfig, commandAssembly, SendCommandByName,
                SendCommandByNameAsync, OnUnhandledCommandHandler, customLogging);

            // ######################## Add default command ########################
            // G9 Echo Command
            _core.CommandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9EchoCommand),
                G9EchoCommandPingCommandReceiveHandler, null);
            // G9 Test Send Receive
            _core.CommandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9TestSendReceive),
                G9TestSendReceiveCommandReceiveHandler, null);
            // G9 Ping Command
            _core.CommandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9PingCommand),
                PingCommandReceiveHandler, null);

            // Initialize packet management
            _packetManagement = new G9PacketManagement(_core.Configuration.CommandSize, _core.Configuration.BodySize,
                _core.Configuration.EncodingAndDecoding, _core.Logging);

            // Set log
            if (_core.Logging.LogIsActive(LogsType.EVENT))
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
            long sessionId = -1;
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

                // Create the state object.  
                var state = new G9SuperNetCoreStateObjectServer(_packetManagement.MaximumPacketSize,
                    account.Session.SessionId)
                {
                    WorkSocket = handler
                };
                sessionId = state.SessionIdentity;

                // Run event on connected
                OnConnectedHandler(account);

                // Ready for begin receive
                handler.BeginReceive(state.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                    ReadCallback, state);

                // Set log
                if (_core.Logging.LogIsActive(LogsType.EVENT))
                    _core.Logging.LogEvent(
                        $"{LogMessage.SuccessClientConnection}\n{LogMessage.BufferSize}: {_packetManagement.MaximumPacketSize}\n{LogMessage.ClientSessionIdentity}: {sessionId}",
                        G9LogIdentity.SERVER_ACCEPT_CALLBACK, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex,
                        $"{LogMessage.FailClinetConnection}\n{LogMessage.ClientSessionIdentity}: {(sessionId == -1 ? "NONE" : sessionId.ToString())}",
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
            long sessionId = -1;
            try
            {
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                var state = (G9SuperNetCoreStateObjectServer) asyncResult.AsyncState;
                var handler = state.WorkSocket;
                sessionId = state.SessionIdentity;

                var accountUtilities = _core.GetAccountUtilitiesBySessionId(sessionId);

                // Read data from the client socket.   
                var bytesRead = handler.EndReceive(asyncResult);

                if (bytesRead > 0)
                {
                    // Use like span
                    ReadOnlySpan<byte> packet = state.Buffer;

                    // Plus total receive
                    _totalReceiveBytes += packet.Length;

                    // unpacking request
                    var receivePacket = _packetManagement.UnpackingRequestByData(packet);

                    // Set last command (check ping automatically when set last command)
                    accountUtilities.SessionHandler.SetLastCommand(receivePacket.Command);

                    // Set log
                    if (_core.Logging.LogIsActive(LogsType.INFO))
                        _core.Logging.LogInformation(
                            $"{LogMessage.SuccessUnpackingReceiveData}\n{LogMessage.PacketType}: {receivePacket.TypeOfPacketType.ToString()}\n{LogMessage.Command}: {receivePacket.Command}\n{LogMessage.Body}: '{_core.Configuration.EncodingAndDecoding.EncodingType.GetString(receivePacket.Body.ToArray())}'\n{LogMessage.PacketRequestId}: {receivePacket.RequestId}",
                            G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.SuccessfulOperation);

                    // Clear Data
                    Array.Clear(state.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize);

                    // Progress packet
                    _core.CommandHandler.G9CallHandler(receivePacket,
                        _core.GetAccountUtilitiesBySessionId(sessionId).Account);
                }

                // Listen and get other packet
                handler.BeginReceive(state.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                    ReadCallback, state);

                // Set log
                if (_core.Logging.LogIsActive(LogsType.EVENT))
                    _core.Logging.LogEvent(
                        $"{LogMessage.SuccessClientReceive}\n{LogMessage.DataLength}: {bytesRead}\n{LogMessage.ClientSessionIdentity}: {sessionId}",
                        G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (ex is SocketException exception && exception.ErrorCode == 10054)
                {
                    // Run event disconnect
                    OnDisconnectedHandler(_core.GetAccountUtilitiesBySessionId(sessionId).Account,
                        DisconnectReason.DisconnectedFromClient);
                }
                else
                {
                    if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex,
                            $"{LogMessage.FailClinetConnection}\n{LogMessage.ClientSessionIdentity}: {(sessionId == -1 ? "NONE" : sessionId.ToString())}",
                            G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.FailedOperation);

                    // Run event on connected error
                    OnErrorHandler(ex, ServerErrorReason.ErrorReceiveDataFromClient);
                }
            }
        }

        #endregion

        /// <summary>
        ///     Send data to client
        /// </summary>
        /// <param name="handler">Socket handler for send</param>
        /// <param name="byteData">Specify byte data for send</param>

        #region Send

        private void Send(Socket handler, ReadOnlySpan<byte> byteData)
        {
            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData.ToArray(), 0, byteData.Length, 0,
                SendCallback, handler);

            // Set log
            if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                _core.Logging.LogEvent($"{LogMessage.RequestSendData}\n{LogMessage.DataLength}: {byteData.Length}",
                    G9LogIdentity.SERVER_SEND_DATA, LogMessage.SuccessfulOperation);
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
            try
            {
                // Retrieve the socket from the state object.  
                var handler = (Socket) asyncResult.AsyncState;

                // Complete sending the data to the remote device.  
                var bytesSent = handler.EndSend(asyncResult);

                // Plus total send
                _totalSendBytes += bytesSent;

                // Set log
                if (_core.Logging.LogIsActive(LogsType.INFO))
                    _core.Logging.LogInformation(
                        $"{LogMessage.SuccessRequestSendData}\n{LogMessage.DataLength}: {bytesSent}",
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailRequestSendData, G9LogIdentity.SERVER_SEND_DATA,
                        LogMessage.FailedOperation);

                // Run event on connected error
                OnErrorHandler(ex, ServerErrorReason.ErrorSendDataToClient);
            }
        }

        #endregion

        #endregion

        #region Other Methods

        /// <summary>
        ///     Start server
        ///     Ready listen connection and accept request
        /// </summary>

        #region Start

        public async Task Start()
        {
            // Set log
            if (_core.Logging.LogIsActive(LogsType.EVENT))
                _core.Logging.LogEvent(LogMessage.StartServer, G9LogIdentity.START_SERVER,
                    LogMessage.SuccessfulOperation);

            // Run task
            await Task.Run(() =>
            {
                // Establish the local endpoint for the socket.  
                var localEndPoint = new IPEndPoint(_core.Configuration.IpAddress, _core.Configuration.PortNumber);

                try
                {
                    // Create a TCP/IP socket.  
                    _mainSocketListener = new Socket(_core.Configuration.IpAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                    // Set Log
                    if (_core.Logging.LogIsActive(LogsType.EVENT))
                        _core.Logging.LogEvent(
                            $"{LogMessage.SuccessCreateServerSocket}\n{LogMessage.IpAddress}: {_core.Configuration.IpAddress}\n{LogMessage.Port}: {_core.Configuration.PortNumber}",
                            G9LogIdentity.CREATE_LISTENER, LogMessage.SuccessfulOperation);
                }
                catch (Exception ex)
                {
                    if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex, LogMessage.ProblemCreateServerSocket,
                            G9LogIdentity.CREATE_LISTENER, LogMessage.FailedOperation);
                    OnErrorHandler(ex, ServerErrorReason.ErrorInStartServer);
                    return;
                }

                try
                {
                    // Start bind
                    _mainSocketListener.Bind(localEndPoint);
                    _mainSocketListener.Listen(_core.Configuration.MaxConnectionNumber);

                    //Set Log
                    if (_core.Logging.LogIsActive(LogsType.INFO))
                        _core.Logging.LogInformation(LogMessage.SuccessBindAndListenSocket, G9LogIdentity.BIND_LISTENER,
                            LogMessage.SuccessfulOperation);
                }
                catch (Exception ex)
                {
                    if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex, LogMessage.FailBindAndListenSocket, G9LogIdentity.BIND_LISTENER,
                            LogMessage.FailedOperation);
                    OnErrorHandler(ex, ServerErrorReason.ErrorInStartServer);
                    return;
                }

                // Set on start requirement
                OnStartHandler();

                // Bind the socket to the local endpoint and listen for incoming connections.  
                // Infinity loop for binding connection
                while (IsStarted)
                    try
                    {
                        // Set the event to nonsignaled state.  
                        _listenerAcceptManualResetEvent.Reset();

                        // Set log
                        if (_core.Logging.LogIsActive(LogsType.INFO))
                            _core.Logging.LogInformation(LogMessage.WaitForConnection, G9LogIdentity.WAIT_LISTENER,
                                LogMessage.Waiting);

                        // Start an asynchronous socket to listen for connections.  
                        _mainSocketListener.BeginAccept(
                            AcceptCallback,
                            _mainSocketListener);

                        // Wait until a connection is made before continuing.  
                        _listenerAcceptManualResetEvent.WaitOne();
                    }
                    catch (Exception ex)
                    {
                        if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                            _core.Logging.LogException(ex, LogMessage.FailtOnWaitForConnection,
                                G9LogIdentity.WAIT_LISTENER, LogMessage.FailedOperation);
                        OnErrorHandler(ex, ServerErrorReason.ClientConnectedError);
                    }
            });
        }

        #endregion

        /// <summary>
        ///     Stop server
        ///     Releases all resources used by server
        /// </summary>

        #region Stop

        public async Task<bool> Stop()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (_mainSocketListener is null)
                    {
                        // Set log
                        if (_core.Logging.LogIsActive(LogsType.ERROR))
                            _core.Logging.LogError(LogMessage.CantStopStoppedServer,
                                G9LogIdentity.STOP_SERVER, LogMessage.FailedOperation);
                        // Run event
                        OnErrorHandler(new Exception(LogMessage.CantStopStoppedServer),
                            ServerErrorReason.ServerIsStoppedAndReceiveRequestForStop);
                        return false;
                    }

                    // Run event stop
                    OnStopHandler(ServerStopReason.StopWithOperator);

                    _core.ScrollingAllAccountUtilities(s => { s.SessionSocket.Dispose(); });

                    // Close, Disconnect and dispose
                    _mainSocketListener.Dispose();
                    _mainSocketListener = null;

                    // Set log
                    if (_core.Logging.LogIsActive(LogsType.EVENT))
                        _core.Logging.LogEvent(LogMessage.StopServer, G9LogIdentity.STOP_SERVER,
                            LogMessage.SuccessfulOperation);

                    // Call gc collect
                    GC.Collect();

                    return true;
                }
                catch (Exception ex)
                {
                    // Set log
                    if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex, LogMessage.FailStopServer,
                            G9LogIdentity.STOP_SERVER, LogMessage.FailedOperation);
                    // Run event
                    OnErrorHandler(ex, ServerErrorReason.ErrorInStopServer);

                    return false;
                }
            });
        }

        #endregion

        #region Test Mode Methods

        /// <summary>
        ///     Enable command test send and receive for all clients
        /// </summary>
        /// <param name="testMessage">
        ///     Test message
        ///     If set null => $"Test Mode - Session Id: {SessionId}"
        /// </param>

        #region EnableCommandTestSendAndReceiveForAllClients

        public void EnableCommandTestSendAndReceiveForAllClients(string testMessage = null)
        {
            _core.ScrollingAllAccountUtilities(s => s.SessionHandler.EnableTestMode(testMessage));

            // Set log
            if (_core.Logging.LogIsActive(LogsType.EVENT))
                _core.Logging.LogEvent(LogMessage.EnableCommandTestModeForAllClients,
                    G9LogIdentity.ENABLE_TEST_MODE_ALL_CLIENT, LogMessage.SuccessfulOperation);
        }

        #endregion

        /// <summary>
        ///     Disable command test send and receive for all clients
        /// </summary>

        #region DisableCommandTestSendAndReceiveForAllClients

        public void DisableCommandTestSendAndReceiveForAllClients()
        {
            _core.ScrollingAllAccountUtilities(s => s.SessionHandler.DisableTestMode());

            // Set log
            if (_core.Logging.LogIsActive(LogsType.EVENT))
                _core.Logging.LogEvent(LogMessage.DisableCommandTestModeForAllClients,
                    G9LogIdentity.DISABLE_TEST_MODE_ALL_CLIENT, LogMessage.SuccessfulOperation);
        }

        #endregion

        /// <summary>
        ///     Enable command test send and receive for single client
        /// </summary>
        /// <param name="sessionId">Specified session id for enable command send and receive</param>
        /// <param name="testMessage">
        ///     Test message
        ///     If set null => $"Test Mode - Session Id: {SessionId}"
        /// </param>

        #region EnableCommandTestSendAndReceiveForAllClients

        public void EnableCommandTestSendAndReceiveBySession(long sessionId, string testMessage = null)
        {
            _core.GetAccountUtilitiesBySessionId(sessionId).SessionHandler.EnableTestMode(testMessage);

            // Set log
            if (_core.Logging.LogIsActive(LogsType.EVENT))
                _core.Logging.LogEvent(LogMessage.EnableCommandTestModeForSingleSession,
                    G9LogIdentity.ENABLE_TEST_MODE_SINGLE_CLIENT, LogMessage.SuccessfulOperation);
        }

        #endregion

        /// <summary>
        ///     Disable command test send and receive for single client
        /// </summary>
        /// <param name="sessionId">Specified session id for enable command send and receive</param>

        #region DisableCommandTestSendAndReceiveForAllClients

        public void DisableCommandTestSendAndReceiveBySession(long sessionId)
        {
            _core.GetAccountUtilitiesBySessionId(sessionId).SessionHandler.DisableTestMode();

            // Set log
            if (_core.Logging.LogIsActive(LogsType.EVENT))
                _core.Logging.LogEvent(LogMessage.DisableCommandTestModeForSingleSession,
                    G9LogIdentity.DISABLE_TEST_MODE_SINGLE_CLIENT, LogMessage.SuccessfulOperation);
        }

        #endregion

        #endregion

        /// <summary>
        ///     Helper class for ready data for send
        /// </summary>
        /// <param name="commandName">Command name</param>
        /// <param name="data">Data for send</param>
        /// <returns>Ready packet split handler</returns>

        #region ReadyDataForSend

        private PacketSplitHandler ReadyDataForSend(string commandName, object data)
        {
            // Ready data for send
            ReadOnlySpan<byte> dataForSend = data is byte[]
                ? (byte[]) data
                : _core.Configuration.EncodingAndDecoding.EncodingType.GetBytes(data.ToJson());

            // Initialize command - length = CommandSize
            var commandData =
                _core.Configuration.EncodingAndDecoding.EncodingType.GetBytes(commandName
                    .PadLeft(_packetManagement.CalculateCommandSize, '9')
                    .Substring(0, _packetManagement.CalculateCommandSize));

            return _packetManagement.PackingRequestByData(commandData, dataForSend);
        }

        #endregion

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return => int number specify byte to send. if don't send return 0</returns>

        #region SendCommandByName

        public int SendCommandByName(long sessionId, string name, object data)
        {
            // Set send data
            var sendBytes = 0;
            try
            {
                // Ready data for send
                var dataForSend = ReadyDataForSend(name, data);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Get socket by session id
                var socket = _core.GetAccountUtilitiesBySessionId(sessionId).SessionSocket;

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    //Try to send
                    sendBytes += socket.Send(packets[i]);
            }
            catch (Exception ex)
            {
                if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByName,
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.FailedOperation);
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToClient);
            }

            return sendBytes;
        }

        #endregion

        /// <summary>
        ///     Send async command request by name
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>

        #region SendCommandByNameAsync

        public async Task<int> SendCommandByNameAsync(long sessionId, string name, object data)
        {
            // Set send data
            var sendBytes = 0;

            try
            {
                // Ready data for send
                var dataForSend = ReadyDataForSend(name, data);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Get socket by session id
                var socket = _core.GetAccountUtilitiesBySessionId(sessionId).SessionSocket;

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    sendBytes += await socket.SendAsync(new ArraySegment<byte>(packets[i]), SocketFlags.None);
            }
            catch (Exception ex)
            {
                if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByNameAsync,
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.FailedOperation);
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToClient);
            }

            return sendBytes;
        }

        #endregion

        /// <summary>
        ///     Send command request to all connectionby name
        /// </summary>
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return 'true' if send is success</returns>

        #region SendCommandToAllByName

        public bool SendCommandToAllByName(string name, object data)
        {
            // Ready data for send
            var dataForSend = ReadyDataForSend(name, data);

            // Get total packets
            var packets = dataForSend.GetPacketsArray();

            // Send total packets
            for (var i = 0; i < dataForSend.TotalPackets; i++)
            {
                var i1 = i;
                _core.ScrollingAllAccountUtilities(socketConnection =>
                    socketConnection.SessionSocket.Send(packets[i1]));
            }

            return true;
        }

        #endregion

        #endregion
    }

    #endregion
}