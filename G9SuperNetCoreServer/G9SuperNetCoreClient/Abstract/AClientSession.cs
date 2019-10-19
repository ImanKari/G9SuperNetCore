using System;
using System.Net;
using System.Threading.Tasks;
using G9Common.Abstract;
using G9SuperNetCoreClient.Helper;

namespace G9SuperNetCoreClient.Abstract
{
    /// <inheritdoc />
    public abstract class AClientSession : ASession
    {
        #region Fields And Properties

        /// <summary>
        ///     Access to session handler
        /// </summary>
        private G9ClientSessionHandler _sessionHandler;

        #region LastCommand Utilities

        /// <summary>
        /// Field save last command
        /// </summary>
        private string _lastCommand;

        /// <summary>
        /// Specified last command use
        /// </summary>
        public override string LastCommand
        {
            protected set
            {
                _lastCommand = value;
                LastCommandDateTime = DateTime.Now;
            }
            get => _lastCommand;
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        ///     Initialize and handle session
        ///     Automatic use with core
        /// </summary>

        #region InitializeAndHandlerAccountAndSessionAutomaticFirstTime

        public void InitializeAndHandlerAccountAndSessionAutomaticFirstTime(G9ClientSessionHandler handler,
            uint oSessionId,
            IPAddress oIpAddress)
        {
            // Set session handler
            _sessionHandler = handler;

            // Set ping utilities
            PingDurationInMilliseconds = _sessionHandler.PingDurationInMilliseconds;
            _sessionHandler.Core_SetPing = newPing => Ping = newPing;

            // Plus send and receive and packets for current session
            _sessionHandler.Core_PlusSessionTotalReceiveBytes = receiveBytes =>
            {
                SessionTotalReceiveBytes += receiveBytes;
                SessionTotalReceivePacket++;
            };
            _sessionHandler.Core_PlusSessionTotalSendBytes = sendBytes =>
            {
                SessionTotalSendBytes += sendBytes;
                SessionTotalSendPacket++;
            };

            // Set session id
            SessionId = oSessionId;

            // Set ip
            SessionIpAddress = oIpAddress;
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandByNameAsync

        public override Task<int> SendCommandByNameAsync(string name, object data)
        {
            return _sessionHandler.Session_SendCommandByNameAsync(SessionId, name, data);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandByName

        public override int SendCommandByName(string name, object data)
        {
            return _sessionHandler.Session_SendCommandByName(SessionId, name, data);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandAsync

        public override Task<int> SendCommandAsync<TCommand, TTypeSend>(TTypeSend data)
        {
            return _sessionHandler.Session_SendCommandByNameAsync(SessionId, nameof(TCommand), data);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommand

        public override int SendCommand<TCommand, TTypeSend>(TTypeSend data)
        {
            return _sessionHandler.Session_SendCommandByName(SessionId, nameof(TCommand), data);
        }

        #endregion

        #endregion
    }
}