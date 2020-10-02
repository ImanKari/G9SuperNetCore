using System.Net;
using G9Common.Configuration;
using G9Common.Enums;
using G9Common.HelperClass;

namespace G9SuperNetCoreClient.Config
{
    public class G9ClientConfig : AG9BaseConfig
    {
        /// <summary>
        ///     Specified if client disconnected need try for reconnect
        /// </summary>
        public bool AutoReconnect = true;

        /// <summary>
        ///     Specify the duration between try to reconnect
        /// </summary>
        public ushort ReconnectDuration = 9639;

        /// <summary>
        ///     Specify count for try reconnect
        /// </summary>
        public sbyte ReconnectTryCount = 3;

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
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or
        ///     character
        /// </param>
        /// <param name="oEncodingAndDecoding">
        ///     Specify type of encoding and decoding
        ///     If set null adjusted default value => UTF-8
        /// </param>
        public G9ClientConfig(IPAddress oIpAddress, ushort oPortNumber, SocketMode oMode, byte oCommandSize = 1,
            byte oBodySize = 8, G9Encoding oEncodingAndDecoding = null)
            : base(oIpAddress, oPortNumber, oMode, oCommandSize, oBodySize, oEncodingAndDecoding)
        {
        }
    }
}