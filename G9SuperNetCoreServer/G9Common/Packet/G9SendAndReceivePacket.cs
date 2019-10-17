using System;
using G9Common.Enums;
using G9Common.Interface;

namespace G9Common.Packet
{
    public class G9SendAndReceivePacket : IG9SendAndReceivePackage
    {
        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize Requirement
        /// </summary>
        /// <param name="typeOfPacket">Specify type of packet</param>
        /// <param name="command">Specify command</param>
        /// <param name="oBody">Specify packet body</param>
        /// <param name="requestId">Specify unique request id</param>

        #region G9SendAndReceivePackage

        public G9SendAndReceivePacket(PacketType typeOfPacket, string command, ReadOnlySpan<byte> oBody, Guid requestId)
        {
            TypeOfPacketType = typeOfPacket;
            Command = command;
            Body = oBody.ToArray();
            RequestId = requestId;
        }

        #endregion

        #endregion

        #region Fields And Properties

        ///<inheritdoc />
        public PacketType TypeOfPacketType { get; }

        ///<inheritdoc />
        public string Command { get; }

        ///<inheritdoc />
        public ReadOnlyMemory<byte> Body { get; }

        ///<inheritdoc />
        public Guid RequestId { get; }

        #endregion
    }
}