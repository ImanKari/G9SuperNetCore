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

        #region Max Request Utilities

        /// <summary>
        /// Specified check max request is enable
        /// </summary>
        private bool _enableCheckMaxRequest;
        /// <summary>
        ///     Save date time for check max request
        ///     one second previous
        /// </summary>
        private DateTime _checkMaxRequestDateTime;
        /// <summary>
        /// Specify counter for max request
        /// </summary>
        private ushort _counterForMaxRequest;
        /// <summary>
        ///     Specify maximum request from client per second
        ///     Set 0 => infinity
        /// </summary>
        private ushort _maxRequestPerSecond;

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

            // Set check max request
            _sessionHandler.Core_SetMaxRequestRequirement = maximumRequestPerSecond =>
            {
                if (maximumRequestPerSecond == 0)
                {
                    _enableCheckMaxRequest = false;
                    return;
                }
                else
                {
                    _enableCheckMaxRequest = true;
                    _maxRequestPerSecond = maximumRequestPerSecond;
                }
            };

            // Set last command
            _sessionHandler.Core_SetLastCommand = lastCommandName => LastCommand = lastCommandName;

            // Test mode utilities
            _sessionHandler.Core_EnableTestMode = EnableTestSendAndReceiveMode;
            _sessionHandler.Core_DisableTestMode = DisableTestSendAndReceiveMode;

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

        public override Task<int> SendCommandByNameAsync(string commandName, object commandData, bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            return _sessionHandler.Session_SendCommandByNameAsync(SessionId, commandName, commandData, checkCommandExists, checkCommandSendType);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandByName

        public override int SendCommandByName(string commandName, object commandData, bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            return _sessionHandler.Session_SendCommandByName(SessionId, commandName, commandData, checkCommandExists, checkCommandSendType);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommandAsync

        public override Task<int> SendCommandAsync<TCommand, TTypeSend>(TTypeSend commandData, bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            return _sessionHandler.Session_SendCommandByNameAsync(SessionId, typeof(TCommand).Name, commandData, checkCommandExists, checkCommandSendType);
        }

        #endregion

        /// <inheritdoc />

        #region SendCommand

        public override int SendCommand<TCommand, TTypeSend>(TTypeSend commandData, bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            return _sessionHandler.Session_SendCommandByName(SessionId, typeof(TCommand).Name, commandData, checkCommandExists, checkCommandSendType);
        }

        #endregion

        /// <summary>
        /// Check max request over the limit in second
        /// </summary>

        #region CheckMaxRequestOverTheLimitInSecond

        public void CheckMaxRequestOverTheLimitInSecond()
        {
            if (_enableCheckMaxRequest && !EnableTestSendReceiveMode)
            {
                // Checked over flow
                try
                {
                    checked
                    {
                        _counterForMaxRequest++;
                    }
                }
                catch
                {
                    _counterForMaxRequest = ushort.MaxValue;
                }

                // check if time higher than one second
                if ((DateTime.Now - _checkMaxRequestDateTime).TotalSeconds > 1)
                {
                    _checkMaxRequestDateTime = DateTime.Now;

                    // Check counter with max request limit
                    if (_counterForMaxRequest > _maxRequestPerSecond)
                    {
                        // If Receive Request Over The Limit In Second
                        _sessionHandler?.Session_OnSessionReceiveRequestOverTheLimitInSecond?.Invoke(SessionId);
                    }

                    _counterForMaxRequest = 0;
                }
            }
        }

        #endregion

        #endregion
    }
}