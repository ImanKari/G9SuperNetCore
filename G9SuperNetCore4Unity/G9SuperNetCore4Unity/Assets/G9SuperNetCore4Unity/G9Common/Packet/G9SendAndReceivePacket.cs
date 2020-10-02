using System;
using G9Common.Enums;
using G9Common.Interface;
using G9Common.Resource;

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
        /// <param name="packetDataType">Specified packet data type</param>
        /// <param name="command">Specify command</param>
        /// <param name="oBody">Specify packet body</param>
        /// <param name="requestId">Specify unique request id</param>

        #region G9SendAndReceivePackage

#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
      public G9SendAndReceivePacket(G9PacketType typeOfPacket, G9PacketDataType packetDataType, string command,
            ReadOnlySpan<byte> oBody, Guid requestId)
#else
        public G9SendAndReceivePacket(G9PacketType typeOfPacket, G9PacketDataType packetDataType, string command,
            byte[] oBody, Guid requestId)
#endif
        {
            PacketType = typeOfPacket;
            PacketDataType = packetDataType;
            Command = command;
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
            Body = oBody.ToArray();
#else
            Body = oBody;
#endif
            RequestId = requestId;
        }

        #endregion

        /// <summary>
        ///     Change request body by multi packets
        /// </summary>
        /// <param name="multiPackets">Specified multi packets</param>

        #region ChangePackageBodyByMultiPackage

        public void ChangePackageBodyByMultiPackage(G9PacketSplitHandler multiPackets)
        {
            // check fill all packets
            if (!multiPackets.FillAllPacket)
                throw new ArgumentException(LogMessage.ChangeBodyMultiPacketNotFill, nameof(multiPackets));

            // Set new body
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
            Body = new ReadOnlyMemory<byte>(multiPackets.FlushPackets());
#else
            Body = multiPackets.FlushPackets();
#endif
        }

        #endregion

        #endregion

        #region Fields And Properties

        ///<inheritdoc />
        public G9PacketType PacketType { get; }

        ///<inheritdoc />
        public G9PacketDataType PacketDataType { get; }

        ///<inheritdoc />
        public string Command { get; }

        ///<inheritdoc />
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
            public ReadOnlyMemory<byte> Body { private set; get; }
#else
        public byte[] Body { private set; get; }
#endif


        ///<inheritdoc />
        public Guid RequestId { get; }

        #endregion
    }
}