using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.Common.TestUtils;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using Digst.OioIdws.CommonCore;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class TrustConfigurationTests
    {
        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void StsTokenServiceCache_ConstructorWorksCorrectly()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var tokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

            // Act
            var stsTokenServiceCache = new StsTokenServiceCache(tokenServiceConfiguration);    // implies that no exception happens

            // Assert
            Assert.IsNotNull(stsTokenServiceCache); // Implies that the object is created without any exception
        }

        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void StsTokenServiceCache_ConstructorThrowsException_WhenConfigurationParameterIsNull()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            StsTokenServiceConfiguration? tokenServiceConfiguration = null;

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new StsTokenServiceCache(tokenServiceConfiguration!));  // force null

            // Assert
        }

        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void GetTokenWillLookUpInCacheThenCallSTSIfNotExists()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("https://testendpoint.com", "https://teststsid.com", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("https://testwspendpoint.com", "https://testwspendpoint.id", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var tokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

            // Act
            var service = new StsTokenServiceCache(tokenServiceConfiguration);

            // Assert
            // Non-existing cache item will cause a request to STS "https://testendpoint.com", as a result, a EndpointNotFoundException will be thrown.
            Assert.ThrowsException<EndpointNotFoundException>(() => service.GetToken());
        }
    }
}