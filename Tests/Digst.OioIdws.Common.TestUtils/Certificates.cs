using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.Common.TestUtils
{
    public static class Certificates
    {
        public static readonly X509Certificate2 STSSignatureValidationTestCertificate = X509CertificateLoader.LoadCertificateFromFile("Data/stssigvalidation.cer");
        public static readonly X509Certificate2 StsCertificate = X509CertificateLoader.LoadCertificateFromFile("Data/sts.cer");
        public static readonly X509Certificate2 TestCertificate = X509CertificateLoader.LoadPkcs12FromFile("Data/Test.p12", "Test1234");

        /// <summary>
        /// There is no expired OCES3 certificate available for testing yet
        /// </summary>
        public static readonly X509Certificate2 ExpiredCertificate = X509CertificateLoader.LoadCertificateFromFile("Data/expired.cert.cer");
        public static readonly X509Certificate2 RevokeCertificate = X509CertificateLoader.LoadCertificateFromFile("Data/revoked.cert.cer");
    }
}
