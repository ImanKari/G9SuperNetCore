using G9Common.Abstract;
using G9SuperNetCoreClient.Enums;
using G9SuperNetCoreClient.Helper;

namespace G9SuperNetCoreClient.Abstract
{
    public abstract class AClientAccount<TSession> : AAccount
        where TSession : AClientSession, new()
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
        private G9ClientAccountHandler _handler;

        #endregion

        #region Methods

        /// <summary>
        ///     Initialize and handle account and session
        ///     Automatic use with core
        /// </summary>

        #region InitializeAndHandlerAccountAndSessionAutomaticFirstTime

        public void InitializeAndHandlerAccountAndSessionAutomaticFirstTime(G9ClientAccountHandler handler,
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