using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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

        private int SendCommandByName(uint sessionId, string name, object data)
        {
            return SendCommandByName(name, data);
        }

        private async Task<int> SendCommandByNameAsync(uint sessionId, string name, object data)
        {
            return await SendCommandByNameAsync(name, data);
        }

        #endregion

        #endregion
    }
}