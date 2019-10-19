using System;
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
        ///     Access to action send command by name
        ///     long => session id
        ///     string => command name
        ///     object => data for send
        /// </summary>
        private readonly Func<uint, string, object, int> _sendCommandByName;

        /// <summary>
        ///     Access to action send command by name async
        ///     long => session id
        ///     string => command name
        ///     object => data for send
        /// </summary>
        private readonly Func<uint, string, object, Task<int>> _sendCommandByNameAsync;

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
        /// <param name="onUnhandledCommand">Specified event on unhandled command</param>
        /// <param name="customLogging">Specified custom logging system</param>

        #region G9Core

        public G9Core(G9ServerConfig superNetCoreConfig, Assembly commandAssembly,
            Func<uint, string, object, int> sendCommandByName,
            Func<uint, string, object, Task<int>> sendCommandByNameAsync,
            Action<G9SendAndReceivePacket, TAccount> onUnhandledCommand, IG9Logging customLogging = null)
        {
            // TODO: change fixed array to change sizable array
            // Set array
            _accountCollection =
                new G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>[10000];

            // Set logging system
            Logging = customLogging ?? new G9LoggingServer();

            // Set send command
            _sendCommandByName = sendCommandByName;
            _sendCommandByNameAsync = sendCommandByNameAsync;

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

        /// <summary>
        ///     Disconnect session handler
        ///     remove from collections
        /// </summary>
        /// <param name="account">Specified account</param>
        /// <param name="disconnectReason">Specify disconnect reason</param>

        #region DisconnectSocketHandler

        public void DisconnectSocketHandler(TAccount account, DisconnectReason disconnectReason)
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
        ///     Clear all socket and account
        ///     clear all collections
        /// </summary>
        /// <param name="disconnectReason">Specify disconnect reason</param>

        #region ClearAllSocketsAndAccounts

        public void ClearAllSocketsAndAccounts(DisconnectReason disconnectReason)
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

        #region GenerateAccountByAcceptedSocket

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
                            SendCommandByName = _sendCommandByName,
                            SendCommandByNameAsync = _sendCommandByNameAsync,
                            PingDurationInMilliseconds = (ushort) Configuration.GetPingTimeOut.TotalMilliseconds
                        }, _sessionIdentityCounter,
                    ((IPEndPoint) acceptedScoket.RemoteEndPoint).Address);

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