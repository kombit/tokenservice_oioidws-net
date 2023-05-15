using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.Common.TestUtils;
using System.ServiceModel;
using Digst.OioIdws.CommonCore;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class OioWsTrustChannelFactoryTest
    {
        [TestMethod]
        public void TestCreateChannel()
        {
            // Setup
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.TestCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var config = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate)
            {
                SendTimeout = new TimeSpan(0, 0, 30)
            };

            var sut = new TestOioWsTrustChannelFactory(new TestChannelFactory(), config, StsAuthenticationCase.SignatureCase);
            // Act
            var requestChannel = sut.TestCreateChannel(new System.ServiceModel.EndpointAddress("https://test.com"), new Uri("https://another.uri"));
            // Assert

            Assert.IsNotNull(requestChannel);
        }
    }
}