using Digst.OioIdws.Common.TestUtils;

namespace Digst.OioIdws.CommonCore.Test
{
    /// <summary>
    /// Have tests so that we are alerted when valid certificates have become expired.
    /// </summary>
    [TestClass]
    public class CertificatesTest
    {
        [TestMethod]
        public void STSSignatureValidationTestCertificate_ShouldNotBeExpired()
        {
            // Arrange
            var certificate = Certificates.STSSignatureValidationTestCertificate;
            var currentTime = DateTime.UtcNow;

            // Act & Assert
            Assert.IsTrue(certificate.NotBefore <= currentTime, 
                $"Certificate is not yet valid. Not valid before: {certificate.NotBefore}, Current time: {currentTime}");
            Assert.IsTrue(certificate.NotAfter >= currentTime, 
                $"Certificate has expired. Not valid after: {certificate.NotAfter}, Current time: {currentTime}");
        }

        [TestMethod]
        public void StsCertificate_ShouldNotBeExpired()
        {
            // Arrange
            var certificate = Certificates.StsCertificate;
            var currentTime = DateTime.UtcNow;

            // Act & Assert
            Assert.IsTrue(certificate.NotBefore <= currentTime, 
                $"Certificate is not yet valid. Not valid before: {certificate.NotBefore}, Current time: {currentTime}");
            Assert.IsTrue(certificate.NotAfter >= currentTime, 
                $"Certificate has expired. Not valid after: {certificate.NotAfter}, Current time: {currentTime}");
        }

        [TestMethod]
        public void TestCertificate_ShouldNotBeExpired()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;
            var currentTime = DateTime.UtcNow;

            // Act & Assert
            Assert.IsTrue(certificate.NotBefore <= currentTime, 
                $"Certificate is not yet valid. Not valid before: {certificate.NotBefore}, Current time: {currentTime}");
            Assert.IsTrue(certificate.NotAfter >= currentTime, 
                $"Certificate has expired. Not valid after: {certificate.NotAfter}, Current time: {currentTime}");
        }

        [TestMethod]
        public void TestCertificate_ShouldHavePrivateKey()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;

            // Act & Assert
            Assert.IsTrue(certificate.HasPrivateKey, 
                "Test certificate should have a private key since it's loaded from a .p12 file");
        }

        [TestMethod]
        public void ExpiredCertificate_ShouldBeExpired()
        {
            // Arrange
            var certificate = Certificates.ExpiredCertificate;
            var currentTime = DateTime.UtcNow;

            // Act & Assert
            Assert.IsTrue(certificate.NotAfter < currentTime, 
                $"Certificate should be expired. Not valid after: {certificate.NotAfter}, Current time: {currentTime}");
        }
    }
}