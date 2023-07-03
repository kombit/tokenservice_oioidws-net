using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.WscCore.OioWsTrust
{
    public static class CertificateUtil
    {
        /// <summary>
        /// Finds a certificate in the certificate store.
        /// </summary>
        /// <param name="storeName">Name of store</param>
        /// <param name="storeLocation">Location of store</param>
        /// <param name="x509FindType">Find type that <see cref="findValue"/> must match</param>
        /// <param name="findValue">Value corresponding to <see cref="x509FindType"/></param>
        /// <returns>Return a X509Certificate2 certificate or null if a certificate was not found.</returns>
        public static X509Certificate2 GetCertificate(StoreName storeName, StoreLocation storeLocation, X509FindType x509FindType, string findValue)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                var certificate = store.Certificates.Find(x509FindType, findValue,
                    false).OfType<X509Certificate2>().FirstOrDefault();

                if (certificate == null)
                    throw new InvalidOperationException(
                        string.Format("Certificate with the following configuration was not found: {0}, {1}, {2}, {3}",
                            storeLocation, storeName, x509FindType, findValue));

                return certificate;
            }
        }

        /// <summary>
        /// Get a Certificate from File system
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static X509Certificate2 GetCertificate(string filePath, string password)
        {
            var rawCert = File.ReadAllBytes(filePath);
            if (!string.IsNullOrEmpty(password))
            {
                return new X509Certificate2(rawCert, password);
            }

            return new X509Certificate2(rawCert);
        }

        /// <summary>
        /// Gets a certificate based on a oioidws configuration.
        /// </summary>
        /// <param name="certificate">oioidws configuration of a certificate.</param>
        /// <returns></returns>
        public static X509Certificate2 GetCertificate(Certificate certificate)
        {
            if (!certificate.FromFileSystem)
            {
                return GetCertificate(certificate.StoreName, certificate.StoreLocation,
                certificate.X509FindType, certificate.FindValue);
            }

            return GetCertificate(certificate.FilePath, certificate.Password);
        }
    }
}
