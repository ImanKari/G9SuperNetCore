using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using G9Common.CommandHandler;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Config;
using G9SuperNetCoreServer.Enums;
using G9SuperNetCoreServer.HelperClass;
using G9SuperNetCoreServer.Logging;

namespace G9SuperNetCoreServer.Core
{
    internal class G9Core<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        #region Fields And Properties

        /// <summary>
        ///     Access to server configuration
        /// </summary>
        public readonly G9ServerConfig Configuration;

        /// <summary>
        ///     Concurrent collection for save account and sessions
        ///     index: session id
        ///     Val: account utilities
        /// </summary>
        private readonly G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>[]
            _accountCollection;

        /// <summary>
        ///     Save maximum connection number
        /// </summary>
        private readonly uint _maximumConnectionCounter = 0;

        /// <summary>
        ///     Static field for save session identity counter
        /// </summary>
        private uint _sessionIdentityCounter = 1;

        /// <summary>
        ///     Access to logging system
        /// </summary>
        public readonly IG9Logging Logging;

        /// <summary>
        ///     Access to command handler
        /// </summary>
        public readonly G9CommandHandler<TAccount> CommandHandler;

        /// <summary>
        ///     Access to func send command by name
        ///     long => session id
        ///     string => command name
        ///     bool => If set true, check command exists
        ///     object => data for send
        /// </summary>
        private readonly Action<uint, string, object, bool, bool> _sendCommandByName;

        /// <summary>
        ///     Access to func send command by name async
        ///     long => session id
        ///     string => command name
        ///     bool => If set true, check command exists
        ///     object => data for send
        /// </summary>
        private readonly Action<uint, string, object, bool, bool> _sendCommandByNameAsync;

        /// <summary>
        ///     Access to event OnSessionReceiveRequestOverTheLimitInSecond 
        /// </summary>
        private readonly Action<TAccount> _onSessionReceiveRequestOverTheLimitInSecond;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize Requirement
        /// </summary>
        /// <param name="superNetCoreConfig">Server config</param>
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        /// <param name="sendCommandByName">Specify func send command by name</param>
        /// <param name="sendCommandByNameAsync">Specify func send command by name async</param>
        /// <param name="onSessionReceiveRequestOverTheLimitInSecond">Specify action for event OnSessionReceiveRequestOverTheLimitInSecond</param>
        /// <param name="onUnhandledCommand">Specified event on unhandled command</param>
        /// <param name="customLogging">Specified custom logging system</param>

        #region G9Core

        public G9Core(G9ServerConfig superNetCoreConfig, Assembly commandAssembly,
            Action<uint, string, object, bool, bool> sendCommandByName,
            Action<uint, string, object, bool, bool> sendCommandByNameAsync,
            Action<TAccount> onSessionReceiveRequestOverTheLimitInSecond,
            Action<G9SendAndReceivePacket, TAccount> onUnhandledCommand, IG9Logging customLogging = null)
        {
            // TODO: change fixed array to change sizable array
            // Set array
            _accountCollection =
                new G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>[100000];

            // Set logging system
            Logging = customLogging ?? new G9LoggingServer();

            // Set send command
            _sendCommandByName = sendCommandByName;
            _sendCommandByNameAsync = sendCommandByNameAsync;
            _onSessionReceiveRequestOverTheLimitInSecond = onSessionReceiveRequestOverTheLimitInSecond;

            // Set configuration
            Configuration = superNetCoreConfig;

            // Initialize command handler
            CommandHandler = new G9CommandHandler<TAccount>(commandAssembly, Logging, Configuration.CommandSize,
                onUnhandledCommand);
        }

        #endregion

        /// <summary>
        ///     Accept connection by connection socket
        ///     Initialize requirement
        ///     Create new account and session and add to collection
        /// </summary>
        /// <param name="connectionSocket"></param>
        /// <returns></returns>

        #region TryAcceptConnection

        public (bool, TAccount) TryAcceptConnection(Socket connectionSocket)
        {
            if (_maximumConnectionCounter >= Configuration.MaxConnectionNumber)
            {
                if (Logging.LogIsActive(LogsType.WARN))
                    Logging.LogWarning(
                        "On step accept connection, reject connection.\nReason: limit max connection number",
                        G9LogIdentity.ACCEPT_CONNECTION, "Reject connection");

                // TODO: فرمت ارسال باید برای کامند مناسب درست بشه
                connectionSocket.Send(
                    Configuration.EncodingAndDecoding.EncodingType.GetBytes(
                        G9ServerCommandMessage.REJECT_MAX_CONNECTION));
                connectionSocket.Disconnect(false);
                connectionSocket.Close();
                connectionSocket.Dispose();
                return (false, null);
            }

            var newAccountUtilities = CreateAccountByAcceptedSocket(connectionSocket);

            if (newAccountUtilities != null)
            {
                if (Logging.LogIsActive(LogsType.INFO))
                    Logging.LogInformation(LogMessage.AccountAndSessionCreated, G9LogIdentity.CREATE_NEW_ACCOUNT,
                        LogMessage.CreateNewAccount);

                _accountCollection[_sessionIdentityCounter++] = newAccountUtilities;

                if (Logging.LogIsActive(LogsType.INFO))
                    Logging.LogInformation(LogMessage.SuccessAccountAdded, G9LogIdentity.CREATE_NEW_ACCOUNT,
                        LogMessage.CreateNewAccount);
                return (true, newAccountUtilities.Account);
            }

            // TODO: فرمت ارسال باید برای کامند مناسب درست بشه
            connectionSocket.Send(
                Configuration.EncodingAndDecoding.EncodingType.GetBytes(G9ServerCommandMessage.REJECT_SERVER_ERROR));
            connectionSocket.Disconnect(false);
            connectionSocket.Close();
            connectionSocket.Dispose();
            return (false, null);
        }

        #endregion

        #region DisconnectAndCloseSession - Overides

        /// <summary>
        ///     Disconnect and close session
        ///     remove from collection and dispose resource
        /// </summary>
        /// <param name="account">Specified account</param>
        /// <param name="disconnectReason">Specify disconnect reason</param>

        #region DisconnectAndCloseSession

        public void DisconnectAndCloseSession(TAccount account, DisconnectReason disconnectReason)
        {
            // Run on session closed in account
            try
            {
                _accountCollection[account.Session.SessionId]?.Account.OnSessionClosed(disconnectReason);
            }
            catch
            {
                // Ignore
            }

            // Dispose and remove
            _accountCollection[account.Session.SessionId]?.SessionSocket.Dispose();
            _accountCollection[account.Session.SessionId] = null;
            // Gc collect
            GC.Collect();
        }

        #endregion

        /// <summary>
        ///     Disconnect and close session
        ///     remove from collection and dispose resource
        /// </summary>
        /// <param name="sessionId">Specified session id</param>
        /// <param name="disconnectReason">Specify disconnect reason</param>

        #region DisconnectAndCloseSession

        public void DisconnectAndCloseSession(uint sessionId, DisconnectReason disconnectReason)
        {
            // Run on session closed in account
            try
            {
                _accountCollection[sessionId]?.Account.OnSessionClosed(disconnectReason);
            }
            catch
            {
                // Ignore
            }

            // Dispose and remove
            _accountCollection[sessionId]?.SessionSocket.Dispose();
            _accountCollection[sessionId] = null;
            // Gc collect
            GC.Collect();
        }

        #endregion

        #endregion

        /// <summary>
        ///     Clear all socket and account
        ///     clear all collections
        /// </summary>
        /// <param name="disconnectReason">Specify disconnect reason</param>

        #region ClearAllAccountsAndSessions

        public void ClearAllAccountsAndSessions(DisconnectReason disconnectReason)
        {
            for (var i = 0; i < _accountCollection.Length; i++)
            {
                // Run on session closed in account
                try
                {
                    _accountCollection[i]?.Account?.OnSessionClosed(disconnectReason);
                }
                catch
                {
                    // Ignore
                }

                // Dispose session socket
                _accountCollection[i]?.SessionSocket.Dispose();
                // Remove all
                _accountCollection[i] = null;
            }

            // Gc collect
            GC.Collect();
        }

        #endregion

        /// <summary>
        ///     Create new account and session by connection socket
        /// </summary>
        /// <param name="acceptedScoket">connection socket</param>
        /// <returns>Created account</returns>

        #region CreateAccountByAcceptedSocket

        private G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>
            CreateAccountByAcceptedSocket(Socket acceptedScoket)
        {
            try
            {
                // Initialize result
                var result = new G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>();

                // Instance session and pass requirement to constructor
                var session = new TSession();
                session.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(result.SessionHandler =
                        new G9ServerSessionHandler
                        {
                            // Set send commands
                            Session_SendCommandByName = _sendCommandByName,
                            Session_SendCommandByNameAsync = _sendCommandByNameAsync,
                            // Set ping duration
                            PingDurationInMilliseconds = (ushort) Configuration.GetPingTimeOut.TotalMilliseconds,
                            // Set event
                            Session_OnSessionReceiveRequestOverTheLimitInSecond = sessionId => GetAccountUtilitiesBySessionId(sessionId)

                        }, _sessionIdentityCounter,
                    ((IPEndPoint) acceptedScoket.RemoteEndPoint).Address);
                
                // ########### Set requirement###########
                // Set max request
                result.SessionHandler.Core_SetMaxRequestRequirement(Configuration.MaxRequestPerSecond);

                // Instance account and pass session to constructor
                var newAccount = result.Account = new TAccount();
                newAccount.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(result.AccountHandler =
                    new G9ServerAccountHandler(), session);

                // Set socket
                result.SessionSocket = acceptedScoket;

                // return result
                return result;
            }
            catch (Exception ex)
            {
                // set ex log
                if (Logging.LogIsActive(LogsType.EXCEPTION))
                    Logging.LogException(ex, LogMessage.ProblemCreateNewAccountAndSession,
                        G9LogIdentity.CREATE_NEW_ACCOUNT, LogMessage.CreateNewAccount);

                return null;
            }
        }

        #endregion

        /// <summary>
        ///     Scrolling all account utilities
        /// </summary>
        /// <param name="scrollingSocketAction">Action for scrolling</param>

        #region ScrollingAllAccountUtilities

        public void ScrollingAllAccountUtilities(
            Action<G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>> scrollingSocketAction)
        {
            for (var i = 0; i < _accountCollection.Length; i++)
                if (_accountCollection[i] != null)
                    scrollingSocketAction?.Invoke(_accountCollection[i]);
        }

        #endregion

        /// <summary>
        ///     Select account utilities with func
        /// </summary>
        /// <param name="predicate">Predicate func</param>

        #region SelectAccountUtilities

        public IEnumerable<TResult> SelectAccountUtilities<TResult>(
            Func<IList<G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>>,
                IEnumerable<TResult>> predicate)
        {
            return predicate?.Invoke(_accountCollection);
        }

        #endregion

        /// <summary>
        ///     Get account utilities
        /// </summary>
        /// <param name="sessionId">Session id for socket</param>
        /// <returns>Socket of session id</returns>

        #region GetSocketBySessionId

        public G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>
            GetAccountUtilitiesBySessionId(uint sessionId)
        {
            if (sessionId != 0 && sessionId < _accountCollection.Length)
                return _accountCollection[sessionId];
            return null;
        }

        #endregion

        #endregion
    }
}