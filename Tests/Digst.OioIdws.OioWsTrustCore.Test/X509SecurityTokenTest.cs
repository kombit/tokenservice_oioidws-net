using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.Common.TestUtils;
using Microsoft.IdentityModel.Tokens;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class X509SecurityTokenTest
    {
        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void X509SecurityToken_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var certificate = Certificates.StsCertificate;

            // Act
            var x509SecurityToken = new X509SecurityToken(certificate);

            // Assert
            Assert.IsTrue(x509SecurityToken.SecurityKey is X509SecurityKey);
            Assert.AreEqual(certificate.Issuer, x509SecurityToken.Issuer);
            Assert.AreEqual(certificate.Thumbprint, x509SecurityToken.Id);
            Assert.AreEqual(certificate.NotBefore.ToUniversalTime(), x509SecurityToken.ValidFrom);
            Assert.AreEqual(certificate.NotAfter.ToUniversalTime(), x509SecurityToken.ValidTo);
        }

        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void X509SecurityToken_Constructor_ThrowsWhenCertificateIsNull()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new X509SecurityToken(null!));
        }

        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void X509SecurityToken_SecurityKey_ReturnsX509SecurityKey()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;
            var x509SecurityToken = new X509SecurityToken(certificate);

            // Act
            var securityKey = x509SecurityToken.SecurityKey;

            // Assert
            Assert.IsInstanceOfType<X509SecurityKey>(securityKey);
            var x509Key = (X509SecurityKey)securityKey;
            Assert.AreEqual(certificate.Thumbprint, x509Key.Certificate.Thumbprint);
        }

        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void X509SecurityToken_ValidDateRange_IsCorrect()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;
            var x509SecurityToken = new X509SecurityToken(certificate);

            // Act & Assert
            Assert.IsTrue(x509SecurityToken.ValidFrom <= DateTime.UtcNow);
            Assert.IsTrue(x509SecurityToken.ValidTo > DateTime.UtcNow);
            Assert.IsTrue(x509SecurityToken.ValidFrom < x509SecurityToken.ValidTo);
        }

        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void X509SecurityToken_Id_MatchesCertificateThumbprint()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;
            var x509SecurityToken = new X509SecurityToken(certificate);

            // Act & Assert
            Assert.AreEqual(certificate.Thumbprint, x509SecurityToken.Id);
            Assert.IsNotNull(x509SecurityToken.Id);
        }
    }
}
