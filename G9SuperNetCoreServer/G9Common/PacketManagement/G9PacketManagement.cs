using System;
using System.IO;
using System.Linq;
using G9SuperNetCoreCommon.Configuration;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreCommon.HelperClass;
using G9SuperNetCoreCommon.Interface;
using G9SuperNetCoreCommon.LogIdentity;
using G9SuperNetCoreCommon.Packet;
using G9SuperNetCoreCommon.Resource;
using G9LogManagement.Enums;

namespace G9SuperNetCoreCommon.PacketManagement
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
        ///     <para>Specify packet header size</para>
        ///     <para>Header size => PacketTypeSize + CalculateCommandSize + PacketSpecifyBodySize</para>
        /// </summary>
        public int PacketHeaderSize
        {
            get
            {
                if (_packetHeaderSize == -1)
                    _packetHeaderSize = AG9BaseConfig.PacketTypeSizeAndPacketDataTypeSizeAndBodySizeSpaceBusy +
                                        CalculateCommandSize;

                return _packetHeaderSize;
            }
        }


        /// <summary>
        ///     <para>Specify max command size</para>
        ///     <para>Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or</para>
        ///     <para>character</para>
        /// </summary>
        public readonly byte CommandSize;

        /// <summary>
        ///     Specify calculated command size => if set "n" set "n*16"
        /// </summary>
        public readonly int CalculateCommandSize;

        /// <summary>
        ///     <para>Specify max body length size</para>
        ///     <para>
        ///         Example: if set "n" length is "n*16" => if set 8 length is 128 then maximum body length is 128 byte or
        ///         character
        ///     </para>
        ///     <para>Minimum is 1 maximum is 255</para>
        /// </summary>
        public readonly byte BodySize;

        /// <summary>
        ///     Specify calculated body size => if set "n" set "n*16"
        /// </summary>
        public readonly ushort CalculateBodySize;

        /// <summary>
        ///     Save total packet size
        /// </summary>
        private int _getPacketSize = -1;

        /// <summary>
        ///     <para>Specify maximum packet size</para>
        ///     <para>The package is made of the following items:</para>
        ///     <para>
        ///         PacketTypeSize(Const 1 byte) +  + PacketDataTypeSizeSpaceBusy(Const 1 byte) + (CommandSize * 16) +
        ///         PacketSpecifyBodySize((BodySize * 16).ToString().Length) +
        ///         (BodySize * 16) + PacketRequestIdSize(Const 16 byte => Guid.NewGuid().ToByteArray().Length)
        ///     </para>
        /// </summary>
        public ushort MaximumPacketSize
        {
            get
            {
                if (_getPacketSize == -1)
                    _getPacketSize = AG9BaseConfig.PacketTypeSizeSpaceBusy + AG9BaseConfig.PacketDataTypeSizeSpaceBusy +
                                     CommandSize * 16 +
                                     AG9BaseConfig.PacketBodySizeSpaceBusy + BodySize * 16 +
                                     AG9BaseConfig.PacketRequestIdSize;

                return (ushort) _getPacketSize;
            }
        }

        /// <summary>
        ///     <para>Specified packet size when ssl mode is enable</para>
        ///     <para>fixed size => pow (2, n)</para>
        /// </summary>
        /// <returns>return max packet size when ssl mode enable</returns>
        public ushort MaximumPacketSizeInSslMode()
        {
            checked
            {
                var counter = 2;
                ushort packetSize;
                do
                {
                    packetSize = (ushort)Math.Pow(2, counter++);
                } while (packetSize < MaximumPacketSize);

                return packetSize;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     <para>Constructor</para>
        ///     <para>Initialize requirement for packet management</para>
        ///     <para>The package is made of the following items:</para>
        ///     <para>
        ///         PacketTypeSize(Const 1 byte) + (CommandSize * 16) + PacketSpecifyBodySize((BodySize * 16).ToString().Length) +
        ///         (BodySize * 16) + PacketRequestIdSize(Const 16 byte => Guid.NewGuid().ToByteArray().Length)
        ///     </para>
        /// </summary>
        /// <param name="oPacketCommandSize">Specify packet command size</param>
        /// <param name="oPacketBodySize">Specify packet body size</param>
        /// <param name="oEncoding">Specify encoding and decoding type</param>
        /// <param name="logging">Specified custom logging system</param>

        #region G9PacketManagement

        public G9PacketManagement(byte oPacketCommandSize, byte oPacketBodySize, G9Encoding oEncoding,
            IG9Logging logging)
        {
            _logging = logging;
            CommandSize = oPacketCommandSize;
            BodySize = oPacketBodySize;

            CalculateBodySize = (ushort) (BodySize * 16);
            CalculateCommandSize = CommandSize * 16;
            EncodingHandler = oEncoding;
        }

        #endregion

        /// <summary>
        ///     <para>Generate and packing request by data</para>
        ///     <para>Perform calculations to prepare the request</para>
        /// </summary>
        /// <param name="command">Specify package command</param>
        /// <param name="body">Specify package body</param>
        /// <param name="dataType">Specified packet data type</param>
        /// <param name="customRequestId">Specified custom request id for send</param>
        /// <returns>Generated PacketSplitHandler by data</returns>

        #region PackingRequestByData

#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
        public G9PacketSplitHandler PackingRequestByData(ReadOnlySpan<byte> command, ReadOnlySpan<byte> body,
            G9PacketDataType dataType, Guid? customRequestId)
#else
        public G9PacketSplitHandler PackingRequestByData(byte[] command, byte[] body,
            G9PacketDataType dataType, Guid? customRequestId)
#endif
        {
            try
            {
                if (command.Length == CalculateCommandSize)
                {
                    var requestId = customRequestId ?? Guid.NewGuid();
                    var requestIdBytes = requestId.ToByteArray();
                    G9PacketSplitHandler packet;
                    if (body.Length <= CalculateBodySize)
                    {
                        // Initialize packet split handler
                        packet = new G9PacketSplitHandler(requestId, 1);

                        // Initialize memory stream and save packet data
                        using (var memoryStream = new MemoryStream())
                        {
                            memoryStream.WriteByte((byte) G9PacketType.OnePacket);
                            memoryStream.WriteByte((byte) dataType);
                            memoryStream.WriteByte(checked((byte) body.Length));
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                        memoryStream.Write(command);
                        memoryStream.Write(body);
                        memoryStream.Write(requestIdBytes);
#else
                            memoryStream.Write(command, 0, command.Length);
                            memoryStream.Write(body, 0, body.Length);
                            memoryStream.Write(requestIdBytes, 0, requestIdBytes.Length);
#endif

                            // Add packet
                            packet.AddPacket(0, memoryStream.ToArray());
                        }
                    }
                    else
                    {
                        // Calculate packet size
                        var counter = (byte) Math.Ceiling(body.Length / (decimal) CalculateBodySize);

                        // Packet size plus one => because first packet specify packet count information
                        // counter.ToString().Length * counter => Reserve first byte for packet number
                        counter = (byte) (Math.Ceiling(((decimal) body.Length +
                                                        counter.ToString().Length * counter) /
                                                       CalculateBodySize) + 1);

                        // Specify counter length
                        // counter length = 1 => Like byte 0 To 256 
                        var counterLength = 1;

                        // Initialize packet split
                        packet = new G9PacketSplitHandler(requestId, counter);

                        // Initialize memory stream and save packet data
                        using (var memoryStream = new MemoryStream())
                        {
                            memoryStream.WriteByte((byte) G9PacketType.MultiPacket);
                            memoryStream.WriteByte((byte) dataType);
                            // First packet length is 2 | 0 => packet number | 1 => Total packet count
                            memoryStream.WriteByte(2);
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                            memoryStream.Write(command);
#else
                            memoryStream.Write(command, 0, command.Length);
#endif
                            // write packet number
                            memoryStream.WriteByte(0);
                            // write total packet count
                            memoryStream.WriteByte(counter);
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                            memoryStream.Write(requestIdBytes);
#else
                            memoryStream.Write(requestIdBytes, 0, requestIdBytes.Length);
#endif

                            // Set first packet => packet count information
                            packet.AddPacket(0, memoryStream.ToArray());
                        }

                        for (byte i = 1; i < counter; i++)
                        {
                            var newBodyStandardSize = CalculateBodySize - (i == 1
                                                          ? 0
                                                          : counterLength);

                            var offset = newBodyStandardSize * i - newBodyStandardSize;

                            var length = i == counter - 1
                                ? body.Length - offset
                                : CalculateBodySize - counterLength;

                            // Initialize memory stream and save packet data
                            using (var memoryStream = new MemoryStream())
                            {
                                memoryStream.WriteByte((byte) G9PacketType.MultiPacket);
                                memoryStream.WriteByte((byte) dataType);
                                memoryStream.WriteByte(checked((byte) (length + 1)));
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                            memoryStream.Write(command);
#else
                                memoryStream.Write(command, 0, command.Length);
#endif
                                memoryStream.WriteByte(i);
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
                            memoryStream.Write(body.Slice(offset, length));
                                memoryStream.Write(requestId.ToByteArray());
#else
                                memoryStream.Write(body, offset, length);
                                memoryStream.Write(requestIdBytes, 0, requestIdBytes.Length);
#endif


                                // Add total packet
                                packet.AddPacket(i, memoryStream.ToArray());
                            }
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

#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
      public G9SendAndReceivePacket UnpackingRequestByData(ReadOnlySpan<byte> packetData)
#else
        public G9SendAndReceivePacket UnpackingRequestByData(byte[] packetData)
#endif
        {
            // Set body size
            var packetBodySize = packetData[2];

#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
      // Initialize data
            return new G9SendAndReceivePacket(
                // Set packet type
                (G9PacketType) packetData[0],
                // Set packet data type
                (G9PacketDataType) packetData[1],
                // Set command
                EncodingHandler.EncodingType.GetString(packetData.Slice(
                    AG9BaseConfig.PacketTypeSizeAndPacketDataTypeSizeAndBodySizeSpaceBusy,
                    CalculateCommandSize)),
                // Set body
                packetData.Slice(PacketHeaderSize, packetBodySize),
                // Set request id
                new Guid(packetData.Slice(PacketHeaderSize + packetBodySize, AG9BaseConfig.PacketRequestIdSize)));
#else
            // Initialize data
            return new G9SendAndReceivePacket(
                // Set packet type
                (G9PacketType) packetData[0],
                // Set packet data type
                (G9PacketDataType) packetData[1],
                // Set command
                EncodingHandler.EncodingType.GetString(packetData.Skip(
                        AG9BaseConfig.PacketTypeSizeAndPacketDataTypeSizeAndBodySizeSpaceBusy)
                    .Take(CalculateCommandSize)
                    .ToArray()),
                // Set body
                packetData.Skip(PacketHeaderSize).Take(packetBodySize).ToArray(),
                // Set request id
                new Guid(packetData.Skip(PacketHeaderSize + packetBodySize).Take(AG9BaseConfig.PacketRequestIdSize)
                    .ToArray()));
#endif
        }

        #endregion

        #endregion
    }
}