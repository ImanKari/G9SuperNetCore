using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using G9Common.HelperClass;
using G9Common.JsonHelper;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Enums;

namespace G9SuperNetCoreClient.AbstractClient
{
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
                    var remoteEndPoint = new IPEndPoint(Configuration.IpAddress, Configuration.PortNumber);

                    // Create a TCP/IP socket.  
                    var client = new Socket(Configuration.IpAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect to the remote endpoint.  
                    client.BeginConnect(remoteEndPoint,
                        ConnectCallback, client);
                    _connectDone.WaitOne();

                    // Run event on connected handler
                    OnConnectedHandler(_mainAccountUtilities.Account);

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
                    OnDisconnectedHandler(_mainAccountUtilities.Account, DisconnectReason.DisconnectedByProgram);

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
                Configuration.EncodingAndDecoding.EncodingType.GetBytes(
                    commandName.GenerateStandardCommandName(_packetManagement.CalculateCommandSize));

            return _packetManagement.PackingRequestByData(commandData, dataForSend);
        }

        #endregion

        #region Send Command By Name

        /// <summary>
        ///     Send command request by name
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

        #region SendCommandByName

        public int SendCommandByName(string commandName, object commandData, bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            // Set send data
            var sendBytes = 0;
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
                var dataForSend = ReadyDataForSend(commandName, commandData);

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

        public async Task<int> SendCommandByNameAsync(string commandName, object commandData,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            return await Task.Run(() =>
            {
                // Set send data
                var sendBytes = 0;
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
                    var dataForSend = ReadyDataForSend(commandName, commandData);

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

        #endregion

        #region Send Command By Command

        /// <summary>
        ///     Send command request by command
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

        #region SendCommand

        public int SendCommand<TCommand, TTypeSend>(TTypeSend commandData, bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            return SendCommandByName(typeof(TCommand).Name, commandData, checkCommandExists, checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send async command request by command
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
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>

        #region SendCommandByNameAsync

        public async Task<int> SendCommandAsync<TCommand, TTypeSend>(TTypeSend commandData,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            return await SendCommandByNameAsync(typeof(TCommand).Name, commandData, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        #endregion

        #region Helper Class For Send

        private int SendCommandByName(uint sessionId, string commandName, object commandData,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            return SendCommandByName(commandName, commandData, checkCommandExists, checkCommandSendType);
        }

        private async Task<int> SendCommandByNameAsync(uint sessionId, string commandName, object commandData,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            return await SendCommandByNameAsync(commandName, commandData, checkCommandExists, checkCommandSendType);
        }

        #endregion

        #endregion
    }
}