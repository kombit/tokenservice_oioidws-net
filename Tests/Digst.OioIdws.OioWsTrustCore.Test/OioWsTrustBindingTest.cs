using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.OioWsTrustCore.Bindings;
using Digst.OioIdws.Common.TestUtils;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Digst.OioIdws.CommonCore;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class OioWsTrustBindingTest
    {
        [TestMethod]
        [DataRow(StsAuthenticationCase.OnBehalfOfCase)]
        [DataRow(StsAuthenticationCase.ProxyOnBehalfOfCase)]
        [DataRow(StsAuthenticationCase.SignatureCase)]
        public void CreateBindingElementsTest(StsAuthenticationCase authCase)
        {
            // Setup
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.TestCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var config = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate)
            {
                SendTimeout = new TimeSpan(0, 0, 30)
            };

            var sut = new OioWsTrustBinding(authCase, config);
            // Act
            var elements = sut.CreateBindingElements();
            // Assert
            Assert.IsNotNull(elements);
            Assert.AreEqual(config.SendTimeout, sut.SendTimeout);
            Assert.AreEqual(3, elements.Count);

            var httpsTransportBindingElement = elements.OfType<HttpsTransportBindingElement>().Single();
            Assert.IsTrue(httpsTransportBindingElement.ManualAddressing);

            var oioWsTrustBindingElement = elements.OfType<OioWsTrustBindingElement>().SingleOrDefault();
            Assert.IsNotNull(oioWsTrustBindingElement);

            var textMessageEncodingBindingElement = elements.OfType<TextMessageEncodingBindingElement>().Single();
            Assert.AreEqual(EnvelopeVersion.Soap12, textMessageEncodingBindingElement.MessageVersion.Envelope);
            Assert.AreEqual(AddressingVersion.WSAddressing10, textMessageEncodingBindingElement.MessageVersion.Addressing);
        }
    }
}