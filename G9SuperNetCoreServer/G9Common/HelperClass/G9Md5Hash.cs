using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace G9SuperNetCoreCommon.HelperClass
{
    public static class G9Md5Hash
    {
        /// <summary>
        ///     Create md5 hash by string
        /// </summary>
        /// <param name="input">String input for convert to md5 hash</param>
        /// <returns>Converted md5 hash</returns>

        #region GenerateMd5

        public static string GenerateMd5(this string input, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.ASCII;
            return GenerateMd5(encoding.GetBytes(input));
        }

        #endregion

        /// <summary>
        ///     Create md5 hash by string
        /// </summary>
        /// <param name="inputBytes">byte array for convert to md5 hash</param>
        /// <returns>Converted md5 hash</returns>

        #region GenerateMd5

        public static string GenerateMd5(this byte[] inputBytes)
        {
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
            using var md5 = MD5.Create();

            Span<byte> hashBytes = stackalloc byte[16];
            md5.TryComputeHash(inputBytes, hashBytes, out var written);
            if (written != hashBytes.Length)
                throw new OverflowException();


            Span<char> stringBuffer = stackalloc char[32];
            for (var i = 0; i < hashBytes.Length; i++)
                hashBytes[i].TryFormat(stringBuffer.Slice(2 * i), out _, "x2");
            return new string(stringBuffer).ToLower();
#else
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));
                return sb.ToString().ToLower();
            }
#endif
        }

        #endregion

    }
}
