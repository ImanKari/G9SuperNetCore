using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace G9Common.HelperClass
{
    public static class G9Md5Hash
    {
        /// <summary>
        ///     Create md5 hash by string
        /// </summary>
        /// <param name="input">String input for convert to md5 hash</param>
        /// <returns>Converted md5 hash</returns>

        #region GenerateMd5

        public static string GenerateMd5(this string input)
        {
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));
                return sb.ToString();
            }
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
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));
                return sb.ToString();
            }
        }

        #endregion

    }
}
