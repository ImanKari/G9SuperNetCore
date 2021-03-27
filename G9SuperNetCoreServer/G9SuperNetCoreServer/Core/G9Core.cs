using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using G9SuperNetCoreCommon.CommandHandler;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreCommon.HelperClass;
using G9SuperNetCoreCommon.Interface;
using G9SuperNetCoreCommon.LogIdentity;
using G9SuperNetCoreCommon.Packet;
using G9SuperNetCoreCommon.Resource;
using G9LogManagement;
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
        private readonly SortedDictionary<uint,
                G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>>
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
        ///     <para>### Execute From Session ###</para>
        ///     <para>Access to action send command by name</para>
        ///     <para>uint => session id</para>
        ///     <para>string => command name</para>
        ///     <para>object => data for send</para>
        ///     <para>Guid? => set custom request id</para>
        ///     <para>bool => If set true, check command exists</para>
        ///     <para>bool => If set true, check command send type</para>
        /// </summary>
        private readonly Action<uint, string, object, Guid?, bool, bool> _sendCommandByName;

        /// <summary>
        ///     <para>### Execute From Session ###</para>
        ///     <para>Access to action send command by name async</para>
        ///     <para>uint => session id</para>
        ///     <para>string => command name</para>
        ///     <para>object => data for send</para>
        ///     <para>Guid? => set custom request id</para>
        ///     <para>bool => If set true, check command exists</para>
        ///     <para>bool => If set true, check command send type</para>
        /// </summary>
        private readonly Action<uint, string, object, Guid?, bool, bool> _sendCommandByNameAsync;

        /// <summary>
        ///     Access to event OnSessionReceiveRequestOverTheLimitInSecond
        /// </summary>
        private readonly Action<TAccount> _onSessionReceiveRequestOverTheLimitInSecond;

        /// <summary>
        ///     <para>Management disconnect client</para>
        ///     <para>TAccount: Connected account</para>
        ///     <para>DisconnectReason: Reason of disconnect</para>
        /// </summary>
        private readonly Action<TAccount, DisconnectReason> _onDisconnectedHandler;

        /// <summary>
        ///     Used for encrypt and decrypt with certificates
        /// </summary>
        public readonly G9EncryptAndDecryptDataWithCertificate EncryptAndDecryptDataWithCertificate;

        /// <summary>
        ///     Specified enable ssl (Secure) connection for server socket
        ///     If set certificates => it's true
        /// </summary>
        public readonly bool EnableSslConnection;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize Requirement
        /// </summary>
        /// <param name="superNetCoreConfig">Server config</param>
        /// <param name="commandAssemblies">Specified command assemblies (find command in specified assembly)</param>
        /// <param name="sendCommandByName">Specify func send command by name</param>
        /// <param name="sendCommandByNameAsync">Specify func send command by name async</param>
        /// <param name="onSessionReceiveRequestOverTheLimitInSecond">
        ///     Specify action for event
        ///     OnSessionReceiveRequestOverTheLimitInSecond
        /// </param>
        /// <param name="onUnhandledCommand">Specified event on unhandled command</param>
        /// <param name="customLogging">Specified custom logging system</param>
        /// <param name="sslCertificate">Specified object of G9SslCertificate for manage ssl connection</param>

        #region G9Core

        public G9Core(G9ServerConfig superNetCoreConfig, Assembly[] commandAssemblies,
            Action<uint, string, object, Guid?, bool, bool> sendCommandByName,
            Action<uint, string, object, Guid?, bool, bool> sendCommandByNameAsync,
            Action<TAccount> onSessionReceiveRequestOverTheLimitInSecond,
            Action<G9SendAndReceivePacket, TAccount> onUnhandledCommand,
            Action<TAccount, DisconnectReason> onDisconnectedHandler,
            IG9Logging customLogging = null, G9SslCertificate sslCertificate = null)
        {
            // Set sorted dictionary collection
            _accountCollection =
                new SortedDictionary<uint, G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>
                >();

            // Set logging system
            Logging = customLogging ?? new G9LoggingServer();

            // Initialize EncryptAndDecryptDataWithCertificate
            if (sslCertificate != null && sslCertificate.Certificates.Length > 0)
            {
                EnableSslConnection = true;
                EncryptAndDecryptDataWithCertificate = new G9EncryptAndDecryptDataWithCertificate(sslCertificate);
            }

            // Set send command
            _sendCommandByName = sendCommandByName;
            _sendCommandByNameAsync = sendCommandByNameAsync;
            _onSessionReceiveRequestOverTheLimitInSecond = onSessionReceiveRequestOverTheLimitInSecond;
            _onDisconnectedHandler = onDisconnectedHandler;

            // Set configuration
            Configuration = superNetCoreConfig;

            // Initialize command handler
            CommandHandler = new G9CommandHandler<TAccount>(commandAssemblies, Logging, Configuration.CommandSize,
                onUnhandledCommand);
        }

        #endregion

        /// <summary>
        ///     Accept connection by connection socket
        ///     Initialize requirement
        ///     Create new account and session and add to collection
        /// </summary>
        /// <param name="connectionSocket"></param>
        /// <returns>Return added account</returns>

        #region TryAcceptConnection

        public (bool, TAccount) TryAcceptConnection(Socket connectionSocket)
        {
            if (_maximumConnectionCounter >= Configuration.MaxConnectionNumber)
            {
                if (Logging.CheckLoggingIsActive(LogsType.WARN))
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

            var newAccountData = CreateAccountByAcceptedSocket(connectionSocket);

            if (newAccountData != null)
            {
                if (Logging.CheckLoggingIsActive(LogsType.INFO))
                    Logging.LogInformation(LogMessage.AccountAndSessionCreated, G9LogIdentity.CREATE_NEW_ACCOUNT,
                        LogMessage.CreateNewAccount);

                _accountCollection[_sessionIdentityCounter++] = newAccountData;

                if (Logging.CheckLoggingIsActive(LogsType.INFO))
                    Logging.LogInformation(LogMessage.SuccessAccountAdded, G9LogIdentity.CREATE_NEW_ACCOUNT,
                        LogMessage.CreateNewAccount);
                return (true, newAccountData.Account);
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
        ///     <para>Add fake account to server</para>
        ///     <para>Create new fake account and fake session and add to collection</para>
        ///     <para>Used for robots</para>
        /// </summary>
        /// <returns>Return added account</returns>

        #region AddFakeAccount

        public G9FakeAccountHandler<TAccount, TSession> AddFakeAccount(
            Action<G9FakeAccountHandler<TAccount, TSession>, string, object, Guid> receiveCallBack,
            Action<TAccount, string, object, Guid?, bool, bool, CommandSendType> sendAction)
        {
            // Initialize account and fake account 
            var account = new TAccount();
            var fakeAccount = new G9FakeAccountHandler<TAccount, TSession>(account, receiveCallBack, sendAction);

            // Check maximum connection
            if (_maximumConnectionCounter >= Configuration.MaxConnectionNumber)
            {
                if (Logging.CheckLoggingIsActive(LogsType.WARN))
                    Logging.LogWarning(
                        "On add fake account, reject.\nReason: limit max connection number",
                        G9LogIdentity.ACCEPT_CONNECTION, "Reject connection");

                return default;
            }

            // Set fake account
            var newAccountData =
                CreateAccountByAcceptedSocket(new Socket(IPAddress.None.AddressFamily, SocketType.Stream,
                    ProtocolType.Tcp), receiveCallBack, fakeAccount);

            // set member
            if (newAccountData != null)
            {
                if (Logging.CheckLoggingIsActive(LogsType.INFO))
                    Logging.LogInformation(LogMessage.FakeAccountAndSessionCreated,
                        G9LogIdentity.CREATE_NEW_FAKE_ACCOUNT,
                        LogMessage.CreateNewAccount);

                _accountCollection[_sessionIdentityCounter++] = newAccountData;

                if (Logging.CheckLoggingIsActive(LogsType.INFO))
                    Logging.LogInformation(LogMessage.SuccessAccountAdded, G9LogIdentity.CREATE_NEW_FAKE_ACCOUNT,
                        LogMessage.CreateNewAccount);

                return fakeAccount;
            }

            return default;
        }

        #endregion


        #region DisconnectAndCloseSession - Overloads

        /// <summary>
        ///     Disconnect and close session
        ///     remove from collection and dispose resource
        /// </summary>
        /// <param name="account">Specified account</param>
        /// <param name="disconnectReason">Specify disconnect reason</param>

        #region DisconnectAndCloseSession

        public void DisconnectAndCloseSession(TAccount account, DisconnectReason disconnectReason)
        {
            try
            {
                // Run on session closed in account
                _accountCollection[account.Session.SessionId]?.Account.OnSessionClosed(disconnectReason);
            }
            catch
            {
                // Ignore
            }

            try
            {
                // Dispose and remove
                _accountCollection[account.Session.SessionId]?.SessionSocket.Dispose();
            }
            catch (Exception ex)
            {
                if (Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    ex.G9LogException_Default("Exception when dispose socket", nameof(DisconnectAndCloseSession),
                        LogMessage.FailedOperation);
            }

            // Remove account
            _accountCollection.Remove(account.Session.SessionId);

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
            _accountCollection.Remove(sessionId);
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
            for (uint i = 0; i < _accountCollection.Count; i++)
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
                _accountCollection.Remove(i);
            }

            // Gc collect
            GC.Collect();
        }

        #endregion

        /// <summary>
        ///     Create new account and session by connection socket
        /// </summary>
        /// <param name="acceptedSocket">connection socket</param>
        /// <param name="customSendCommandByName">
        ///     <para>When need custom send command</para>
        ///     <para>Use for fake account and robots</para>
        /// </param>
        /// <returns>Return G9AccountUtilities account data</returns>

        #region CreateAccountByAcceptedSocket

        private G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>
            CreateAccountByAcceptedSocket(Socket acceptedSocket,
                Action<G9FakeAccountHandler<TAccount, TSession>, string, object, Guid> customSendCommandByName = null,
                G9FakeAccountHandler<TAccount, TSession>? customAccountForSet = null)
        {
            try
            {
                // Initialize result
                var result = new G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>();

                // Specified account is ai robot
                var isAiRobot = customSendCommandByName != null;

                // Set action session receive request over limit
                Action<uint> sessionReceiveRequestOverLimit;
                if (isAiRobot)
                    sessionReceiveRequestOverLimit = sessionId => { };
                else
                    sessionReceiveRequestOverLimit = sessionId => _onSessionReceiveRequestOverTheLimitInSecond(
                        GetAccountUtilitiesBySessionId(sessionId)
                            .Account);

                // Instance session and pass requirement to constructor
                var session = new TSession();
                session.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(result.SessionHandler =
                        // Set session handler
                        new G9ServerSessionHandler
                        {
                            // Set send commands
                            Session_SendCommandByName = isAiRobot
                                ? (u, s, arg3, arg4, arg5, arg6) =>
                                {
                                    // ReSharper disable once PossibleInvalidOperationException
                                    customSendCommandByName(customAccountForSet.Value, s, arg3, arg4 ?? Guid.Empty);
                                }
                                : _sendCommandByName,
                            Session_SendCommandByNameAsync = isAiRobot
                                ? (u, s, arg3, arg4, arg5, arg6) =>
                                {
                                    // ReSharper disable once PossibleInvalidOperationException
                                    customSendCommandByName(customAccountForSet.Value, s, arg3, arg4 ?? Guid.Empty);
                                }
                                : _sendCommandByNameAsync,
                            // Set ping duration
                            PingDurationInMilliseconds = isAiRobot
                                ? ushort.MaxValue
                                : (ushort) Configuration.GetPingTimeOut.TotalMilliseconds,
                            // Set event
                            Session_OnSessionReceiveRequestOverTheLimitInSecond = sessionReceiveRequestOverLimit,
                            // Set session encoding
                            Session_GetSessionEncoding = () => Configuration.EncodingAndDecoding,
                            // Set account 
                            Core_SetAccount = () => result.Account,
                            // Set is ai robot
                            Session_SpecifiedAccountIsRobot = () => isAiRobot
                        },
                    // Set session id
                    _sessionIdentityCounter,
                    // Set ip address
                    isAiRobot
                        ? IPAddress.None
                        : ((IPEndPoint) acceptedSocket.RemoteEndPoint).Address
                );

                // Ai robots authorization automatic
                if (isAiRobot)
                    result.SessionHandler.Core_AuthorizationClient();

                // ########### Set requirement###########
                // Set max request
                result.SessionHandler.Core_SetMaxRequestRequirement(isAiRobot
                    ? ushort.MaxValue
                    : Configuration.MaxRequestPerSecond);

                // Variable for result
                ushort certNumber = 0;

                if (EnableSslConnection)
                    // Get certificate number
                    certNumber = EncryptAndDecryptDataWithCertificate.GetRandomCertificateNumber();

                // Set certificate number
                result.SessionHandler.Core_SetCertificateNumber(certNumber);

                // Instance account and pass session to constructor
                // Set new account
                var newAccount = result.Account = customAccountForSet.HasValue
                    ? customAccountForSet.Value.Account
                    : new TAccount();
                newAccount.InitializeAndHandlerAccountAndSessionAutomaticFirstTime(result.AccountHandler =
                    new G9ServerAccountHandler(), session);

                // Set socket
                result.SessionSocket = acceptedSocket;

                // return result
                return result;
            }
            catch (Exception ex)
            {
                // set ex log
                if (Logging.CheckLoggingIsActive(LogsType.EXCEPTION))
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
            foreach (var account in _accountCollection.Values.Where(account => IsSocketConnected(account)))
                scrollingSocketAction?.Invoke(account);
        }

        #endregion

        /// <summary>
        ///     Check socket is connected
        /// </summary>
        /// <param name="account">Access to G9AccountUtilities account data</param>
        /// <param name="autoDisposeAndRemove">If true auto dispose socket and remove client if connection is closed</param>
        /// <returns>Return true if connected</returns>

        #region IsSocketConnected

        private bool IsSocketConnected(
            G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler> account,
            bool autoDisposeAndRemove = true)
        {
            try
            {
                // Check socket is connected
                if (account.SessionSocket != null && !(account.SessionSocket.Poll(1000, SelectMode.SelectRead) &&
                                                       account.SessionSocket.Available == 0 ||
                                                       !account.SessionSocket.Connected))
                    return true;

                // If enable auto dispose => 
                if (autoDisposeAndRemove)
                    _onDisconnectedHandler?.Invoke(account.Account, DisconnectReason.DisconnectedFromClient);
                return false;
            }
            catch
            {
                // Ignore
                // If enable auto dispose => 
                if (autoDisposeAndRemove)
                    _onDisconnectedHandler?.Invoke(account.Account, DisconnectReason.DisconnectedFromClient);
                return false;
            }
        }

        #endregion

        /// <summary>
        ///     Select account utilities with func
        /// </summary>
        /// <param name="predicate">Predicate func</param>

        #region SelectAccountUtilities

        public IEnumerable<TResult> SelectAccountUtilities<TResult>(
            Func<IDictionary<uint, G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>>,
                IEnumerable<TResult>> predicate)
        {
            return predicate?.Invoke(_accountCollection);
        }

        #endregion

        /// <summary>
        ///     Get account utilities
        /// </summary>
        /// <param name="sessionId">Session id for socket</param>
        /// <returns>Return account if exist</returns>

        #region GetSocketBySessionId

        public G9AccountUtilities<TAccount, G9ServerAccountHandler, G9ServerSessionHandler>
            GetAccountUtilitiesBySessionId(uint sessionId)
        {
            // Check exist account
            var account = _accountCollection.FirstOrDefault(s => s.Key == sessionId).Value;
            if (account == null) return null;

            // Check connected then return
            return IsSocketConnected(account) ? account : null;
        }

        #endregion

        #endregion
    }
}