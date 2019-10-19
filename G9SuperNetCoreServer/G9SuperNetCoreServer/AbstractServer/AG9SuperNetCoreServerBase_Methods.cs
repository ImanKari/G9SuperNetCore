using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using G9Common.JsonHelper;
using G9Common.LogIdentity;
using G9Common.Packet;
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

        #region Methods

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
            // Set flag and start
            EnableCommandTestSendReceiveAllClients = true;
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
            // Set flag and stop
            _core.ScrollingAllAccountUtilities(s => s.SessionHandler.DisableTestMode());
            EnableCommandTestSendReceiveAllClients = false;

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

        public void EnableCommandTestSendAndReceiveBySession(uint sessionId, string testMessage = null)
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

        public void DisableCommandTestSendAndReceiveBySession(uint sessionId)
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

        public int SendCommandByName(uint sessionId, string name, object data)
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

        public async Task<int> SendCommandByNameAsync(uint sessionId, string name, object data)
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
}