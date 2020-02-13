using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using G9Common.Enums;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.JsonHelper;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Enums;

namespace G9SuperNetCoreServer.AbstractServer
{
    // ReSharper disable once InconsistentNaming
    public abstract partial class AG9SuperNetCoreServerBase<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        #region Methods

        /// <summary>
        ///     <para>Start server</para>
        ///     <para>Ready listen connection and accept request</para>
        /// </summary>

        #region Start

        public async Task Start()
        {
            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
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
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
                        _core.Logging.LogEvent(
                            $"{LogMessage.SuccessCreateServerSocket}\n{LogMessage.IpAddress}: {_core.Configuration.IpAddress}\n{LogMessage.Port}: {_core.Configuration.PortNumber}",
                            G9LogIdentity.CREATE_LISTENER, LogMessage.SuccessfulOperation);
                }
                catch (Exception ex)
                {
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
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
                    if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                        _core.Logging.LogInformation(LogMessage.SuccessBindAndListenSocket, G9LogIdentity.BIND_LISTENER,
                            LogMessage.SuccessfulOperation);
                }
                catch (Exception ex)
                {
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
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
                        if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
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
                        if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                            _core.Logging.LogException(ex, LogMessage.FailtOnWaitForConnection,
                                G9LogIdentity.WAIT_LISTENER, LogMessage.FailedOperation);
                        OnErrorHandler(ex, ServerErrorReason.ClientConnectedError);
                    }
            });
        }

        #endregion

        /// <summary>
        ///     <para>Stop server</para>
        ///     <para>Releases all resources used by server</para>
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
                        if (_core.Logging.CheckLoggingIsActive(LogsType.ERROR))
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
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
                        _core.Logging.LogEvent(LogMessage.StopServer, G9LogIdentity.STOP_SERVER,
                            LogMessage.SuccessfulOperation);

                    // Call gc collect
                    GC.Collect();

                    return true;
                }
                catch (Exception ex)
                {
                    // Set log
                    if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
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
        ///     <para>Test message</para>
        ///     <para>If set null => $"Test Mode - Session Id: {SessionId}"</para>
        /// </param>

        #region EnableCommandTestSendAndReceiveForAllClients

        public void EnableCommandTestSendAndReceiveForAllClients(string testMessage = null)
        {
            // Set flag and start
            EnableCommandTestSendReceiveAllClients = true;
            _core.ScrollingAllAccountUtilities(s => s.SessionHandler.Core_EnableTestMode(testMessage));

            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
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
            _core.ScrollingAllAccountUtilities(s => s.SessionHandler.Core_DisableTestMode());
            EnableCommandTestSendReceiveAllClients = false;

            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
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
            _core.GetAccountUtilitiesBySessionId(sessionId).SessionHandler.Core_EnableTestMode(testMessage);

            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
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
            _core.GetAccountUtilitiesBySessionId(sessionId).SessionHandler.Core_DisableTestMode();

            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.EVENT))
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
        /// <param name="packetDataType">Custom packet data type</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <returns>Ready packet split handler</returns>

        #region ReadyDataForSend

        private G9PacketSplitHandler ReadyDataForSend(string commandName, object data, G9PacketDataType packetDataType,
            Guid? customRequestId)
        {
            // Ready data for send
#if NETSTANDARD2_1 || NETCOREAPP3_0
            ReadOnlySpan<byte>
#else
            var
#endif
            dataForSend = data is byte[]
                ? (byte[]) data
                : _core.Configuration.EncodingAndDecoding.EncodingType.GetBytes(data.ToJson());

            // Initialize command - length = CommandSize
#if NETSTANDARD2_1 || NETCOREAPP3_0
            ReadOnlySpan<byte>
#else
            var
#endif
            commandData =
                _core.Configuration.EncodingAndDecoding.EncodingType.GetBytes(
                    commandName.GenerateStandardCommandName(_packetManagement.CalculateCommandSize));

            return _packetManagement.PackingRequestByData(commandData, dataForSend, packetDataType, customRequestId);
        }

        #endregion

        #region Send Command By Name

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     <para>If set true, check command exists</para>
        ///     <para>If not exists throw exception</para>
        /// </param>
        /// <param name="checkCommandSendType">
        ///     <para>If set true, check command send type</para>
        ///     <para>If func send data type not equal with command send type throw exception</para>
        /// </param>

        #region SendCommandByName

        public void SendCommandByName(uint sessionId, string commandName, object commandData,
            Guid? customRequestId = null, bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_core.CommandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _core.CommandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_core.CommandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, G9PacketDataType.StandardCommand,
                    customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Get account utilities by session id
                var accountUtilities = _core.GetAccountUtilitiesBySessionId(sessionId);

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    Send(accountUtilities.SessionSocket, accountUtilities.Account, packets[i]).WaitOne(3699);
            }
            catch (Exception ex)
            {
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByName,
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.FailedOperation);
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToClient);
            }
        }

        #endregion

        /// <summary>
        ///     Send async command request by name
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     <para>If set true, check command exists</para>
        ///     <para>If not exists throw exception</para>
        /// </param>
        /// <param name="checkCommandSendType">
        ///     <para>If set true, check command send type</para>
        ///     <para>If func send data type not equal with command send type throw exception</para>
        /// </param>

        #region SendCommandByNameAsync

        public void SendCommandByNameAsync(uint sessionId, string commandName, object commandData,
            Guid? customRequestId = null, bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_core.CommandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _core.CommandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_core.CommandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, G9PacketDataType.StandardCommand,
                    customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Get account utilities by session id
                var accountUtilities = _core.GetAccountUtilitiesBySessionId(sessionId);

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    Send(accountUtilities.SessionSocket, accountUtilities.Account, packets[i]).WaitOne(3699);
            }
            catch (Exception ex)
            {
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByNameAsync,
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.FailedOperation);
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToClient);
            }
        }

        #endregion

        /// <summary>
        ///     <para>Send async command request by name</para>
        ///     <para>With custom packet data type</para>
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="packetDataType">custom packet data type</param>
        /// <param name="checkCommandExists">
        ///     <para>If set true, check command exists</para>
        ///     <para>If not exists throw exception</para>
        /// </param>
        /// <param name="checkCommandSendType">
        ///     <para>If set true, check command send type</para>
        ///     <para>If func send data type not equal with command send type throw exception</para>
        /// </param>

        #region SendCommandByNameAsyncWithCustomPacketDataType

        // ReSharper disable once UnusedMember.Local
        private void SendCommandByNameAsyncWithCustomPacketDataType(uint sessionId, string commandName,
            object commandData, G9PacketDataType packetDataType, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_core.CommandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _core.CommandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_core.CommandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, packetDataType, customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Get account utilities by session id
                var accountUtilities = _core.GetAccountUtilitiesBySessionId(sessionId);

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    Send(accountUtilities.SessionSocket, accountUtilities.Account, packets[i]);
            }
            catch (Exception ex)
            {
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByNameAsync,
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.FailedOperation);
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToClient);
            }
        }

        #endregion

        /// <summary>
        ///     <para>Send command request by name</para>
        ///     <para>With custom packet data type</para>
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="packetDataType">custom packet data type</param>
        /// <param name="checkCommandExists">
        ///     <para>If set true, check command exists</para>
        ///     <para>If not exists throw exception</para>
        /// </param>
        /// <param name="checkCommandSendType">
        ///     <para>If set true, check command send type</para>
        ///     <para>If func send data type not equal with command send type throw exception</para>
        /// </param>

        #region SendCommandByNameWithCustomPacketDataType

        private void SendCommandByNameWithCustomPacketDataType(uint sessionId, string commandName,
            object commandData, G9PacketDataType packetDataType, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_core.CommandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _core.CommandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_core.CommandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, packetDataType, customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Get account utilities by session id
                var accountUtilities = _core.GetAccountUtilitiesBySessionId(sessionId);

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    Send(accountUtilities.SessionSocket, accountUtilities.Account, packets[i]).WaitOne(3699);
            }
            catch (Exception ex)
            {
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByNameAsync,
                        G9LogIdentity.SERVER_SEND_DATA, LogMessage.FailedOperation);
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToClient);
            }
        }

        #endregion

        /// <summary>
        ///     Send command request to all clients by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     <para>If set true, check command exists</para>
        ///     <para>If not exists throw exception</para>
        /// </param>
        /// <param name="checkCommandSendType">
        ///     <para>If set true, check command send type</para>
        ///     <para>If func send data type not equal with command send type throw exception</para>
        /// </param>

        #region SendCommandToAllByName

        public void SendCommandToAllByName(string commandName, object commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_core.CommandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _core.CommandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_core.CommandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, G9PacketDataType.StandardCommand,
                    customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                {
                    var i1 = i;
                    _core.ScrollingAllAccountUtilities(socketConnection =>
                        Send(socketConnection.SessionSocket, socketConnection.Account, packets[i1])
                            .WaitOne(3699));
                }
            }
            catch (Exception ex)
            {
                // Set log
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByNameToAll,
                        G9LogIdentity.SERVER_SEND_DATA_ALL_CLIENTS, LogMessage.FailedOperation);

                // Run event on error
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToAllClients);
            }
        }

        #endregion

        /// <summary>
        ///     Send command request to all clients by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     <para>If set true, check command exists</para>
        ///     <para>If not exists throw exception</para>
        /// </param>
        /// <param name="checkCommandSendType">
        ///     <para>If set true, check command send type</para>
        ///     <para>If func send data type not equal with command send type throw exception</para>
        /// </param>

        #region SendCommandToAllByNameAsync

        public void SendCommandToAllByNameAsync(string commandName, object commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_core.CommandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _core.CommandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_core.CommandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, G9PacketDataType.StandardCommand,
                    customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                {
                    var i1 = i;
                    _core.ScrollingAllAccountUtilities(socketConnection =>
                        Send(socketConnection.SessionSocket, socketConnection.Account,
                            packets[i1]));
                }
            }
            catch (Exception ex)
            {
                // Set log
                if (_core.Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByNameToAll,
                        G9LogIdentity.SERVER_SEND_DATA_ALL_CLIENTS, LogMessage.FailedOperation);

                // Run event on error
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToAllClients);
            }
        }

        #endregion

        #endregion

        #region Send Command By Command

        /// <summary>
        ///     Send command request by command
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>

        #region SendCommand

        public void SendCommand<TCommand, TTypeSend>(uint sessionId, TTypeSend commandData,
            Guid? customRequestId = null, bool checkCommandExists = true, bool checkCommandSendType = true)
            where TCommand : IG9CommandWithSend
        {
            SendCommandByName(sessionId, typeof(TCommand).Name, commandData, customRequestId, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send async command request by command
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>

        #region SendCommandAsync

        public void SendCommandAsync<TCommand, TTypeSend>(uint sessionId, TTypeSend commandData,
            Guid? customRequestId = null, bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            SendCommandByNameAsync(sessionId, typeof(TCommand).Name, commandData, customRequestId, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send command request to all clients by command
        /// </summary>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>

        #region SendCommandToAll

        public void SendCommandToAll<TCommand, TTypeSend>(TTypeSend commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            SendCommandToAllByName(typeof(TCommand).Name, commandData, customRequestId, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send command request to all clients by command
        /// </summary>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>

        #region SendCommandToAllAsync

        public void SendCommandToAllAsync<TCommand, TTypeSend>(TTypeSend commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            SendCommandToAllByNameAsync(typeof(TCommand).Name, commandData, customRequestId, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        #endregion

        #endregion
    }
}