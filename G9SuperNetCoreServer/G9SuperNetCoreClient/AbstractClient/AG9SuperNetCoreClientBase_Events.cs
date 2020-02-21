using System;
using System.Threading;
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

        /// <summary>
        ///     <para>Event reconnect</para>
        ///     <para>Call when client disconnected and try for reconnect.</para>
        /// </summary>
        public event G9Delegates<TAccount, ClientStopReason, ClientErrorReason, DisconnectReason>.Reconnecting
            OnReconnect;

        /// <summary>
        ///     <para>Event unable to connect</para>
        ///     <para>Call when client disconnect and try for reconnect failed!</para>
        ///     <para>Call after try reconnect</para>
        /// </summary>
        public event G9Delegates<TAccount, ClientStopReason, ClientErrorReason, DisconnectReason>.UnableToConnect
            OnUnableToConnect;

        #endregion

        #region Methods

        /// <summary>
        ///     Management error
        /// </summary>
        /// <param name="exceptionError">Exception of error</param>
        /// <param name="errorReason">Reason of error</param>
        /// <param name="tryForReconnect">If true try for reconnect to server</param>

        #region OnErrorHandler

        private void OnErrorHandler(Exception exceptionError, ClientErrorReason errorReason,
            bool tryForReconnect = true)
        {
            // run event
            OnError?.Invoke(exceptionError, errorReason);

            // Try for reconnect
            if (tryForReconnect)
                OnReconnectHandler(_mainAccountUtilities.Account);
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
            if (_logging.CheckLoggingIsActive(LogsType.WARN))
                _logging.LogWarning(
                    $"{LogMessage.ReceivedUnhandledCommand}\n{LogMessage.CommandName}: {packet.Command}\n{LogMessage.Body}: {Configuration.EncodingAndDecoding.GetString(packet.Body)}\n{LogMessage.PacketType}: {packet.PacketType}",
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
        /// <param name="tryForReconnect">Specified need try for reconnect</param>

        #region OnDisconnectedHandler

        private void OnDisconnectedHandler(TAccount account, DisconnectReason disconnectReason, bool tryForReconnect = true)
        {
            // Set is connected
            IsConnected = false;

            try
            {
                // Run on session close in account
                MainAccount?.OnSessionClosed(disconnectReason);
                // Run event
                OnDisconnected?.Invoke(account, disconnectReason);
                if (tryForReconnect
#if UNITY_2018_1_OR_NEWER
                    && G9SuperNetCoreClient4Unity.GameIsPlaying
#endif
                    )
                    // Reconnect
                    OnReconnectHandler(account);
                else
                    // Disconnect if server is connected
                    Disconnect().Wait(3999);
            }
            catch (Exception ex)
            {
                // Ignore
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, $"Receive exception in {nameof(OnDisconnectedHandler)}",
                        nameof(OnDisconnectedHandler), nameof(OnDisconnectedHandler));
            }

            if (tryForReconnect)
                OnReconnectHandler(account);
        }

        #endregion

        /// <summary>
        ///     Management reconnect
        /// </summary>
        /// <param name="account">Connected account</param>

        #region OnReconnectHandler

        private void OnReconnectHandler(TAccount account)
        {
            var forceReconnect = false;
            while (true)
            {
                // If enable auto reconnect and try count greater than zero - try for reconnect
                // If try count equal zero or auto reconnect is disable
                if (!Configuration.AutoReconnect || _reconnectTryCount <= 0)
                {
                    // Check flag use unable to connect is false
                    if (!_unableToConnectFlag)
                    {
                        _unableToConnectFlag = true;
                        _reconnectModeEnable = false;
                        OnUnableToConnectHandler();
                    }
                    break;
                }

                // else try for reconnect
                // If reconnect enable And forceReconnect is false  => reject reconnect request
                if (_reconnectModeEnable && !forceReconnect)
                    break;

                // If game playing is false (For unity) => reject reconnect request
#if UNITY_2018_1_OR_NEWER
                if (!G9SuperNetCoreClient4Unity.GameIsPlaying)
                    break;
#endif

                // Set reconnect mode true
                _reconnectModeEnable = true;

                // Call reconnect event
                OnReconnect?.Invoke(account, _reconnectTryCount);

                // Check first time or no => set delay for try reconnect
                if (Configuration.ReconnectTryCount == _reconnectTryCount)
                    // if first time => 30% of duration for try to reconnect
                    Thread.Sleep((ushort)(Configuration.ReconnectDuration * 0.3));
                else
                    // In other time => full duration for try to reconnect
                    Thread.Sleep(Configuration.ReconnectDuration);

                // Minus try reconnect
                _reconnectTryCount--;

                // Reconnect again
                var restart = StartConnection();

                if (restart.Wait(Configuration.ReconnectDuration) && restart.Result)
                {
                    _reconnectModeEnable = false;
                    break;
                }

                forceReconnect = true;
            }
        }

        #endregion

        /// <summary>
        ///     Management unable to connect
        /// </summary>

        #region OnUnableToConnectHandler

        private void OnUnableToConnectHandler()
        {
            // Call reconnect event
            OnUnableToConnect?.Invoke();
        }

        #endregion

        #endregion
    }
}