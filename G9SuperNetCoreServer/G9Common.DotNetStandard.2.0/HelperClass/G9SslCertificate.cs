using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using G9Common.RandomNumberWithoutDuplicates;
using G9Common.Resource;

namespace G9Common.HelperClass
{
    public class G9SslCertificate
    {
        #region Fields And Properties

        /// <summary>
        ///     Specified starter private key
        /// </summary>
        protected const string STARTER_PRIVATE_KEY = "G9TM-";

        /// <summary>
        ///     Specified default private key
        /// </summary>
        protected const string DEFAULT_PRIVATE_KEY = "@ThisIsG9Team";

        /// <summary>
        ///     Specified private key
        ///     Maximum length is 255
        /// </summary>
        public readonly string PrivateKey;

        /// <summary>
        ///     Specified certificates for ssl connection
        /// </summary>
        public readonly X509Certificate2[] Certificates;

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        ///     <para>Constructor</para>
        ///     <para>Initialize requirement</para>
        /// </summary>
        /// <param name="privateKey">
        ///     <para>
        ///         Notice: This is not a certificate password, it is a private, shared key between the client and the server for
        ///         secure connection (SSL)
        ///     </para>
        ///     <para>Specified custom private key</para>
        ///     <para>Maximum length is 255</para>
        /// </param>
        /// <param name="exportableCertificates">Exportable certificates</param>

        #region G9SslCertificate

        public G9SslCertificate(string privateKey, params X509Certificate2[] exportableCertificates)
        {
            // Check private key and set default private key if it's null or empty
            if (string.IsNullOrEmpty(privateKey))
                privateKey = DEFAULT_PRIVATE_KEY;

            // Generate new private key by private key
            PrivateKey = GenerateNewPrivateKey(privateKey);

            // Set certificates
            Certificates = exportableCertificates;
        }

        #endregion

        #endregion

        /// <summary>
        ///     Generate new private key by private key
        /// </summary>
        /// <param name="privateKey">specified private key</param>
        /// <returns>Generated new private key</returns>

        #region GenerateNewPrivateKey

        public static string GenerateNewPrivateKey(string privateKey)
        {
            if (privateKey.Length > 218)
            {
                privateKey = privateKey.Substring(0, 186) + privateKey.Substring(186).GenerateMd5();
            }
            else if (privateKey.Length < 218)
            {
                int counter = 0;
                while (privateKey.Length < 218)
                {
                    if ((privateKey.Length & 1) == 1)
                        privateKey += privateKey.Substring(privateKey.Length - counter - 2).GenerateMd5() + "9";
                    else
                        privateKey = privateKey.Substring(counter + 2).GenerateMd5() + privateKey + "9";
                    counter++;
                }

                privateKey = privateKey.Substring(0, 186) + privateKey.Substring(186).GenerateMd5();
            }
            else
            {
                privateKey += privateKey.GenerateMd5();
            }

            return STARTER_PRIVATE_KEY + privateKey;
        }

        #endregion

        #endregion
    }
}