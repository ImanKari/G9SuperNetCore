using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace G9Common.Packet
{
    public class PacketSplitHandler
    {
        /// <summary>
        /// Request id for packets
        /// </summary>
        public Guid RequestId { get; }
        /// <summary>
        /// Date time create split packet
        /// user for delete packet at timeout
        /// </summary>
        public DateTime PacketCreateDateTime { get; }
        /// <summary>
        /// Specify total packet
        /// </summary>
        public int TotalPackets { get; }
        /// <summary>
        /// Memory stream for save packets
        /// </summary>
        public MemoryStream Packets { get; }

        /// <summary>
        /// save packet total length
        /// </summary>
        private int _packetTotalLength;

        /// <summary>
        /// Constructor
        /// Initialize requirements
        /// </summary>
        /// <param name="requestId">Request id</param>
        /// <param name="totalPackets">Specify total packets</param>
        public PacketSplitHandler(Guid requestId, int totalPackets, int packetTotalLength)
        {
            _packetTotalLength = packetTotalLength;
            PacketCreateDateTime = DateTime.Now;
            RequestId = requestId;
            TotalPackets = totalPackets;
            Packets = new MemoryStream();
        }

        /// <summary>
        /// Add new packet
        /// </summary>
        /// <param name="packetNumber">Packet number</param>
        /// <param name="packetData">Packet data</param>
        public void AddPacket(int packetNumber, byte[] packetData)
        {
            Packets.Write(packetData, 0, packetData.Length);
        }

        /// <summary>
        /// Get total packets like Jagged Arrays
        /// </summary>
        /// <returns>Jagged Arrays of packets</returns>
        public List<byte[]> GetPacketsArray()
        {
            Packets.Seek(0, SeekOrigin.Begin);
            List<byte[]> result = new List<byte[]>();
            if (TotalPackets > 1)
            {
                for (var i = 0; i < TotalPackets - 1; i++)
                {
                    result.Add(new byte[0]);
                    Packets.Read(result[i], i * _packetTotalLength, _packetTotalLength);
                }

                var lastIndex = TotalPackets - 1;
                var lastOffset = lastIndex * _packetTotalLength;
                result.Add(new byte[0]);
                Packets.Read(result[lastIndex], lastOffset, (int) (Packets.Length - lastOffset));
                return result;
            }
            result.Add(new byte[Packets.Length]);
            Packets.Read(result[0], 0, (int) Packets.Length);
            return result;
        }

        /// <summary>
        /// Add all packet data to one packet 
        /// </summary>
        /// <returns>flush bytes of all packets</returns>
        public byte[] FlushPackets()
        {
            return Packets.ToArray();
        }
    }
}
