using System.Net.Sockets;

namespace G9Common.Abstract
{
    // State object for reading client data asynchronously  
    public abstract class AG9SuperNetCoreStateObjectBase
    {
        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirement
        /// </summary>

        #region StateObjectServer

        protected AG9SuperNetCoreStateObjectBase(int oBufferSize, long oSessionIdentity)
        {
            BufferSize = oBufferSize;
            SessionIdentity = oSessionIdentity;
            Buffer = new byte[BufferSize];
        }

        #endregion

        #endregion

        #region Fields And Properties

        /// <summary>
        ///     Size of receive buffer.
        /// </summary>
        public static int BufferSize { get; private set; }

        /// <summary>
        ///     Get Unique identity from session
        /// </summary>
        public readonly long SessionIdentity;

        /// <summary>
        ///     Get Client socket.
        /// </summary>
        public Socket WorkSocket = null;

        /// <summary>
        ///     Receive buffer.
        /// </summary>
        public byte[] Buffer;

        #endregion
    }
}