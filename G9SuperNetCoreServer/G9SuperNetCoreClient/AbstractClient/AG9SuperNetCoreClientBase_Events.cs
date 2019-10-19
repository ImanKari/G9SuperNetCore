using System;
using G9Common.Delegates;
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
        #region Fields And Properties

        /// <summary>
        ///     Specified client is connected
        /// </summary>
        public bool IsConnected { private set; get; }

        #endregion

        #region Events

        /// <summary>
        ///     Event used for unhandled command
        ///     Call when command not exists
        /// </summary>
        public event G9Delegates<TAccount, ClientStopReason, ClientErrorReason, DisconnectReason>.Unhandled
            OnUnhandledCommand;

        /// <summary>
        ///     Event used for error management
        ///     Call when received error in run time
        /// </summary>
        public event G9Delegates<TAccount, ClientStopReason, ClientErrorReason, DisconnectReason>.Error OnError;

        /// <summary>
        ///     Event connected
        ///     Call when client connected to server
        /// </summary>
        public event G9Delegates<TAccount, ClientStopReason, ClientErrorReason, DisconnectReason>.Connected OnConnected;

        /// <summary>
        ///     Event disconnected
        ///     Call when client disconnected from server
        /// </summary>
        public event G9Delegates<TAccount, ClientStopReason, ClientErrorReason, DisconnectReason>.Disconnected
            OnDisconnected;

        #endregion

        #region Methods

        /// <summary>
        ///     Management error
        /// </summary>
        /// <param name="exceptionError">Exception of error</param>
        /// <param name="errorReason">Reason of error</param>

        #region OnErrorHandler

        private void OnErrorHandler(Exception exceptionError, ClientErrorReason errorReason)
        {
            // run event
            OnError?.Invoke(exceptionError, errorReason);
        }

        #endregion

        /// <summary>
        ///     Management Unhandled commands
        /// </summary>
        /// <param name="packet">Received packet data</param>
        /// <param name="account">account send command</param>

        #region OnUnhandledCommandHandler

        private void OnUnhandledCommandHandler(G9SendAndReceivePacket packet, TAccount account)
        {
            // Set log
            if (_logging.LogIsActive(LogsType.WARN))
                _logging.LogWarning(
                    $"{LogMessage.ReceivedUnhandledCommand}\n{LogMessage.CommandName}: {packet.Command}\n{LogMessage.Body}: {Configuration.EncodingAndDecoding.EncodingType.GetString(packet.Body.Span)}\n{LogMessage.PacketType}: {packet.TypeOfPacketType}",
                    G9LogIdentity.RECEIVE_UNHANDLED_COMMAND, LogMessage.UnhandledCommand);

            // Run event
            OnUnhandledCommand?.Invoke(packet, account);
        }

        #endregion

        /// <summary>
        ///     Management connected
        /// </summary>
        /// <param name="account">Connected account</param>

        #region OnConnectedHandler

        private void OnConnectedHandler(TAccount account)
        {
            // Set is connected
            IsConnected = true;

            // Set connected date time
            ClientConnectedDateTime = DateTime.Now;

            // Run event
            OnConnected?.Invoke(account);
        }

        #endregion

        /// <summary>
        ///     Management disconnected
        /// </summary>
        /// <param name="account">Disconnected account</param>
        /// <param name="disconnectReason">Reason of disconnect</param>

        #region OnDisconnectedHandler

        private void OnDisconnectedHandler(TAccount account, DisconnectReason disconnectReason)
        {
            // Set is connected
            IsConnected = false;

            try
            {
                // Run on session close in account
                MainAccount?.OnSessionClosed(disconnectReason);
            }
            catch
            {
                // Ignore
            }

            // Run event
            OnDisconnected?.Invoke(account, disconnectReason);
        }

        #endregion

        #endregion
    }
}