using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace G9SuperNetCoreCommon.AESEncryptionDecryption
{
    /// <summary>
    ///     Encryption Decryption AES 128
    /// </summary>
    public static class AES128
    {
        private static Aes CustomAes;

        static AES128()
        {
            CustomAes = Aes.Create();
            CustomAes.KeySize = 128;
            CustomAes.BlockSize = 128;
            CustomAes.Padding = PaddingMode.PKCS7;
        }

        #region EncryptBytesToBytes

        public static byte[] EncryptBytesToBytes(byte[] plainText, byte[] privateKey, byte[] publicKey,
            out string message)
        {
            try
            {
                using (var encryptor = CustomAes.CreateEncryptor(privateKey, publicKey))
                {
                    message = null;
                    return PerformCryptography(plainText, encryptor);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return null;
            }
        }

        #endregion

        /// <summary>
        ///     Decrypt bytes from bytes
        /// </summary>
        /// <param name="cipherText">Byte cipher text</param>
        /// <param name="privateKey">Byte private key</param>
        /// <param name="publicKey">Byte public key</param>
        /// <param name="message"> If parameter is null method success else contain error message</param>
        /// <returns>Decrypt byte[]</returns>

        #region DecryptBytesFromBytes

        public static byte[] DecryptBytesFromBytes(byte[] cipherText, byte[] privateKey, byte[] publicKey,
            out string message)
        {
            try
            {
                using (var decryptor = CustomAes.CreateDecryptor(privateKey, publicKey))
                {
                    message = null;
                    return PerformCryptography(cipherText, decryptor);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message == "The input data is not a complete block."
                    ? "The input data is not a complete block.\nCipher text not correct"
                    : ex.Message;
                return null;
            }
        }

        #endregion

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }
    }
}