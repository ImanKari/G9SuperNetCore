using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using G9Common.Abstract;
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
using G9SuperNetCoreServer.HelperClass;

namespace G9SuperNetCoreServer.AbstractServer
{
    public abstract class AG9SuperNetCoreServerBase<TAccount, TSession>
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
                SendCommandByNameAsync, customLogging);

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
                var (accept, sessionIdentity) = _core.TryAcceptConnection(handler);

                // Return if not accepted
                if (!accept) return;

                // Create the state object.  
                var state = new G9SuperNetCoreStateObjectServer(_packetManagement.MaximumPacketSize, sessionIdentity)
                {
                    WorkSocket = handler
                };
                sessionId = state.SessionIdentity;

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

                // Read data from the client socket.   
                var bytesRead = handler.EndReceive(asyncResult);

                if (bytesRead > 0)
                {
                    // Use like span
                    ReadOnlySpan<byte> packet = state.Buffer;

                    // unpacking request
                    var receivePacket = _packetManagement.UnpackingRequestByData(packet);

                    // Set log
                    if (_core.Logging.LogIsActive(LogsType.INFO))
                        _core.Logging.LogInformation(
                            $"{LogMessage.SuccessUnpackingReceiveData}\n{LogMessage.PacketType}: {receivePacket.TypeOfPacketType.ToString()}\n{LogMessage.Command}: {receivePacket.Command}\n{LogMessage.Body}: '{_core.Configuration.EncodingAndDecoding.EncodingType.GetString(receivePacket.Body.ToArray())}'\n{LogMessage.PacketRequestId}: {receivePacket.RequestId}",
                            G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.SuccessfulOperation);

                    // Clear Data
                    Array.Clear(state.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize);

                    // Progress packet
                    _core.CommandHandler.G9CallHandler(receivePacket, _core.GetAccountBySessionId(sessionId));
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
                if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex,
                        $"{LogMessage.FailClinetConnection}\n{LogMessage.ClientSessionIdentity}: {(sessionId == -1 ? "NONE" : sessionId.ToString())}",
                        G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.FailedOperation);
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

                // Set log
                if (_core.Logging.LogIsActive(LogsType.INFO))
                    _core.Logging.LogInformation(
                        $"{LogMessage.SuccessRequestSendData}\n{LogMessage.DataLength}: {bytesSent}",
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.SuccessfulOperation);

                //handler.Shutdown(SocketShutdown.Send);
                //handler.Close();
            }
            catch (Exception ex)
            {
                if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailRequestSendData, G9LogIdentity.SERVER_SEND_DATA,
                        LogMessage.FailedOperation);
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

        public void Start()
        {
            // Set log
            if (_core.Logging.LogIsActive(LogsType.EVENT))
                _core.Logging.LogEvent(LogMessage.StartServer, G9LogIdentity.START_SERVER,
                    LogMessage.SuccessfulOperation);

            // Run task
            Task.Run(() =>
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
                    return;
                }

                // Bind the socket to the local endpoint and listen for incoming connections.  
                // Infinity loop for binding connection
                while (true)
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
                    }
            });
        }

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
            // Ready data for send
            var dataForSend = ReadyDataForSend(name, data);

            // Get total packets
            var packets = dataForSend.GetPacketsArray();

            // Get socket by session id
            var socket = _core.GetSocketBySessionId(sessionId);

            // Set send data
            var sendBytes = 0;

            // Send total packets
            for (var i = 0; i < dataForSend.TotalPackets; i++)
                //Try to send
                sendBytes += socket.Send(packets[i]);
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
            // Ready data for send
            var dataForSend = ReadyDataForSend(name, data);

            // Get total packets
            var packets = dataForSend.GetPacketsArray();

            // Get socket by session id
            var socket = _core.GetSocketBySessionId(sessionId);

            // Set send data
            var sendBytes = 0;

            // Send total packets
            for (var i = 0; i < dataForSend.TotalPackets; i++)
                // Try to send
                sendBytes += await socket.SendAsync(new ArraySegment<byte>(packets[i]), SocketFlags.None);

            // if don't send
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
                _core.ScrollingAllSocket(socketConnection => socketConnection.Send(packets[i1]));
            }

            return true;
        }

        #endregion

        #endregion
    }

    #endregion
}