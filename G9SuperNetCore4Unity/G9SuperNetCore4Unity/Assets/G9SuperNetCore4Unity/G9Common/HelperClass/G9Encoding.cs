#if NETSTANDARD2_1 || NETCOREAPP3_0
using System;
#endif
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
#region Fields And Properties

        /// <summary>
        ///     Access to encoding
        /// </summary>
        public readonly Encoding EncodingType;

#endregion

#region Methods

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="typeOfEncoding">Specify type of encoding</param>

#region G9Encoding

        public G9Encoding(EncodingTypes typeOfEncoding)
        {
#if NETSTANDARD2_1 || NETCOREAPP3_0
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
#else
            switch (typeOfEncoding)
            {
                case EncodingTypes.ASCII:
                    EncodingType = Encoding.ASCII;
                    break;
                case EncodingTypes.BigEndianUnicode:
                    EncodingType = Encoding.BigEndianUnicode;
                    break;
#if NETSTANDARD2_0
                case EncodingTypes.Default:
                    EncodingType = Encoding.Default;
                    break;
#endif
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
                    throw new InvalidEnumArgumentException(nameof(EncodingType), (byte)typeOfEncoding,
                        typeof(EncodingTypes));
            }
#endif
            }

#endregion

        /// <summary>
        ///     Easy access to get bytes
        /// </summary>

#region GetBytes

        public byte[] GetBytes(string input)
        {
            return EncodingType.GetBytes(input);
        }

#endregion

        /// <summary>
        ///     Easy access to get string
        /// </summary>

#region GetString

        public string GetString(byte[] input)
        {
            return EncodingType.GetString(input);
        }

        #endregion

#if NETSTANDARD2_1 || NETCOREAPP3_0

        /// <summary>
        /// Easy access to get string
        /// </summary>
        #region GetString
        public string GetString(ReadOnlyMemory<byte> input)
        {
            return EncodingType.GetString(input.ToArray());
        }
        #endregion

#endif

        #endregion
    }
}