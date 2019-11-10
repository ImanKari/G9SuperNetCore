using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using G9Common.Resource;

namespace G9Common.Packet
{
    /// <summary>
    ///     Class Used for packet split handler
    ///     Management single and multi packets
    /// </summary>
    public class G9PacketSplitHandler
    {
        #region Fields And Properties

        /// <summary>
        ///     Request id for packets
        /// </summary>
        public Guid RequestId { get; }

        /// <summary>
        ///     Date time create split packet
        ///     user for delete packet at timeout
        /// </summary>
        public DateTime PacketCreateDateTime { get; }

        /// <summary>
        ///     Specify total packet
        /// </summary>
        public int TotalPackets { get; }

        /// <summary>
        ///     Memory stream for save packets
        /// </summary>
        private readonly byte[][] _packets;

        /// <summary>
        ///     Specified fill all packet
        /// </summary>
        public bool FillAllPacket { private set; get; }

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirements
        /// </summary>
        /// <param name="requestId">Request id</param>
        /// <param name="totalPackets">Specify total packets</param>

        #region PacketSplitHandler

        public G9PacketSplitHandler(Guid requestId, byte totalPackets)
        {
            PacketCreateDateTime = DateTime.Now;
            RequestId = requestId;
            TotalPackets = totalPackets;
            _packets = new byte[TotalPackets][];
        }

        #endregion

        /// <summary>
        ///     Add new packet
        /// </summary>
        /// <param name="packetNumber">Specified packet number</param>
        /// <param name="packetData">Packet data</param>

        #region AddPacket

        public void AddPacket(byte packetNumber, byte[] packetData)
        {
            // Set exception if packet is greater
            if (packetNumber > TotalPackets)
                throw new ArgumentException(LogMessage.PacketNumberIsGreater, nameof(packetNumber));
            // Add packet
            _packets[packetNumber] = packetData;
            // Set flag
            if (_packets.All(s => s != null))
                FillAllPacket = true;
        }

        #endregion

        /// <summary>
        ///     Get total packets like Jagged Arrays
        /// </summary>
        /// <returns>Jagged Arrays of packets</returns>

        #region GetPacketsArray

        public List<byte[]> GetPacketsArray()
        {
            var result = new List<byte[]>();
            for (var i = 0; i < TotalPackets; i++) result.Add(_packets[i]);
            return result;
        }

        #endregion

        /// <summary>
        ///     Add all packet data to one packet
        /// </summary>
        /// <returns>flush bytes of all packets</returns>

        #region FlushPackets

        public byte[] FlushPackets()
        {
            using (var packets = new MemoryStream())
            {
                if (TotalPackets > 1)
                    // Start from 1 because first packet is information
                    for (var i = 1; i < TotalPackets; i++)
                        // Write with offset 1 because first byte specified packet number
                        packets.Write(_packets[i], 1, _packets[i].Length - 1);
                else
#if NETSTANDARD2_1 || NETCOREAPP3_0
                    packets.Write(_packets[0]);
#else
                    packets.Write(_packets[0], 0, _packets[0].Length);
#endif

                return packets.ToArray();
            }
        }

#endregion

#endregion
    }
}