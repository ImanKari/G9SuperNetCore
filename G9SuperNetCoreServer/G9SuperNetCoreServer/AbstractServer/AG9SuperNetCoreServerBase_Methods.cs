﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
            _core.ScrollingAllAccountUtilities(s => s.SessionHandler.Core_EnableTestMode(testMessage));

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
            _core.ScrollingAllAccountUtilities(s => s.SessionHandler.Core_DisableTestMode());
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
            _core.GetAccountUtilitiesBySessionId(sessionId).SessionHandler.Core_EnableTestMode(testMessage);

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
            _core.GetAccountUtilitiesBySessionId(sessionId).SessionHandler.Core_DisableTestMode();

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
                _core.Configuration.EncodingAndDecoding.EncodingType.GetBytes(
                    commandName.GenerateStandardCommandName(_packetManagement.CalculateCommandSize));

            return _packetManagement.PackingRequestByData(commandData, dataForSend);
        }

        #endregion

        #region Send Command By Name

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return => int number specify byte to send. if don't send return 0</returns>

        #region SendCommandByName

        public int SendCommandByName(uint sessionId, string commandName, object commandData,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            // Set send data
            var sendBytes = 0;
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
                var dataForSend = ReadyDataForSend(commandName, commandData);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Get socket by session id
                var socket = _core.GetAccountUtilitiesBySessionId(sessionId).SessionSocket;

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    Send(socket, sessionId, packets[i])?.WaitOne();
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
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>

        #region SendCommandByNameAsync

        public async Task<int> SendCommandByNameAsync(uint sessionId, string commandName, object commandData,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            // Set send data
            var sendBytes = 0;

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
                var dataForSend = ReadyDataForSend(commandName, commandData);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Get socket by session id
                var socket = _core.GetAccountUtilitiesBySessionId(sessionId).SessionSocket;

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    Send(socket, sessionId, packets[i]);
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
        ///     Send command request to all clients by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return 'true' if send is success</returns>

        #region SendCommandToAllByName

        public bool SendCommandToAllByName(string commandName, object commandData, bool checkCommandExists = true,
            bool checkCommandSendType = true)
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
                var dataForSend = ReadyDataForSend(commandName, commandData);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                {
                    var i1 = i;
                    _core.ScrollingAllAccountUtilities(socketConnection =>
                        Send(socketConnection.SessionSocket, socketConnection.Account.Session.SessionId, packets[i1])?.WaitOne());
                }

                return true;
            }
            catch (Exception ex)
            {
                // Set log
                if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                    _core.Logging.LogException(ex, LogMessage.FailSendComandByNameToAll,
                        G9LogIdentity.SERVER_SEND_DATA_ALL_CLIENTS, LogMessage.FailedOperation);

                // Run event on error
                OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToAllClients);

                return false;
            }
        }

        #endregion

        /// <summary>
        ///     Send command request to all clients by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return 'true' if send is success</returns>

        #region SendCommandToAllByNameAsync

        public async Task<bool> SendCommandToAllByNameAsync(string commandName, object commandData,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            return await Task.Run(() =>
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
                    var dataForSend = ReadyDataForSend(commandName, commandData);

                    // Get total packets
                    var packets = dataForSend.GetPacketsArray();

                    // Send total packets
                    for (var i = 0; i < dataForSend.TotalPackets; i++)
                    {
                        var i1 = i;
                        _core.ScrollingAllAccountUtilities(socketConnection =>
                            Send(socketConnection.SessionSocket, socketConnection.Account.Session.SessionId, packets[i1]));
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    // Set log
                    if (_core.Logging.LogIsActive(LogsType.EXCEPTION))
                        _core.Logging.LogException(ex, LogMessage.FailSendComandByNameToAll,
                            G9LogIdentity.SERVER_SEND_DATA_ALL_CLIENTS, LogMessage.FailedOperation);

                    // Run event on error
                    OnErrorHandler(ex, ServerErrorReason.ErrorReadyToSendDataToAllClients);

                    return false;
                }
            });
        }

        #endregion

        #endregion

        #region Send Command By Command

        /// <summary>
        ///     Send command request by command
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return => int number specify byte to send. if don't send return 0</returns>

        #region SendCommand

        public int SendCommand<TCommand, TTypeSend>(uint sessionId, TTypeSend commandData,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
            where TCommand : IG9CommandWithSend
        {
            return SendCommandByName(sessionId, typeof(TCommand).Name, commandData, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send async command request by command
        /// </summary>
        /// <param name="sessionId">Session id for send</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>

        #region SendCommandAsync

        public async Task<int> SendCommandAsync<TCommand, TTypeSend>(uint sessionId, TTypeSend commandData,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            return await SendCommandByNameAsync(sessionId, typeof(TCommand).Name, commandData, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send command request to all clients by command
        /// </summary>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return 'true' if send is success</returns>

        #region SendCommandToAll

        public bool SendCommandToAll<TCommand, TTypeSend>(uint sessionId, TTypeSend commandData,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            return SendCommandToAllByName(typeof(TCommand).Name, commandData, checkCommandExists, checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send command request to all clients by command
        /// </summary>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return 'true' if send is success</returns>

        #region SendCommandToAllAsync

        public async Task<bool> SendCommandToAllAsync<TCommand, TTypeSend>(uint sessionId, TTypeSend commandData,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            return await SendCommandToAllByNameAsync(typeof(TCommand).Name, commandData, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        #endregion

        #endregion
    }
}