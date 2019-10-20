using System;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using G9Common.Abstract;
using G9Common.Enums;
using G9Common.Interface;
using G9Common.LogIdentity;
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
                SendCommandByNameAsync, OnSessionReceiveRequestOverTheLimitInSecondHandler, OnUnhandledCommandHandler,
                customLogging);

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
                var stateReceive = new G9SuperNetCoreStateObjectServer(_packetManagement.MaximumPacketSize,
                    account.Session.SessionId)
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
                if (_core.Logging.LogIsActive(LogsType.EVENT))
                    _core.Logging.LogEvent(
                        $"{LogMessage.SuccessClientConnection}\n{LogMessage.BufferSize}: {_packetManagement.MaximumPacketSize}\n{LogMessage.ClientSessionIdentity}: {sessionId}",
                        G9LogIdentity.SERVER_ACCEPT_CALLBACK, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
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

                // Read data from the client socket.   
                var bytesRead = (ushort) state.WorkSocket.EndReceive(asyncResult);

                if (bytesRead > 0)
                {
                    // Plus total receive bytes and packet
                    TotalReceiveBytes += bytesRead;
                    TotalReceivePacket++;

                    // unpacking request
                    var receivePacket = _packetManagement.UnpackingRequestByData(state.Buffer);

                    // Set last command (check ping automatically when set last command)
                    accountUtilities.SessionHandler.Core_SetLastCommand(receivePacket.Command);

                    // Plus receive bytes for session
                    accountUtilities.SessionHandler.Core_PlusSessionTotalReceiveBytes(bytesRead);

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

                // Set log
                if (_core.Logging.LogIsActive(LogsType.EVENT))
                    _core.Logging.LogEvent(
                        $"{LogMessage.SuccessClientReceive}\n{LogMessage.DataLength}: {bytesRead}\n{LogMessage.ClientSessionIdentity}: {sessionId}",
                        G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.SuccessfulOperation);
            }
            catch (Exception ex)
            {
                if (state?.Buffer != null)
                    // Clear Data
                    Array.Clear(state.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize);

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
                            $"{LogMessage.FailClinetConnection}\n{LogMessage.ClientSessionIdentity}: {(sessionId == 0 ? "NONE" : sessionId.ToString())}",
                            G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.FailedOperation);

                    // Run event on connected error
                    OnErrorHandler(ex, ServerErrorReason.ErrorReceiveDataFromClient);
                }
            }
            finally
            {
                // Listen and get other packet
                state?.WorkSocket.BeginReceive(state.Buffer, 0, AG9SuperNetCoreStateObjectBase.BufferSize, 0,
                    ReadCallback, state);
            }
        }

        #endregion

        /// <summary>
        ///     Send data to client
        /// </summary>
        /// <param name="handler">Socket handler for send</param>
        /// <param name="sessionId">Specified session id</param>
        /// <param name="byteData">Specify byte data for send</param>

        #region Send

        private WaitHandle Send(Socket handler, uint sessionId, byte[] byteData)
        {
            // Create the state object for sent
            var stateSend = new G9SuperNetCoreStateObjectServer(_packetManagement.MaximumPacketSize, sessionId)
            {
                WorkSocket = handler,
                Buffer = byteData
            };

            // Set log
            if (_core.Logging.LogIsActive(LogsType.EVENT))
                _core.Logging.LogEvent($"{LogMessage.RequestSendData}\n{LogMessage.DataLength}: {byteData.Length}",
                    G9LogIdentity.SERVER_SEND_DATA, LogMessage.SuccessfulOperation);

            // Begin sending the data to the remote device.  
            return handler.BeginSend(stateSend.Buffer, 0, byteData.Length, 0,
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
    }
}