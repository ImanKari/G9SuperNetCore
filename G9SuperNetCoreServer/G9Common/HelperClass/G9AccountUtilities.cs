using System.Net.Sockets;

namespace G9Common.HelperClass
{
    public class G9AccountUtilities<TAccount, TAccountHandler, TSessionHandler>
    {
        /// <summary>
        ///     Access to account
        /// </summary>
        public TAccount Account;

        /// <summary>
        ///     Access to account handler
        /// </summary>
        public TAccountHandler AccountHandler;

        /// <summary>
        ///     Access to Session handler
        /// </summary>
        public TSessionHandler SessionHandler;

        /// <summary>
        ///     Access to session socket
        /// </summary>
        public Socket SessionSocket;
    }
}