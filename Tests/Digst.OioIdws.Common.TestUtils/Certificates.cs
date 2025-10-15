using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.Common.TestUtils
{
    public static class Certificates
    {
        public static readonly X509Certificate2 STSSignatureValidationTestCertificate = new X509Certificate2("Data/stssigvalidation.cer");
        public static readonly X509Certificate2 StsCertificate = new X509Certificate2("Data/sts.cer");
        public static readonly X509Certificate2 TestCertificate = new X509Certificate2("Data/Test.p12", "Test1234");
        public static readonly X509Certificate2 ExpiredCertificate = new X509Certificate2("Data/expired.cert.cer");
        public static readonly X509Certificate2 RevokeCertificate = new X509Certificate2("Data/revoked.cert.cer");
    }
}
