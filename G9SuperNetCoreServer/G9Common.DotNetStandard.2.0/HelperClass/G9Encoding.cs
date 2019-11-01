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
            switch (typeOfEncoding)
            {
                case EncodingTypes.ASCII:
                    EncodingType = Encoding.ASCII;
                    break;
                case EncodingTypes.BigEndianUnicode:
                    EncodingType = Encoding.BigEndianUnicode;
                    break;
                case EncodingTypes.Default:
                    EncodingType = Encoding.Default;
                    break;
                case EncodingTypes.UTF_32:
                    EncodingType = Encoding.UTF32;
                    break;
                case EncodingTypes.UTF_7:
                    EncodingType = Encoding.UTF7;
                    break;
                case EncodingTypes.UTF_8:
                    EncodingType = Encoding.UTF8;
                    break;
                case EncodingTypes.Unicode:
                    EncodingType = Encoding.Unicode;
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(EncodingType), (byte) typeOfEncoding,
                        typeof(EncodingTypes));
                    break;
            }
        }

        /// <summary>
        ///     Access to encoding
        /// </summary>
        public readonly Encoding EncodingType;
    }
}