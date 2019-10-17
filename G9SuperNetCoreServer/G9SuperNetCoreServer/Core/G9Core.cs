using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using G9Common.CommandHandler;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.LogIdentity;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Config;
using G9SuperNetCoreServer.HelperClass;
using G9SuperNetCoreServer.Logging;
using G9SuperNetCoreServer.ServerDefaultCommand;

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
        ///     Key: session id
        ///     Val: account
        /// </summary>
        private readonly ConcurrentDictionary<long, TAccount> _accountCollection =
            new ConcurrentDictionary<long, TAccount>();

        /// <summary>
        ///     Concurrent collection for save socket connection
        ///     Key: session id
        ///     Val: socket connection
        /// </summary>
        private readonly ConcurrentDictionary<long, Socket> _socketCollection =
            new ConcurrentDictionary<long, Socket>();

        /// <summary>
        ///     Save maximum connection number
        /// </summary>
        private readonly int _maximumConnectionCounter = 0;

        /// <summary>
        ///     Static field for save session identity counter
        /// </summary>
        private long _sessionIdentityCounter;

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
        private readonly Func<long, string, object, int> _sendCommandByName;

        /// <summary>
        ///     Access to action send command by name async
        ///     long => session id
        ///     string => command name
        ///     object => data for send
        /// </summary>
        private readonly Func<long, string, object, Task<int>> _sendCommandByNameAsync;

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
        /// <param name="customLogging">Specified custom logging system</param>

        #region G9Core

        public G9Core(G9ServerConfig superNetCoreConfig, Assembly commandAssembly,
            Func<long, string, object, int> sendCommandByName,
            Func<long, string, object, Task<int>> sendCommandByNameAsync, IG9Logging customLogging = null)
        {
            // Set logging system
            Logging = customLogging ?? new G9LoggingServer();

            // Set send command
            _sendCommandByName = sendCommandByName;
            _sendCommandByNameAsync = sendCommandByNameAsync;

            // Set configuration
            Configuration = superNetCoreConfig;

            // Initialize command handler
            CommandHandler = new G9CommandHandler<TAccount>(commandAssembly, Logging, Configuration.CommandSize);

            // ######################## Add default command ########################
            // G9 Echo Command
            CommandHandler.AddCustomCommand<string>(G9EchoCommand.G9CommandName, G9EchoCommand.ReceiveHandler,
                G9EchoCommand.ErrorHandler);
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

        public (bool, long) TryAcceptConnection(Socket connectionSocket)
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
                return (false, -1);
            }

            var newAccount = CreateAccountByAcceptedSocket(connectionSocket);

            if (newAccount != null)
            {
                if (Logging.LogIsActive(LogsType.INFO))
                    Logging.LogInformation(LogMessage.AccountAndSessionCreated, G9LogIdentity.CREATE_NEW_ACCOUNT,
                        LogMessage.CreateNewAccount);
                // Try add account to collection
                if (_accountCollection.TryAdd(_sessionIdentityCounter, newAccount))
                {
                    if (Logging.LogIsActive(LogsType.INFO))
                        Logging.LogInformation(LogMessage.SuccessAccountAdded, G9LogIdentity.CREATE_NEW_ACCOUNT,
                            LogMessage.CreateNewAccount);
                    return (true, _sessionIdentityCounter++);
                }

                if (Logging.LogIsActive(LogsType.ERROR))
                    Logging.LogError(LogMessage.ProblemAddingAccount, G9LogIdentity.ACCEPT_CONNECTION,
                        LogMessage.AccountAdded);

                // TODO: فرمت ارسال باید برای کامند مناسب درست بشه
                connectionSocket.Send(
                    Configuration.EncodingAndDecoding.EncodingType.GetBytes(G9ServerCommandMessage
                        .REJECT_SERVER_ERROR));
                connectionSocket.Disconnect(false);
                connectionSocket.Close();
                connectionSocket.Dispose();
                return (false, -1);
            }

            // TODO: فرمت ارسال باید برای کامند مناسب درست بشه
            connectionSocket.Send(
                Configuration.EncodingAndDecoding.EncodingType.GetBytes(G9ServerCommandMessage.REJECT_SERVER_ERROR));
            connectionSocket.Disconnect(false);
            connectionSocket.Close();
            connectionSocket.Dispose();
            return (false, -1);
        }

        #endregion

        /// <summary>
        ///     Create new account and session by connection socket
        /// </summary>
        /// <param name="acceptedScoket">connection socket</param>
        /// <returns>Created account</returns>

        #region GenerateAccountByAcceptedSocket

        private TAccount CreateAccountByAcceptedSocket(Socket acceptedScoket)
        {
            try
            {
                // Save socket connection
                _socketCollection.TryAdd(_sessionIdentityCounter, acceptedScoket);

                // Instance session and pass requirement to constructor
                var session = new TSession();
                session.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(new G9ServerSessionHandler()
                    {
                        // Set send commands
                        SendCommandByName = _sendCommandByName,
                        SendCommandByNameAsync = _sendCommandByNameAsync
                    },
                    _sessionIdentityCounter, ((IPEndPoint) acceptedScoket.RemoteEndPoint).Address);

                // Instance account and pass session to constructor
                var newAccount = new TAccount();
                newAccount.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(new G9ServerAccountHandler(),
                    session);

                // Plus session identity
                _sessionIdentityCounter++;

                // return result
                return newAccount;
            }
            catch (Exception ex)
            {
                // Remove if add to collection
                _socketCollection.TryRemove(_sessionIdentityCounter, out _);
                _accountCollection.TryRemove(_sessionIdentityCounter, out _);

                // set ex log
                if (Logging.LogIsActive(LogsType.EXCEPTION))
                    Logging.LogException(ex, LogMessage.ProblemCreateNewAccountAndSession,
                        G9LogIdentity.CREATE_NEW_ACCOUNT, LogMessage.CreateNewAccount);

                return null;
            }
        }

        #endregion

        /// <summary>
        ///     Scrolling all socket connection
        /// </summary>
        /// <param name="scrollingSocketAction">Action for scrolling</param>

        #region ScrollingAllSocket

        public void ScrollingAllSocket(Action<Socket> scrollingSocketAction)
        {
            foreach (var aServerAccount in _socketCollection) scrollingSocketAction?.Invoke(aServerAccount.Value);
        }

        #endregion

        /// <summary>
        ///     Get socket connection by session id
        /// </summary>
        /// <param name="sessionId">Session id for socket</param>
        /// <returns>Socket of session id</returns>

        #region GetSocketBySessionId

        public Socket GetSocketBySessionId(long sessionId)
        {
            if (_socketCollection.ContainsKey(sessionId))
                return _socketCollection[sessionId];
            return null;
        }

        #endregion

        /// <summary>
        ///     Get account by session id
        /// </summary>
        /// <param name="sessionId">Specify session id</param>
        /// <returns>TAccount type</returns>

        #region GetAccountBySessionId

        public TAccount GetAccountBySessionId(long sessionId)
        {
            if (sessionId == -1 || !_accountCollection.ContainsKey(sessionId))
                return null;
            return _accountCollection[sessionId];
        }

        #endregion

        #endregion
    }
}