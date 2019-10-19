using System;
using System.Net.Sockets;
using System.Reflection;
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
            uint sessionId = 0;
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
                    _totalReceiveBytes += (ulong) packet.Length;

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
                            $"{LogMessage.FailClinetConnection}\n{LogMessage.ClientSessionIdentity}: {(sessionId == 0 ? "NONE" : sessionId.ToString())}",
                            G9LogIdentity.SERVER_RECEIVE_DATA, LogMessage.FailedOperation);

                    // Run event on connected error
                    OnErrorHandler(ex, ServerErrorReason.ErrorReceiveDataFromClient);
                }
            }
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
                _totalSendBytes += (ulong) bytesSent;

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