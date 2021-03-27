using System;
using System.Net;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreCommon.HelperClass;
using G9SuperNetCoreCommon.Resource;

namespace G9SuperNetCoreCommon.Configuration
{
    public abstract class AG9BaseConfig
    {
        #region Methods

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="oIpAddress">Specify ip address</param>
        /// <param name="oPortNumber">Specify port number</param>
        /// <param name="oMode">Specify mode TCP or UDP</param>
        /// <param name="oCommandSize">
        ///     Specify command size
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or
        ///     character
        /// </param>
        /// <param name="oBodySize">
        ///     Specify body size
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or character
        ///     Minimum is 1 maximum is 255
        /// </param>
        /// <param name="oEncodingAndDecoding">
        ///     Specify type of encoding and decoding
        ///     If set null adjusted default value => UTF-8
        /// </param>

        #region G9SuperSocketConfig

        public AG9BaseConfig(IPAddress oIpAddress, ushort oPortNumber, SocketMode oMode,
            byte oCommandSize = 1, byte oBodySize = 8, G9Encoding oEncodingAndDecoding = null)
        {
            // Set ip address
            IpAddress = oIpAddress is null || Equals(oIpAddress, IPAddress.None)
                ? throw new ArgumentException(string.Format(LogMessage.ArgumentXNotCorrect, nameof(oIpAddress)), nameof(oIpAddress))
                : oIpAddress;
            // Set port number
            PortNumber = oPortNumber < 1
                ? throw new ArgumentException(string.Format(LogMessage.ArgumentXNotCorrect, nameof(oPortNumber)), nameof(oPortNumber))
                : oPortNumber;
            // Set socket mode
            Mode = oMode;
            // Set command size
            CommandSize = oCommandSize < 1
                ? throw new ArgumentException(string.Format(LogMessage.ArgumentXNotCorrect, nameof(oCommandSize)), nameof(oCommandSize))
                : oCommandSize;
            // Set body size
            BodySize = oBodySize < 1 && oBodySize > 255
                ? throw new ArgumentException(string.Format(LogMessage.ArgumentXNotCorrect, nameof(oBodySize)), nameof(oBodySize))
                : oBodySize;
            // Set encoding
            EncodingAndDecoding = oEncodingAndDecoding ?? new G9Encoding(EncodingTypes.UTF_8);
        }

        #endregion

        #endregion

        #region Fields And Properties

        /// <summary>
        ///     <para>Specify packet type size => is 1 byte</para>
        ///     <para>Packet has two type => OnePacket = 0, MultiPacket = 1</para>
        /// </summary>
        public const byte PacketTypeSizeSpaceBusy = 1;

        /// <summary>
        /// <para>Specify packet data type size => is 1 byte (enum byte)</para>
        /// </summary>
        public const byte PacketDataTypeSizeSpaceBusy = 1;

        /// <summary>
        ///     <para>Specify space busy for body size</para>
        ///     <para>Just 1 byte => size between 1 to 255</para>
        /// </summary>
        public const byte PacketBodySizeSpaceBusy = 1;

        /// <summary>
        ///     Sum 'PacketTypeSizeSpaceBusy' and 'PacketDataTypeSizeSpaceBusy' and 'PacketBodySizeSpaceBusy'
        /// </summary>
        public const byte PacketTypeSizeAndPacketDataTypeSizeAndBodySizeSpaceBusy = 3;

        /// <summary>
        ///     <para>Specify packet request id => is 16 byte</para>
        ///     <para>Guid.NewGuid().ToByteArray().Length</para>
        /// </summary>
        public const byte PacketRequestIdSize = 16;

        /// <summary>
        ///     Specify server ip address
        /// </summary>
        public readonly IPAddress IpAddress;

        /// <summary>
        ///     Specify server port number
        /// </summary>
        public readonly ushort PortNumber;

        /// <summary>
        ///     <para>Specify socket mode</para>
        ///     <para>TCP or UDP</para>
        /// </summary>
        public readonly SocketMode Mode;

        /// <summary>
        ///     <para>Specify max command size</para>
        ///     <para>Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or</para>
        ///     <para>character</para>
        /// </summary>
        public readonly byte CommandSize;

        /// <summary>
        ///     <para>Specify max body length size</para>
        ///     <para>Example: if set "n" length is "n*16" => if set 8 length is 128 then maximum body length is 128 byte or character</para>
        /// </summary>
        public readonly byte BodySize;

        /// <summary>
        ///     <para>Specify encoding and decoding type</para>
        ///     <para>Default value is UTF-8</para>
        /// </summary>
        public readonly G9Encoding EncodingAndDecoding;

        #endregion
    }
}