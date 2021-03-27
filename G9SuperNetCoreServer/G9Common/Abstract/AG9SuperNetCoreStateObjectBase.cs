using System;
using System.Collections.Generic;
using System.Net.Sockets;
using G9SuperNetCoreCommon.Packet;

namespace G9SuperNetCoreCommon.Abstract
{
    /// <summary>
    ///     State object for reading client data asynchronously
    /// </summary>
    public abstract class AG9SuperNetCoreStateObjectBase
    {
        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirement
        /// </summary>

        #region StateObjectServer

        protected AG9SuperNetCoreStateObjectBase(ushort oBufferSize, uint oSessionIdentity)
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
        public static ushort BufferSize { get; protected set; }

        /// <summary>
        ///     Get Unique identity from session
        /// </summary>
        public readonly uint SessionIdentity;

        /// <summary>
        ///     Get Client socket.
        /// </summary>
        public Socket WorkSocket = null;

        /// <summary>
        ///     Receive buffer.
        /// </summary>
        public byte[] Buffer;

        /// <summary>
        ///     Collection for save and access multi packages
        /// </summary>
        public readonly SortedDictionary<Guid, G9PacketSplitHandler> MultiPacketCollection =
            new SortedDictionary<Guid, G9PacketSplitHandler>();

        #endregion
    }
}