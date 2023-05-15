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
        public void StsTokenService_Require_Configuration()
        {
            // Setup

            // Act

            // Assert
            Assert.ThrowsException<ArgumentNullException>(() => { new StsTokenService(null!); });   // force allow null for testing purpose
        }

        [TestMethod]
        public void StsTokenService_Does_Not_Allow_Revoked_StsCertificate()
        {
            // Setup
            var config = new StsTokenServiceConfiguration
            (new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.RevokeCertificate),
             new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate),
             Certificates.TestCertificate);

            var service = new StsTokenService(config);
            // Act

            // Assert
            Assert.ThrowsException<SecurityTokenValidationException>(() => { service.GetToken(); }, "StsCertificate");
        }
    }
}