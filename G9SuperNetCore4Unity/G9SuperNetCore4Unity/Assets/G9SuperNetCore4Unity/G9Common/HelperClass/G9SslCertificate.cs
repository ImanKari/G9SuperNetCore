using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
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
            // Check certificates
            if (exportableCertificates == null || !exportableCertificates.Any())
                throw new ArgumentException($"Certificate is required!", nameof(exportableCertificates));

            // Check private key and set default private key if it's null or empty
            if (string.IsNullOrEmpty(privateKey))
                privateKey = DEFAULT_PRIVATE_KEY;

            // Generate new private key by private key
            PrivateKey = GenerateNewPrivateKey(privateKey);

            // Set certificates
            Certificates = exportableCertificates;
        }

        #endregion

#if NETSTANDARD2_1

        /// <summary>
        ///     <para>Constructor</para>
        ///     <para>Initialize requirement</para>
        ///     <para>Use random generated certificate</para>
        /// </summary>
        /// <param name="privateKey">
        ///     <para>
        ///         Notice: This is not a certificate password, it is a private, shared key between the client and the server for
        ///         secure communication
        ///     </para>
        ///     <para>Specified custom private key (not is cer</para>
        ///     <para>Maximum length is 255</para>
        /// </param>
        /// <param name="countOfRandomCertificateGenerate">
        /// <para>Specified count of the certificate to be generated randomly</para>
        /// <para>Notice: Certificate generated programmatically with random data</para>
        /// </param>
#region G9SslCertificate
        public G9SslCertificate(string privateKey, ushort countOfRandomCertificateGenerate, string countryCode = "US")
        {
            // Check private key and set default private key if it's null or empty
            if (string.IsNullOrEmpty(privateKey))
                privateKey = DEFAULT_PRIVATE_KEY;

            // Generate new private key by private key
            PrivateKey = GenerateNewPrivateKey(privateKey);

            // Check country code
            if (countryCode.Length != 2)
                throw new ArgumentException(LogMessage.CountryCodeIsTwoChar, nameof(countryCode));

            // Initialize certificates array
            Certificates = new X509Certificate2[countOfRandomCertificateGenerate];

            // Initialize random pfx certificates
            Parallel.For(0, countOfRandomCertificateGenerate, (index) =>
            {
                Certificates[index] = GenerateCustomX509Certificate2(
                    $"{STARTER_PRIVATE_KEY}{Guid.NewGuid()}",
                    $"{DEFAULT_PRIVATE_KEY}{Guid.NewGuid()}",
                    $"XIXO-{Guid.NewGuid()}",
                    countryCode: countryCode);
            });
        }
#endregion

        /// <summary>
        ///     Generate custom X509Certificate2 like pfx
        /// </summary>

#region GenerateCustomX509Certificate2

        public static X509Certificate2 GenerateCustomX509Certificate2(string commonName, string password,
            string friendlyName = null, string[] dnsNames = null, DateTime? expirationBefore = null,
            DateTime? expirationAfter = null, bool isCertificateAuthority = false, string countryCode = "US",
            string organization = "JCCE", string[] organizationalUnits = null)
        {
            var sanBuilder = new SubjectAlternativeNameBuilder();
            if (dnsNames == null)
            {
                sanBuilder.AddIpAddress(IPAddress.Loopback);
                sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
                sanBuilder.AddDnsName("localhost");
                sanBuilder.AddDnsName(Environment.MachineName);
            }
            else
            {
                foreach (var dnsName in dnsNames) sanBuilder.AddDnsName(dnsName);
            }

            if (countryCode.Length != 2) countryCode = "US";

            if (organizationalUnits == null)
                organizationalUnits = new[] { "Copyright (c), " + DateTime.UtcNow.ToString("yyyy") + " JCCE" };

            var dn = new StringBuilder();

            dn.Append("CN=\"" + commonName.Replace("\"", "\"\"") + "\"");
            foreach (var ou in organizationalUnits) dn.Append(",OU=\"" + ou.Replace("\"", "\"\"") + "\"");
            dn.Append(",O=\"" + organization.Replace("\"", "\"\"") + "\"");
            dn.Append(",C=" + countryCode.ToUpper());
            dn.Append(",C=" + "JP");

            var strDn = dn.ToString();

            var distinguishedName = new X500DistinguishedName(strDn);

            X509Certificate2 cert;

            using (var rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                var usages = X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment |
                             X509KeyUsageFlags.DigitalSignature;

                if (isCertificateAuthority) usages = usages | X509KeyUsageFlags.KeyCertSign;

                request.CertificateExtensions.Add(new X509KeyUsageExtension(usages, false));


                request.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                if (isCertificateAuthority)
                    request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 1, true));

                if (expirationAfter == null) expirationAfter = DateTime.UtcNow.AddDays(-1).AddYears(10);

                if (expirationBefore == null) expirationBefore = DateTime.UtcNow;

                var certificate = request.CreateSelfSigned(new DateTimeOffset(expirationBefore.Value),
                    new DateTimeOffset(expirationAfter.Value));
                if (friendlyName == null) friendlyName = commonName;
                certificate.FriendlyName = friendlyName;

                cert = new X509Certificate2(certificate.Export(X509ContentType.Pfx, password), password,
                    X509KeyStorageFlags.Exportable);

                // If need certificate pfx file
                // File.WriteAllBytes(path, certificate.Export(X509ContentType.Pkcs12, Password));
            }

            return cert;
        }

#endregion
#endif

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