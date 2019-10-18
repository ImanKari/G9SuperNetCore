using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using G9Common.Abstract;
using G9Common.CommandHandler;
using G9Common.DefaultCommonCommand;
using G9Common.Interface;
using G9Common.JsonHelper;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.PacketManagement;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.ClientDefaultCommand;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Enums;
using G9SuperNetCoreClient.Helper;
using G9SuperNetCoreClient.Logging;

namespace G9SuperNetCoreClient.AbstractClient
{
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        #region Fields And Properties

        /// <summary>
        ///     Specified logging system
        /// </summary>
        private readonly IG9Logging _logging;

        /// <summary>
        ///     State object handle client task
        /// </summary>
        private G9SuperNetCoreStateObjectClient _stateObject;

        /// <summary>
        ///     Socket for client connection
        /// </summary>
        private Socket _clientSocket;

        /// <summary>
        ///     Access to client configuration
        /// </summary>
        public readonly G9ClientConfig Configuration;

        /// <summary>
        ///     Access to packet management
        /// </summary>
        private readonly G9PacketManagement _packetManagement;

        /// <summary>
        ///     ManualResetEvent instances signal completion.
        /// </summary>
        private readonly ManualResetEvent _connectDone = new ManualResetEvent(false);

        /// <summary>
        ///     ManualResetEvent instances signal completion.
        /// </summary>
        private readonly ManualResetEvent _receiveDone = new ManualResetEvent(false);

        /// <summary>
        ///     ManualResetEvent instances signal completion.
        /// </summary>
        private readonly ManualResetEvent _sendDone = new ManualResetEvent(false);

        /// <summary>
        ///     Access to command handler
        /// </summary>
        private readonly G9CommandHandler<TAccount> _commandHandler;

        /// <summary>
        ///     Access to main account
        /// </summary>
        public readonly TAccount MainAccount;

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

        #region Internal Client Method

        /// <summary>
        ///     Constructor
        ///     Initialize requirement
        ///     Set config
        /// </summary>
        /// <param name="clientConfig">Specify client configuration</param>
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        /// <param name="customLogging">Specified custom logging system</param>

        #region G9SuperNetCoreClientBase

        protected AG9SuperNetCoreClientBase(G9ClientConfig clientConfig, Assembly commandAssembly,
            IG9Logging customLogging = null)
        {
            // Set logging system
            _logging = customLogging ?? new G9LoggingClient();

            // Initialize account and session
            var session = new TSession();
            session.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(new G9ClientSessionHandler
            {
                SendCommandByName = SendCommandByName,
                SendCommandByNameAsync = SendCommandByNameAsync
            }, -1, IPAddress.Any);
            MainAccount = new TAccount();
            MainAccount.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(new G9ClientAccountHandler(), session);

            // Set configuration
            Configuration = clientConfig;

            // Initialize packet management
            _packetManagement = new G9PacketManagement(Configuration.CommandSize, Configuration.BodySize,
                Configuration.EncodingAndDecoding, _logging);

            // Initialize state object
            _stateObject =
                new G9SuperNetCoreStateObjectClient(_packetManagement.MaximumPacketSize, MainAccount.Session.SessionId);

            // Set log
            if (_logging.LogIsActive(LogsType.EVENT))
                _logging.LogEvent(LogMessage.CreateAndInitializeClient, G9LogIdentity.CREATE_CLIENT,
                    LogMessage.SuccessfulOperation);

            // Initialize command handler
            _commandHandler = new G9CommandHandler<TAccount>(commandAssembly, _logging, Configuration.CommandSize,
                OnUnhandledCommandHandler);

            // ######################## Add default command ########################
            // G9 Echo Command
            _commandHandler.AddCustomCommand<string>(G9EchoCommand.G9CommandName, G9EchoCommand.ReceiveHandler,
                G9EchoCommand.ErrorHandler);
            // G9 Test Send Receive
            _commandHandler.AddCustomCommand<string>(G9TestSendReceive.G9CommandName, G9TestSendReceive.ReceiveHandler,
                G9TestSendReceive.ErrorHandler);
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

                // Signal that the connection has been made.  
                _connectDone.Set();

                // Set log
                if (_logging.LogIsActive(LogsType.EVENT))
                {
                    var ipe = _clientSocket.RemoteEndPoint as IPEndPoint;
                    _logging.LogEvent(
                        $"{LogMessage.SuccessClientConnection}\n{LogMessage.IpAddress}: {ipe.Address}\n{LogMessage.Port}: {ipe.Port}",
                        G9LogIdentity.CLIENT_CONNECTED, LogMessage.SuccessfulOperation);
                }

                // Run event on connected
                OnConnectedHandler(MainAccount);

                // Listen for receive
                Receive(_clientSocket);
            }
            catch (Exception ex)
            {
                if (_logging.LogIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailClinetConnection, G9LogIdentity.CLIENT_CONNECTED,
                        LogMessage.FailedOperation);
                // Run event on connected error
                OnErrorHandler(ex, ClientErrorReason.ClientConnectedError);
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
                if (_logging.LogIsActive(LogsType.EVENT))
                    _logging.LogEvent($"{LogMessage.ReceiveClientFromServer}", G9LogIdentity.CLIENT_RECEIVE,
                        LogMessage.SuccessfulOperation);
            }
            catch (Exception e)
            {
                if (_logging.LogIsActive(LogsType.EXCEPTION))
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
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                _stateObject = (G9SuperNetCoreStateObjectClient) asyncResult.AsyncState;
                var client = _stateObject.WorkSocket;

                // Read data from the remote device.  
                var bytesRead = client.EndReceive(asyncResult);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Use like span
                    ReadOnlySpan<byte> packet = _stateObject.Buffer;

                    // Plus receive bytes
                    _totalReceiveBytes += packet.Length;

                    // unpacking request
                    var receivePacket = _packetManagement.UnpackingRequestByData(packet);

                    // Set log
                    if (_logging.LogIsActive(LogsType.INFO))
                        _logging.LogInformation(
                            $"{LogMessage.SuccessUnpackingReceiveData}\n{LogMessage.PacketType}: {receivePacket.TypeOfPacketType.ToString()}\n{LogMessage.Command}: {receivePacket.Command}\n{LogMessage.Body}: '{Configuration.EncodingAndDecoding.EncodingType.GetString(receivePacket.Body.ToArray())}'\n{LogMessage.PacketRequestId}: {receivePacket.RequestId}",
                            $"{G9LogIdentity.CLIENT_RECEIVE}", LogMessage.SuccessfulOperation);

                    // Clear Data
                    Array.Clear(_stateObject.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize);


                    // Progress packet
                    _commandHandler.G9CallHandler(receivePacket, MainAccount);

                    // Signal that all bytes have been received.  
                    _receiveDone.Set();
                }

                // Get the rest of the data.  
                client.BeginReceive(_stateObject.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                    ReceiveCallback, _stateObject);
            }
            catch (Exception ex)
            {

                if (ex is SocketException exception && exception.ErrorCode == 10054)
                {
                    // Run event disconnect
                    OnDisconnectedHandler(MainAccount, DisconnectReason.DisconnectedFromServer);
                }
                else
                {

                    if (_logging.LogIsActive(LogsType.EXCEPTION))
                        _logging.LogException(ex, LogMessage.FailClientReceive, G9LogIdentity.CLIENT_RECEIVE,
                            LogMessage.FailedOperation);

                    OnErrorHandler(ex, ClientErrorReason.ErrorInReceiveData);
                }
            }
        }

        #endregion

        /// <summary>
        ///     Send data
        ///     Handle for send
        /// </summary>
        /// <param name="clientSocket">Specify client socket</param>
        /// <param name="data">Specify data for send</param>

        #region Send

        private int Send(Socket clientSocket, ReadOnlySpan<byte> data)
        {
            // Begin sending the data to the remote device.  
            clientSocket.BeginSend(data.ToArray(), 0, data.Length, 0,
                SendCallback, clientSocket);

            // Set log
            if (_logging.LogIsActive(LogsType.EVENT))
                _logging.LogEvent($"{LogMessage.RequestSendData}\n{LogMessage.DataLength}: {data.Length}",
                    $"{G9LogIdentity.CLIENT_SEND_DATA}", LogMessage.SuccessfulOperation);

            return data.Length;
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
                var bytesSent = client.EndSend(asyncResult);

                // Signal that all bytes have been sent.  
                _sendDone.Set();

                // Plus send bytes
                _totalSendBytes += bytesSent;

                // Set log
                if (_logging.LogIsActive(LogsType.INFO))
                    _logging.LogInformation(
                        $"{LogMessage.SuccessRequestSendData}\n{LogMessage.DataLength}: {bytesSent}",
                        G9LogIdentity.CLIENT_SEND_DATA, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (_logging.LogIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailRequestSendData, G9LogIdentity.CLIENT_SEND_DATA,
                        LogMessage.FailedOperation);

                OnErrorHandler(ex, ClientErrorReason.ErrorSendDataToServer);
            }
        }

        #endregion

        #endregion

        #region Other methods

        /// <summary>
        ///     Start connection
        ///     Initialize client and try to connect server
        /// </summary>

        #region StartConnection

        public async Task<bool> StartConnection()
        {
            return await Task.Run(() =>
            {
                // Set log
                if (_logging.LogIsActive(LogsType.EXCEPTION))
                    _logging.LogEvent(LogMessage.StartClientConnection, G9LogIdentity.START_CLIENT_CONNECTION,
                        LogMessage.SuccessfulOperation);

                // Connect to a remote device.  
                try
                {
                    // Establish the remote endpoint for the socket.  
                    var remoteEP = new IPEndPoint(Configuration.IpAddress, Configuration.PortNumber);

                    // Create a TCP/IP socket.  
                    var client = new Socket(Configuration.IpAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect to the remote endpoint.  
                    client.BeginConnect(remoteEP,
                        ConnectCallback, client);
                    _connectDone.WaitOne();

                    // Run event on connected handler
                    OnConnectedHandler(MainAccount);

                    return true;
                }
                catch (Exception e)
                {
                    // Set log
                    if (_logging.LogIsActive(LogsType.EXCEPTION))
                        _logging.LogException(e, LogMessage.FailClinetConnection, G9LogIdentity.START_CLIENT_CONNECTION,
                            LogMessage.FailedOperation);

                    // Run Event on error
                    OnErrorHandler(e, ClientErrorReason.ClientConnectedError);

                    return false;
                }
            });
        }

        #endregion

        /// <summary>
        ///     Disconnect from server
        ///     Initialize client and try to connect server
        /// </summary>

        #region Disconnect

        public async Task<bool> Disconnect()
        {
            return await Task.Run(() =>
            {
                // Connect to a remote device.  
                try
                {
                    if (_clientSocket is null)
                    {
                        // Set log
                        if (_logging.LogIsActive(LogsType.ERROR))
                            _logging.LogError(LogMessage.CantStopStoppedServer,
                                G9LogIdentity.STOP_SERVER, LogMessage.FailedOperation);
                        // Run event
                        OnErrorHandler(new Exception(LogMessage.CantStopStoppedServer),
                            ClientErrorReason.ClientDisconnectedAndReceiveRequestForDisconnect);
                        return false;
                    }

                    // Close, Disconnect and dispose
                    _clientSocket.Dispose();
                    _clientSocket = null;

                    // Set log
                    if (_logging.LogIsActive(LogsType.EVENT))
                        _logging.LogEvent(LogMessage.StopServer, G9LogIdentity.STOP_SERVER,
                            LogMessage.SuccessfulOperation);
                    
                    // Run event on disconnect
                    OnDisconnectedHandler(MainAccount, DisconnectReason.DisconnectedByProgram);

                    return true;
                }
                catch (Exception e)
                {
                    // Set log
                    if (_logging.LogIsActive(LogsType.EXCEPTION))
                        _logging.LogException(e, LogMessage.FailClinetConnection, G9LogIdentity.START_CLIENT_CONNECTION,
                            LogMessage.FailedOperation);

                    // Run Event on error
                    OnErrorHandler(e, ClientErrorReason.ClientConnectedError);

                    return false;
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
                : Configuration.EncodingAndDecoding.EncodingType.GetBytes(data.ToJson());

            // Initialize command - length = CommandSize
            ReadOnlySpan<byte> commandData =
                Configuration.EncodingAndDecoding.EncodingType.GetBytes(commandName
                    .PadLeft(_packetManagement.CalculateCommandSize, '9')
                    .Substring(0, _packetManagement.CalculateCommandSize));

            return _packetManagement.PackingRequestByData(commandData, dataForSend);
        }

        #endregion

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return 'true' if send is success</returns>

        #region SendCommandByName

        public int SendCommandByName(string name, object data)
        {
            // Set send data
            var sendBytes = 0;
            try
            {
                // Ready data for send
                var dataForSend = ReadyDataForSend(name, data);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    sendBytes = Send(_clientSocket, packets[i]);
            }
            catch (Exception ex)
            {
                // Set log
                if (_logging.LogIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailSendComandByName,
                        G9LogIdentity.CLIENT_SEND_DATA, LogMessage.FailedOperation);

                // Run event on error
                OnErrorHandler(ex, ClientErrorReason.ErrorReadyToSendDataToServer);
            }

            return sendBytes;
        }

        #endregion

        /// <summary>
        ///     Send async command request by name
        /// </summary>
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>

        #region SendCommandByNameAsync

        public async Task<int> SendCommandByNameAsync(string name, object data)
        {
            return await Task.Run(() =>
            {
                // Set send data
                var sendBytes = 0;
                try
                {
                    // Ready data for send
                    var dataForSend = ReadyDataForSend(name, data);

                    // Get total packets
                    var packets = dataForSend.GetPacketsArray();

                    // Send total packets
                    for (var i = 0; i < dataForSend.TotalPackets; i++)
                        // Try to send
                        sendBytes = Send(_clientSocket, packets[i]);
                }
                catch (Exception ex)
                {
                    // Set log
                    if (_logging.LogIsActive(LogsType.EXCEPTION))
                        _logging.LogException(ex, LogMessage.FailSendComandByNameAsync,
                            G9LogIdentity.CLIENT_SEND_DATA, LogMessage.FailedOperation);

                    // Run event on error
                    OnErrorHandler(ex, ClientErrorReason.ErrorReadyToSendDataToServer);
                }

                return sendBytes;
            });
        }

        #endregion

        #region Helper Class For Send

        private int SendCommandByName(long sessionId, string name, object data)
        {
            return SendCommandByName(name, data);
        }

        private async Task<int> SendCommandByNameAsync(long sessionId, string name, object data)
        {
            return await SendCommandByNameAsync(name, data);
        }

        #endregion

        #endregion

        #endregion
    }
}