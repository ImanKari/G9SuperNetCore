using System;
using G9Common.Delegates;
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
        #region Fields And Properties

        /// <summary>
        ///     Specified server is started
        /// </summary>
        public bool IsStarted { private set; get; }

        #endregion

        #region Events

        /// <summary>
        ///     Event used for unhandled command
        ///     Call when command not exists
        /// </summary>
        public event G9Delegates<TAccount, ServerStopReason, ServerErrorReason, DisconnectReason>.Unhandled
            OnUnhandledCommand;

        /// <summary>
        ///     Event used for error management
        ///     Call when received error in run time
        /// </summary>
        public event G9Delegates<TAccount, ServerStopReason, ServerErrorReason, DisconnectReason>.Error OnError;

        /// <summary>
        ///     Event stop
        ///     Call when server stopped
        /// </summary>
        public event G9Delegates<TAccount, ServerStopReason, ServerErrorReason, DisconnectReason>.Stop OnStop;

        /// <summary>
        ///     Event start
        ///     Call when server started
        /// </summary>
        public event G9Delegates<TAccount, ServerStopReason, ServerErrorReason, DisconnectReason>.Start OnStart;

        /// <summary>
        ///     Event connected
        ///     Call when a new client connected to server
        /// </summary>
        public event G9Delegates<TAccount, ServerStopReason, ServerErrorReason, DisconnectReason>.Connected OnConnected;

        /// <summary>
        ///     Event disconnected
        ///     Call when a old client disconnected from server
        /// </summary>
        public event G9Delegates<TAccount, ServerStopReason, ServerErrorReason, DisconnectReason>.Disconnected
            OnDisconnected;

        #endregion

        #region Methods

        /// <summary>
        ///     Management start
        /// </summary>

        #region OnStartHandler

        private void OnStartHandler()
        {
            // Set is started to true
            IsStarted = true;
            // Run event
            OnStart?.Invoke();
        }

        #endregion

        /// <summary>
        ///     Management stop
        /// </summary>
        /// <param name="stopReason">Reason of stop</param>

        #region OnStopHandler

        private void OnStopHandler(ServerStopReason stopReason)
        {
            // Set is started to false
            IsStarted = false;

            // Clear all sessions
            _core.ClearAllSocketsAndAccounts(DisconnectReason.ServerStopped);

            // Run event
            OnStop?.Invoke(stopReason);
        }

        #endregion

        /// <summary>
        ///     Management error
        /// </summary>
        /// <param name="exceptionError">Exception of error</param>
        /// <param name="errorReason">Reason of error</param>

        #region OnErrorHandler

        private void OnErrorHandler(Exception exceptionError, ServerErrorReason errorReason)
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
            if (_core.Logging.LogIsActive(LogsType.WARN))
                _core.Logging.LogWarning(
                    $"{LogMessage.ReceivedUnhandledCommand}\n{LogMessage.CommandName}: {packet.Command}\n{LogMessage.Body}: {_core.Configuration.EncodingAndDecoding.EncodingType.GetString(packet.Body.Span)}\n{LogMessage.PacketType}: {packet.TypeOfPacketType}",
                    G9LogIdentity.RECEIVE_UNHANDLED_COMMAND, LogMessage.UnhandledCommand);

            // Run event
            OnUnhandledCommand?.Invoke(packet, account);
        }

        #endregion

        /// <summary>
        ///     Management new connection
        /// </summary>
        /// <param name="account">Connected account</param>

        #region OnConnectedHandler

        private void OnConnectedHandler(TAccount account)
        {
            // Run event
            OnConnected?.Invoke(account);
        }

        #endregion

        /// <summary>
        ///     Management disconnect client
        /// </summary>
        /// <param name="account">Connected account</param>

        #region OnConnectedHandler

        private void OnDisconnectedHandler(TAccount account, DisconnectReason disconnectReason)
        {
            // Server disconnect handler
            _core.DisconnectSocketHandler(account, disconnectReason);

            // Run event
            OnDisconnected?.Invoke(account, disconnectReason);
        }

        #endregion

        #endregion
    }
}