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
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
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
                    Session_SendCommandByName = SendCommandByName,
                    Session_SendCommandByNameAsync = SendCommandByNameAsync
                }, 0, IPAddress.Any);
            var account = new TAccount();
            _mainAccountUtilities.Account.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(
                _mainAccountUtilities.AccountHandler = new G9ClientAccountHandler(), session);

            // Set configuration
            Configuration = clientConfig;

            // Initialize packet management
            _packetManagement = new G9PacketManagement(Configuration.CommandSize, Configuration.BodySize,
                Configuration.EncodingAndDecoding, _logging);

            // Initialize state object
            _stateObject =
                new G9SuperNetCoreStateObjectClient(_packetManagement.MaximumPacketSize,
                    _mainAccountUtilities.Account.Session.SessionId);

            // Set log
            if (_logging.LogIsActive(LogsType.EVENT))
                _logging.LogEvent(LogMessage.CreateAndInitializeClient, G9LogIdentity.CREATE_CLIENT,
                    LogMessage.SuccessfulOperation);

            // Initialize command handler
            _commandHandler = new G9CommandHandler<TAccount>(commandAssembly, _logging, Configuration.CommandSize,
                OnUnhandledCommandHandler);

            // ######################## Add default command ########################
            // G9 Echo Command
            _commandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9EchoCommand),
                G9EchoCommandPingCommandReceiveHandler, null);
            // G9 Test Send Receive
            _commandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9TestSendReceive),
                G9TestSendReceiveCommandReceiveHandler, null);
            // G9 Ping Command
            _commandHandler.AddCustomCommand<string>(nameof(G9ReservedCommandName.G9PingCommand),
                PingCommandReceiveHandler, null);
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
                    var ipEndPoint = _clientSocket.RemoteEndPoint as IPEndPoint;
                    _logging.LogEvent(
                        $"{LogMessage.SuccessClientConnection}\n{LogMessage.IpAddress}: {ipEndPoint?.Address}\n{LogMessage.Port}: {ipEndPoint?.Port}",
                        G9LogIdentity.CLIENT_CONNECTED, LogMessage.SuccessfulOperation);
                }

                // Run event on connected
                OnConnectedHandler(_mainAccountUtilities.Account);

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
                    // Use like span
                    ReadOnlySpan<byte> packet = _stateObject.Buffer;

                    var receiveBytes = (ushort) packet.Length;

                    // Plus receive bytes and packet
                    TotalReceiveBytes += receiveBytes;
                    TotalReceivePacket++;

                    // Plus receive bytes and packet in session
                    _mainAccountUtilities.SessionHandler.Core_PlusSessionTotalReceiveBytes(receiveBytes);

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
                    _commandHandler.G9CallHandler(receivePacket, _mainAccountUtilities.Account);

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
                    OnDisconnectedHandler(_mainAccountUtilities.Account, DisconnectReason.DisconnectedFromServer);
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
        /// <returns>return WaitHandle for begin send</returns>

        #region Send

        private WaitHandle Send(Socket clientSocket, ReadOnlySpan<byte> data)
        {

            // Set log
            if (_logging.LogIsActive(LogsType.EVENT))
                _logging.LogEvent($"{LogMessage.RequestSendData}\n{LogMessage.DataLength}: {data.Length}",
                    $"{G9LogIdentity.CLIENT_SEND_DATA}", LogMessage.SuccessfulOperation);

            // Begin sending the data to the remote device.  
            return clientSocket.BeginSend(data.ToArray(), 0, data.Length, 0,
                SendCallback, clientSocket)?.AsyncWaitHandle;
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
                var bytesSent = (ushort)client.EndSend(asyncResult);

                // Signal that all bytes have been sent.  
                _sendDone.Set();

                // Plus send bytes and packet
                TotalSendBytes += bytesSent;
                TotalSendPacket++;

                // Plus send bytes and packet in session
                _mainAccountUtilities.SessionHandler.Core_PlusSessionTotalSendBytes(bytesSent);

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
    }
}