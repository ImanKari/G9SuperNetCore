﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using G9Common.Resource;

namespace G9Common.HelperClass
{
    /// <summary>
    ///     Encrypt Decrypt management by fpx certificates
    /// </summary>
    public class G9EncryptAndDecryptDataWithCertificate
    {
        #region Fields And Propeties

        /// <summary>
        ///     Field for save object of G9SslCertificate
        /// </summary>
        private readonly G9SslCertificate _sslCertificate;

        /// <summary>
        ///     Save encrypt object generated by public key
        /// </summary>
        private readonly RSA[] _encryptCryptoArray;

        /// <summary>
        ///     Save decrypt object generated by private key
        /// </summary>
        private readonly RSA[] _decryptCryptoArray;

        /// <summary>
        ///     Specified number of cert
        /// </summary>
        private readonly ushort _numberOfCert;

        /// <summary>
        ///     Random object for generate random number
        /// </summary>
        private readonly Random _random = new Random();

        /// <summary>
        ///     Specified private key
        /// </summary>
        public readonly string PrivateKey;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sslCertificate">Specified object of G9SslCertificate</param>
        /// <param name="exportableCheck">Check certificate is exportable</param>

        #region G9EncryptAndDecryptDataWithCertificate

        public G9EncryptAndDecryptDataWithCertificate(G9SslCertificate sslCertificate, bool exportableCheck = true)
        {
            // Check certificates is exportable
            if (exportableCheck &&
                sslCertificate.Certificates.Any(s => !CheckCertificateIsExportable(s, X509ContentType.Pkcs12)))
                throw new Exception(LogMessage.CertificateIsNotExportable);

            // Check Certificate for private key
            if (sslCertificate.Certificates.Any(s => !s.HasPrivateKey))
                throw new Exception(LogMessage.CertificateDoseNotHavePrivateKey);

            // Set certificates
            _sslCertificate = sslCertificate;

            // Initialize crypto array
            _encryptCryptoArray = new RSA[_sslCertificate.Certificates.Length];
            _decryptCryptoArray = new RSA[_sslCertificate.Certificates.Length];

            // Set crypto array
            for (var i = 0; i < _sslCertificate.Certificates.Length; i++)
            {
                _encryptCryptoArray[i] = (RSA)_sslCertificate.Certificates[i].PublicKey.Key;
                _decryptCryptoArray[i] = (RSA)_sslCertificate.Certificates[i].PrivateKey;
            }

            // Set number of cert
            _numberOfCert = (ushort) _sslCertificate.Certificates.Length;

            // Set read only public private key
            PrivateKey = _sslCertificate.PrivateKey;
        }

        #endregion

        /// <summary>
        ///     Encrypt data
        /// </summary>
        /// <param name="data">Data for encrypt</param>
        /// <param name="certNumber">Number of cert</param>
        /// <returns>Encrypted data</returns>

        #region EncryptDataByCertificate

        public byte[] EncryptDataByCertificate(byte[] data, ushort certNumber)
        {
            // Check cert number fond
            if (certNumber >= _numberOfCert)
                throw new Exception($"Cert number {certNumber} for encrypt not found!");
            // Encrypt data
            return _encryptCryptoArray[certNumber].Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        #endregion

        /// <summary>
        ///     Decrypt data
        /// </summary>
        /// <param name="data">Data for decrypt</param>
        /// <param name="certNumber">Number of cert</param>
        /// <returns>Decrypted data</returns>

        #region DecryptDataWithCertificate

        public byte[] DecryptDataWithCertificate(byte[] data, ushort certNumber)
        {
            // Check cert number fond
            if (certNumber >= _numberOfCert)
                throw new Exception($"Cert number {certNumber} for encrypt not found!");
            // Decrypt data
            return _decryptCryptoArray[certNumber].Decrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        #endregion

        /// <summary>
        ///     Get random certificate number
        /// </summary>
        /// <returns>Get random certificate number</returns>

        #region GetRandomCertificate

        public ushort GetRandomCertificateNumber()
        {
            // if cert not found
            if (_numberOfCert == 0)
                return 0;
            // return random cert number
            return (ushort) _random.Next(0, _numberOfCert);
        }

        #endregion

        #region GetCertificateByCertificateNumber

        public byte[] GetCertificateByCertificateNumber(ushort certificateNumber, string mdf5ExtraPass32Char)
        {
            // Check extra pass length
            if (mdf5ExtraPass32Char.Length != 32)
                throw new ArgumentException(LogMessage.ExtraPassIsNot32Char, nameof(mdf5ExtraPass32Char));

            return _sslCertificate.Certificates[certificateNumber]
                .Export(X509ContentType.Pkcs12, $"{PrivateKey}{mdf5ExtraPass32Char}");
        }

        #endregion

        /// <summary>
        ///     Check certificate is exportable
        /// </summary>
        /// <param name="certForCheck">Certificate for check</param>
        /// <param name="certType">Certificate type</param>
        /// <returns>Return true if certificate is exportable</returns>

        #region CheckCertificateIsExportable

        public static bool CheckCertificateIsExportable(X509Certificate2 certForCheck, X509ContentType certType)
        {
            try
            {
                certForCheck.Export(certType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #endregion
    }
}