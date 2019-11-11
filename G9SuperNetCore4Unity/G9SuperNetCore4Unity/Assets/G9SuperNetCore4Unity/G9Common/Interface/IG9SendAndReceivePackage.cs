using System;
using G9Common.Enums;

namespace G9Common.Interface
{
    /// <summary>
    ///     Package for send and receive between server and client
    /// </summary>
    public interface IG9SendAndReceivePackage
    {
        /// <summary>
        ///     Type of packet (one packet, multi packet)
        /// </summary>
        G9PacketType PacketType { get; }

        /// <summary>
        ///     Specified packet data
        /// </summary>
        G9PacketDataType PacketDataType { get; }

        /// <summary>
        ///     Command name
        /// </summary>
        string Command { get; }

        /// <summary>
        ///     Body of command
        /// </summary>
#if NETSTANDARD2_1 || NETCOREAPP3_0
        ReadOnlyMemory<byte> Body { get; }
#else
        byte[] Body { get; }
#endif

        /// <summary>
        ///     Unique request id
        /// </summary>
        Guid RequestId { get; }
    }
}