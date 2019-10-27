using System;
using System.IO;
using G9Common.Configuration;
using G9Common.Enums;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.Resource;
using G9LogManagement.Enums;

namespace G9Common.PacketManagement
{
    /// <summary>
    ///     Class packet management
    ///     handle packets with data
    /// </summary>
    public class G9PacketManagement
    {
        #region Fields And Properties

        /// <summary>
        ///     Access to logging
        /// </summary>
        private readonly IG9Logging _logging;

        /// <summary>
        ///     Specify encoding and decoding
        /// </summary>
        public readonly G9Encoding EncodingHandler;

        /// <summary>
        ///     Field save header size
        /// </summary>
        private int _packetHeaderSize = -1;

        /// <summary>
        ///     Specify packet header size
        ///     Header size => PacketTypeSize + CalculateCommandSize + PacketSpecifyBodySize
        /// </summary>
        public int PacketHeaderSize
        {
            get
            {
                if (_packetHeaderSize == -1)
                    _packetHeaderSize = AG9BaseConfig.PacketTypeAndBodySpaceBusy + CalculateCommandSize;

                return _packetHeaderSize;
            }
        }


        /// <summary>
        ///     Specify max command size
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or
        ///     character
        /// </summary>
        public readonly int CommandSize;

        /// <summary>
        ///     Specify calculated command size => if set "n" set "n*16"
        /// </summary>
        public readonly int CalculateCommandSize;

        /// <summary>
        ///     Specify max body length size
        ///     Example: if set "n" length is "n*16" => if set 8 length is 128 then maximum body length is 128 byte or character
        ///     Minimum is 1 maximum is 255
        /// </summary>
        public readonly int BodySize;

        /// <summary>
        ///     Specify calculated body size => if set "n" set "n*16"
        /// </summary>
        public readonly int CalculateBodySize;

        /// <summary>
        ///     Save total packet size
        /// </summary>
        private int _getPacketSize = -1;

        /// <summary>
        ///     Specify maximum packet size
        ///     The package is made of the following items:
        ///     PacketTypeSize(Const 1 byte) + (CommandSize * 16) + PacketSpecifyBodySize((BodySize * 16).ToString().Length) +
        ///     (BodySize * 16) + PacketRequestIdSize(Const 16 byte => Guid.NewGuid().ToByteArray().Length)
        /// </summary>
        public int MaximumPacketSize
        {
            get
            {
                if (_getPacketSize == -1)
                    _getPacketSize = AG9BaseConfig.PacketTypeSizeSpaceBusy + CommandSize * 16 +
                                     AG9BaseConfig.PacketBodySizeSpaceBusy + BodySize * 16 +
                                     AG9BaseConfig.PacketRequestIdSize;

                return _getPacketSize;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirement for packet management
        ///     The package is made of the following items:
        ///     PacketTypeSize(Const 1 byte) + (CommandSize * 16) + PacketSpecifyBodySize((BodySize * 16).ToString().Length) +
        ///     (BodySize * 16) + PacketRequestIdSize(Const 16 byte => Guid.NewGuid().ToByteArray().Length)
        /// </summary>
        /// <param name="oPacketCommandSize">Specify packet command size</param>
        /// <param name="oPacketBodySize">Specify packet body size</param>
        /// <param name="oEncoding">Specify encoding and decoding type</param>
        /// <param name="logging">Specified custom logging system</param>

        #region G9PacketManagement

        public G9PacketManagement(int oPacketCommandSize, int oPacketBodySize, G9Encoding oEncoding, IG9Logging logging)
        {
            _logging = logging;
            CommandSize = oPacketCommandSize;
            BodySize = oPacketBodySize;

            CalculateBodySize = BodySize * 16;
            CalculateCommandSize = CommandSize * 16;
            EncodingHandler = oEncoding;
        }

        #endregion

        /// <summary>
        ///     Generate and packing request by data
        ///     Perform calculations to prepare the request
        /// </summary>
        /// <param name="command">Specify package command</param>
        /// <param name="body">Specify package body</param>
        /// <returns>Generated PacketSplitHandler by data</returns>

        #region PackingRequestByData

        public PacketSplitHandler PackingRequestByData(ReadOnlySpan<byte> command, ReadOnlySpan<byte> body)
        {
            try
            {
                if (command.Length == CalculateCommandSize)
                {
                    var requestId = Guid.NewGuid();
                    PacketSplitHandler packet;
                    if (body.Length <= CalculateBodySize)
                    {
                        // Initialize packet split handler
                        packet = new PacketSplitHandler(requestId, 1, MaximumPacketSize);

                        // Initialize memory stream and save packet data
                        var oMemoryStream = new MemoryStream();
                        oMemoryStream.WriteByte((byte) PacketType.OnePacket);
                        oMemoryStream.WriteByte(checked((byte) body.Length));
                        oMemoryStream.Write(command);
                        oMemoryStream.Write(body);
                        oMemoryStream.Write(requestId.ToByteArray());

                        // Add packet
                        packet.AddPacket(oMemoryStream.ToArray());
                    }
                    else
                    {
                        // Calculate packet size
                        var counter = (int) Math.Ceiling(body.Length / (decimal) CalculateBodySize);

                        // Packet size plus one => because first packet specify packet count information
                        // counter.ToString().Length * counter => Reserve first byte for packet number
                        counter = (int) Math.Ceiling(((decimal) body.Length +
                                                      counter.ToString().Length * counter) /
                                                     CalculateBodySize) + 1;

                        // Specify counter length
                        var counterLength = counter.ToString().Length;

                        // Initialize packet split
                        packet = new PacketSplitHandler(requestId, counter, MaximumPacketSize);

                        // Initialize memory stream and save packet data
                        using (var oMemoryStream = new MemoryStream())
                        {
                            oMemoryStream.WriteByte((byte) PacketType.MultiPacket);
                            oMemoryStream.WriteByte(checked((byte) body.Length));
                            oMemoryStream.Write(command);
                            oMemoryStream.Write(EncodingHandler.EncodingType.GetBytes(counter.ToString()));
                            oMemoryStream.Write(requestId.ToByteArray());

                            // Set first packet => packet count information
                            packet.AddPacket(oMemoryStream.ToArray());
                        }

                        for (var i = 1; i < counter; i++)
                        {
                            if (i == counter - 26) Console.WriteLine("X");

                            var newBodyStandardSize = CalculateBodySize - (i == 1
                                                          ? 0
                                                          : counterLength);

                            var offset = newBodyStandardSize * i - newBodyStandardSize;

                            var length = i == counter - 1
                                ? body.Length - offset
                                : CalculateBodySize - counterLength;

                            // Initialize memory stream and save packet data
                            using var oMemoryStream = new MemoryStream();
                            oMemoryStream.WriteByte((byte) PacketType.MultiPacket);
                            oMemoryStream.WriteByte(checked((byte) body.Length));
                            oMemoryStream.Write(command);
                            oMemoryStream.Write(
                                EncodingHandler.EncodingType.GetBytes(i.ToString().PadLeft(counterLength, '0')));
                            oMemoryStream.Write(body.Slice(offset, length));
                            oMemoryStream.Write(requestId.ToByteArray());

                            // Add total packet
                            packet.AddPacket(oMemoryStream.ToArray());
                        }
                    }

                    return packet;
                }

                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(new ArgumentException(
                            $"{LogMessage.CommandLengthError}\n{LogMessage.StandardLength}: {CalculateCommandSize}\n{LogMessage.LengthEntered}: {command.Length}"),
                        null, G9LogIdentity.GENERATE_PACKET, LogMessage.FailedOperation);

                return null;
            }
            catch (Exception ex)
            {
                if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                    _logging.LogException(ex, LogMessage.FailGeneratePacket, G9LogIdentity.GENERATE_PACKET,
                        LogMessage.FailedOperation);
                return null;
            }
        }

        #endregion

        /// <summary>
        ///     Unpacking byte data
        /// </summary>
        /// <param name="packetData">packet data</param>
        /// <returns>Unpacking data and instance G9SendAndReceivePacket from data</returns>

        #region UnpackingRequestByData

        public G9SendAndReceivePacket UnpackingRequestByData(ReadOnlySpan<byte> packetData)
        {
            // Set body size
            var packetBodySize = packetData[1];

            // Initialize data
            return new G9SendAndReceivePacket(
                // Set packet type
                (PacketType) packetData[0],
                // Set command
                EncodingHandler.EncodingType.GetString(packetData.Slice(AG9BaseConfig.PacketTypeAndBodySpaceBusy,
                    CalculateCommandSize)),
                // Set body
                packetData.Slice(PacketHeaderSize, packetBodySize),
                // Set request id
                new Guid(packetData.Slice(PacketHeaderSize + packetBodySize, AG9BaseConfig.PacketRequestIdSize)));
        }

        #endregion

        #endregion
    }
}