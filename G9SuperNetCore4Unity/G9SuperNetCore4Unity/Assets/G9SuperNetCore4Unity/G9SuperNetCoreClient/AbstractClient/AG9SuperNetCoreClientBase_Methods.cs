using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using G9Common.Enums;
using G9Common.HelperClass;
using G9Common.JsonHelper;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.PacketManagement;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Enums;
using G9SuperNetCoreClient.Helper;

// ReSharper disable once CheckNamespace
namespace G9SuperNetCoreClient.AbstractClient
{
    // ReSharper disable once InconsistentNaming
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        #region Methods

        /// <summary>
        ///     Start connection
        ///     Initialize client and try to connect server
        /// </summary>

        #region StartConnection

        public async Task<bool> StartConnection()
        {
            return await Task.Run(async () =>
            {
                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                    _logging.LogEvent(LogMessage.StartClientConnection, G9LogIdentity.START_CLIENT_CONNECTION,
                        LogMessage.SuccessfulOperation);

                // Connect to a remote device.  
                try
                {
                    // Disconnect if server is connected
                    await Disconnect();

                    // Establish the remote endpoint for the socket.  
                    var remoteEndPoint = new IPEndPoint(Configuration.IpAddress, Configuration.PortNumber);

                    // Create a TCP/IP socket.  
                    var client = new Socket(Configuration.IpAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);


                    // Connect to the remote endpoint.  
                    client.BeginConnect(remoteEndPoint,
                        ConnectCallback, client);

                    _connectDone.WaitOne();

                    return true;
                }
                catch (Exception e)
                {
                    // Set log
                    if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                        _logging.LogException(e, LogMessage.FailClinetConnection, G9LogIdentity.START_CLIENT_CONNECTION,
                            LogMessage.FailedOperation);

                    // Run Event on error
                    OnErrorHandler(e, ClientErrorReason.ClientConnectedError, true);

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
                    // If client is null => it's disconnect => return true
                    if (_clientSocket is null) return true;

                    // Close, Disconnect and dispose
                    _clientSocket.Dispose();
                    _clientSocket = null;

                    // Set log
                    if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                        _logging.LogEvent(LogMessage.StopServer, G9LogIdentity.STOP_SERVER,
                            LogMessage.SuccessfulOperation);

                    // Run event on disconnect
                    OnDisconnectedHandler(_mainAccountUtilities.Account, DisconnectReason.DisconnectedByProgram, false);

                    // Clear total client data for reset
                    ResetAndClearClientData();

                    return true;
                }
                catch (Exception e)
                {
                    // Set log
                    if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                        _logging.LogException(e, LogMessage.CantStopStoppedServer, G9LogIdentity.STOP_CLIENT_CONNECTION,
                            LogMessage.FailedOperation);

                    // Run Event on error
                    OnErrorHandler(e, ClientErrorReason.ClientConnectedError);

                    return false;
                }
            });
        }

        #endregion

        /// <summary>
        ///     Used for reset and clear total data and ready for restart
        /// </summary>

        #region ResetAndClearClientData

        private void ResetAndClearClientData()
        {
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
                    // Set send command sync
                    Session_SendCommandByName = SendCommandByName,
                    // Set send command async
                    Session_SendCommandByNameAsync = SendCommandByNameAsync,
                    // Set session encoding
                    Session_GetSessionEncoding = () => Configuration.EncodingAndDecoding
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

            // Reset send receive bytes counter
            TotalSendBytes = TotalReceiveBytes = TotalSendPacket = TotalReceivePacket = 0;

            // Remove dead space
            GC.Collect();
        }

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
                    : Configuration.EncodingAndDecoding.EncodingType.GetBytes(data.ToJson());

            // Initialize command - length = CommandSize
#if NETSTANDARD2_1 || NETCOREAPP3_0
            ReadOnlySpan<byte>
#else
            var
#endif
                commandData =
                    Configuration.EncodingAndDecoding.EncodingType.GetBytes(
                        commandName.GenerateStandardCommandName(_packetManagement.CalculateCommandSize));

            return _packetManagement.PackingRequestByData(commandData, dataForSend, packetDataType, customRequestId);
        }

        #endregion

        #region Send Command By Name

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
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

        #region SendCommandByName

        public void SendCommandByName(string commandName, object commandData, Guid? customRequestId = null,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_commandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _commandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_commandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, G9PacketDataType.StandardCommand,
                    customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    Send(_clientSocket, packets[i])?.WaitOne(3999);
            }
            catch (Exception ex)
            {
                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailSendComandByName,
                        G9LogIdentity.CLIENT_SEND_DATA, LogMessage.FailedOperation);

                // Run event on error
                OnErrorHandler(ex, ClientErrorReason.ErrorReadyToSendDataToServer);
            }
        }

        #endregion

        /// <summary>
        ///     Send async command request by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
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

        #region SendCommandByNameAsync

        public void SendCommandByNameAsync(string commandName, object commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_commandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _commandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_commandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, G9PacketDataType.StandardCommand,
                    customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    Send(_clientSocket, packets[i]).WaitOne(3999);
            }
            catch (Exception ex)
            {
                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailSendComandByNameAsync,
                        G9LogIdentity.CLIENT_SEND_DATA, LogMessage.FailedOperation);

                // Run event on error
                OnErrorHandler(ex, ClientErrorReason.ErrorReadyToSendDataToServer);
            }
        }

        #endregion

        /// <summary>
        ///     <para>Send async command request by name</para>
        ///     <para>With custom packet data type</para>
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="packetDataType">custom packet data type</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     <para>If set true, check command exists</para>
        ///     <para>If not exists throw exception</para>
        /// </param>
        /// <param name="checkCommandSendType">
        ///     <para>If set true, check command send type</para>
        ///     <para>If func send data type not equal with command send type throw exception</para>
        /// </param>
        /// <param name="isAuthorization">Specified send for authorization</param>

        #region SendCommandByNameAsyncWithCustomPacketDataType

        private void SendCommandByNameAsyncWithCustomPacketDataType(string commandName, object commandData,
            G9PacketDataType packetDataType, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true, bool isAuthorization = false)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_commandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _commandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_commandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, packetDataType, customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    Send(_clientSocket, packets[i], isAuthorization).WaitOne(3999);
            }
            catch (Exception ex)
            {
                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailSendComandByNameAsync,
                        G9LogIdentity.CLIENT_SEND_DATA, LogMessage.FailedOperation);

                // Run event on error
                OnErrorHandler(ex, ClientErrorReason.ErrorReadyToSendDataToServer);
            }
        }

        #endregion

        /// <summary>
        ///     <para>Send command request by name</para>
        ///     <para>With custom packet data type</para>
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="packetDataType">custom packet data type</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     <para>If set true, check command exists</para>
        ///     <para>If not exists throw exception</para>
        /// </param>
        /// <param name="checkCommandSendType">
        ///     <para>If set true, check command send type</para>
        ///     <para>If func send data type not equal with command send type throw exception</para>
        /// </param>

        #region SendCommandByNameWithCustomPacketDataType

        private void SendCommandByNameWithCustomPacketDataType(string commandName, object commandData,
            G9PacketDataType packetDataType, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            try
            {
                // Check exists command
                if (checkCommandExists && !_commandHandler.CheckCommandExist(commandName))
                    throw new Exception($"{LogMessage.Command}\n{LogMessage.CommandName}: {commandName}");

                // Check exists command
                if (checkCommandSendType &&
                    _commandHandler.GetCommandSendType(commandName) != commandData.GetType())
                    throw new Exception(
                        $"{LogMessage.CommandSendTypeNotCorrect}\n{LogMessage.CommandName}: {commandName}\n{LogMessage.SendTypeWithFunction}: {commandData.GetType()}\n{LogMessage.CommandSendType}: {_commandHandler.GetCommandSendType(commandName)}");

                // Ready data for send
                var dataForSend = ReadyDataForSend(commandName, commandData, packetDataType, customRequestId);

                // Get total packets
                var packets = dataForSend.GetPacketsArray();

                // Send total packets
                for (var i = 0; i < dataForSend.TotalPackets; i++)
                    // Try to send
                    Send(_clientSocket, packets[i])?.WaitOne(3999);
            }
            catch (Exception ex)
            {
                // Set log
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailSendComandByNameAsync,
                        G9LogIdentity.CLIENT_SEND_DATA, LogMessage.FailedOperation);

                // Run event on error
                OnErrorHandler(ex, ClientErrorReason.ErrorReadyToSendDataToServer);
            }
        }

        #endregion

        #endregion

        #region Send Command By Command

        /// <summary>
        ///     Send command request by command
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

        #region SendCommand

        public void SendCommand<TCommand, TTypeSend>(TTypeSend commandData, Guid? customRequestId = null,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            SendCommandByName(typeof(TCommand).Name, commandData, customRequestId, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send async command request by command
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
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>

        #region SendCommandByNameAsync

        public void SendCommandAsync<TCommand, TTypeSend>(TTypeSend commandData, Guid? customRequestId = null,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            SendCommandByNameAsync(typeof(TCommand).Name, commandData, customRequestId, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        #endregion

        #region Helper Class For Send

        private void SendCommandByName(uint sessionId, string commandName, object commandData,
            Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            SendCommandByName(commandName, commandData, customRequestId, checkCommandExists, checkCommandSendType);
        }

        private void SendCommandByNameAsync(uint sessionId, string commandName, object commandData,
            Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            SendCommandByNameAsync(commandName, commandData, customRequestId, checkCommandExists, checkCommandSendType);
        }

        #endregion

        #endregion
    }
}