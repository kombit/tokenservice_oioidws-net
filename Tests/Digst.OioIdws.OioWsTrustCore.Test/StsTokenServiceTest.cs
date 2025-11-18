using Digst.OioIdws.Common.TestUtils;
using Microsoft.IdentityModel.Tokens;
using System.ServiceModel;
using Digst.OioIdws.CommonCore;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class StsTokenServiceTest
    {
        [TestMethod]
        public void StsTokenService_Constructor_Require_Configuration()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new StsTokenService(null!));
        }

        [TestMethod]
        public void StsTokenService_Does_Not_Allow_Revoked_StsCertificate()
        {
            // Arrange
            var config = new StsTokenServiceConfiguration(
                new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.RevokeCertificate),
                new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate),
                Certificates.TestCertificate);

            var service = new StsTokenService(config);

            // Act & Assert
            Assert.ThrowsExactly<SecurityTokenValidationException>(() => service.GetToken());
        }

        [TestMethod]
        public void StsTokenService_Does_Not_Allow_Expired_StsCertificate()
        {
            // Arrange
            var config = new StsTokenServiceConfiguration(
                new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.ExpiredCertificate),
                new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate),
                Certificates.TestCertificate);

            var service = new StsTokenService(config);

            // Act & Assert
            Assert.ThrowsExactly<SecurityTokenValidationException>(() => service.GetToken());
        }

        [TestMethod]
        public void StsTokenService_Constructor_WithValidConfiguration_CreatesInstance()
        {
            // Arrange
            var config = new StsTokenServiceConfiguration(
                new StsConfiguration("https://testendpoint", "https://teststsid", "12345678", Certificates.TestCertificate),
                new WspConfiguration("https://testwspendpoint", "https://testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate),
                Certificates.TestCertificate);

            // Act
            var service = new StsTokenService(config);

            // Assert
            Assert.IsNotNull(service);
        }
    }
}