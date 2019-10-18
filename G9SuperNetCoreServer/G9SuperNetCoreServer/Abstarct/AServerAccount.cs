using G9Common.Abstract;
using G9SuperNetCoreServer.Enums;
using G9SuperNetCoreServer.HelperClass;

namespace G9SuperNetCoreServer.Abstarct
{
    public abstract class AServerAccount<TSession> : AAccount
        where TSession : AServerSession, new()
    {
        #region Fields And Properties

        /// <inheritdoc />
        public override ASession SessionSendCommand => Session;

        /// <summary>
        ///     Access to session
        /// </summary>
        public TSession Session { private set; get; }

        /// <summary>
        ///     Access to handler
        /// </summary>
        private G9ServerAccountHandler _handler;

        #endregion

        #region Methods

        /// <summary>
        ///     Initialize and handle account and session
        ///     Automatic use with core
        /// </summary>

        #region InitializeAndHandlerAccountAndSessionAutomaticFirstTime

        public void InitializeAndHandlerAccountAndSessionAutomaticFirstTime(G9ServerAccountHandler handler,
            TSession oSession)
        {
            // Set handler
            _handler = handler;

            // Set session
            Session = oSession;
        }

        #endregion

        /// <summary>
        ///     Call when session close
        /// </summary>
        /// <param name="reason">Get reason of close</param>

        #region OnSessionClosed

        public abstract void OnSessionClosed(DisconnectReason reason);

        #endregion

        #endregion
    }
}