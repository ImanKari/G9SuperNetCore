using System;
using System.Net;
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
        ///     Field save last command
        /// </summary>
        private string _lastCommand;

        /// <summary>
        ///     Specified last command use
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

        public override void SendCommandByNameAsync(string commandName, object commandData,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            _sessionHandler.Session_SendCommandByNameAsync(SessionId, commandName, commandData, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandByName

        public override void SendCommandByName(string commandName, object commandData, bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            _sessionHandler.Session_SendCommandByName(SessionId, commandName, commandData, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandAsync

        public override void SendCommandAsync<TCommand, TTypeSend>(TTypeSend commandData,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            _sessionHandler.Session_SendCommandByNameAsync(SessionId, typeof(TCommand).Name, commandData,
                checkCommandExists, checkCommandSendType);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommand

        public override void SendCommand<TCommand, TTypeSend>(TTypeSend commandData, bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            _sessionHandler.Session_SendCommandByName(SessionId, typeof(TCommand).Name, commandData, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        #endregion
    }
}