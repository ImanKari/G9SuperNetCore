﻿using System;
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
        PacketType TypeOfPacketType { get; }

        /// <summary>
        ///     Command name
        /// </summary>
        string Command { get; }

        /// <summary>
        ///     Body of command
        /// </summary>
        ReadOnlyMemory<byte> Body { get; }

        /// <summary>
        ///     Unique request id
        /// </summary>
        Guid RequestId { get; }
    }
}