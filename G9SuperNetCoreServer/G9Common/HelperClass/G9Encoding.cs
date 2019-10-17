using System.ComponentModel;
using System.Text;
using G9Common.Enums;

namespace G9Common.HelperClass
{
    /// <summary>
    ///     Encoding management
    /// </summary>
    public class G9Encoding
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="typeOfEncoding">Specify type of encoding</param>
        public G9Encoding(EncodingTypes typeOfEncoding)
        {
            EncodingType = typeOfEncoding switch
            {
                EncodingTypes.ASCII => Encoding.ASCII,
                EncodingTypes.BigEndianUnicode => Encoding.BigEndianUnicode,
                EncodingTypes.Default => Encoding.Default,
                EncodingTypes.UTF_32 => Encoding.UTF32,
                EncodingTypes.UTF_7 => Encoding.UTF7,
                EncodingTypes.UTF_8 => Encoding.UTF8,
                EncodingTypes.Unicode => Encoding.Unicode,
                _ => throw new InvalidEnumArgumentException(nameof(EncodingType), (byte) typeOfEncoding,
                    typeof(EncodingTypes))
            };
        }

        /// <summary>
        ///     Access to encoding
        /// </summary>
        public readonly Encoding EncodingType;
    }
}