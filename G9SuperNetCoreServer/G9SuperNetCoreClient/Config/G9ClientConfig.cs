using System.Net;
using G9Common.Configuration;
using G9Common.Enums;
using G9Common.HelperClass;

namespace G9SuperNetCoreClient.Config
{
    public class G9ClientConfig : AG9BaseConfig
    {

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
        public G9ClientConfig(IPAddress oIpAddress, int oPortNumber, SocketMode oMode, int oCommandSize = 1, int oBodySize = 8, G9Encoding oEncodingAndDecoding = null)
            : base(oIpAddress, oPortNumber, oMode, oCommandSize, oBodySize, oEncodingAndDecoding)
        {

        }
    }
}