using System;
using System.Net;
using System.Threading.Tasks;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreServer.HelperClass;

namespace G9SuperNetCoreServer.Abstarct
{
    /// <inheritdoc />
    public abstract class AServerSession : ASession
    {
        #region Fields And Properties

        /// <summary>
        ///     Access to session handler
        /// </summary>
        private G9ServerSessionHandler _sessionHandler;

        /// <summary>
        ///     Specified enable command send and receive mode
        /// </summary>
        public bool EnableTestSendReceiveMode { private set; get; }

        #region LastCommand Utilities

        /// <summary>
        ///     Field save last command
        /// </summary>
        private string _lastCommand;

        /// <inheritdoc />
        public override string LastCommand
        {
            protected set
            {
                _lastCommand = value;
                LastCommandDateTime = DateTime.Now;
                PingHandler();
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

        public void InitializeAndHandlerAccountAndSessionAutomaticFirstTime(G9ServerSessionHandler handler,
            uint oSessionId,
            IPAddress oIpAddress)
        {
            // Set session handler
            _sessionHandler = handler;

            // Set ping utilities
            PingDurationInMilliseconds = _sessionHandler.PingDurationInMilliseconds;
            _sessionHandler.SetPing = newPing => Ping = newPing;

            // Set last command
            _sessionHandler.SetLastCommand = lastCommandName => LastCommand = lastCommandName;

            // Test mode utilities
            _sessionHandler.EnableTestMode = EnableTestSendAndReceiveMode;
            _sessionHandler.DisableTestMode = DisableTestSendAndReceiveMode;

            // Set session id
            SessionId = oSessionId;

            // Set ip
            SessionIpAddress = oIpAddress;
        }

        #endregion

        /// <summary>
        ///     Ping handler
        ///     calculate ping automatically
        /// </summary>

        #region PingHandler

        private void PingHandler()
        {
            if ((DateTime.Now - LastPingDateTime).TotalMilliseconds > PingDurationInMilliseconds)
            {
                LastPingDateTime = DateTime.Now;
                SendCommandByNameAsync("G9PingCommand", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            }
        }

        #endregion

        /// <summary>
        ///     Enable test send and receive mode
        /// </summary>
        /// <param name="testMessage">
        ///     Test message
        ///     If set null => $"Test Mode - Session Id: {SessionId}"
        /// </param>

        #region EnableTestSendAndReceiveMode

        private void EnableTestSendAndReceiveMode(string testMessage = null)
        {
            // Set message
            if (string.IsNullOrEmpty(testMessage))
                testMessage = $"Test Mode - Session Id: {SessionId}";
            // Enable test mode
            EnableTestSendReceiveMode = true;
            // Send test mode command
            SendCommandByNameAsync(nameof(G9ReservedCommandName.G9TestSendReceive), testMessage);
        }

        #endregion

        /// <summary>
        ///     Disable  test send and receive mode
        /// </summary>

        #region DisableTestSendAndReceiveMode

        private void DisableTestSendAndReceiveMode()
        {
            // Disable test mode
            EnableTestSendReceiveMode = false;
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandByNameAsync

        public override Task<int> SendCommandByNameAsync(string name, object data)
        {
            return _sessionHandler.SendCommandByNameAsync(SessionId, name, data);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandByName

        public override int SendCommandByName(string name, object data)
        {
            return _sessionHandler.SendCommandByName(SessionId, name, data);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandAsync

        public override Task<int> SendCommandAsync<TCommand, TTypeSend>(TTypeSend data)
        {
            return _sessionHandler.SendCommandByNameAsync(SessionId, nameof(TCommand), data);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommand

        public override int SendCommand<TCommand, TTypeSend>(TTypeSend data)
        {
            return _sessionHandler.SendCommandByName(SessionId, nameof(TCommand), data);
        }

        #endregion

        #endregion
    }
}