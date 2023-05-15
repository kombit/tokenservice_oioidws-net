using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.OioWsTrustCore.Bindings;
using Digst.OioIdws.OioWsTrustCore.ProtocolChannel;
using Digst.OioIdws.Common.TestUtils;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Digst.OioIdws.CommonCore;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class OioWsTrustBindingElementTest
    {
        [TestMethod]
        public void BuildChannelFactory_Require_ClientCertificate()
        {
            // Arrange
            var bindingContext = new BindingContext(new CustomBinding(), new BindingParameterCollection());
            bindingContext.BindingParameters.Add(new ClientCredentials()); //ClientCredentials without Client certificate

            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.TestCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var sut = new OioWsTrustBindingElement(new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate), StsAuthenticationCase.SignatureCase);
            // Act

            // Assert
            Assert.ThrowsException<InvalidOperationException>(() => sut.BuildChannelFactory<TestRequestChannel>(bindingContext));
        }


        [TestMethod]
        public void BuildChannelFactory_With_ClientCertificate_ReturnCorrectFactory()
        {
            // Arrange
            var bindingContext = new BindingContext(new CustomBinding(new List<BindingElement> { new HttpsTransportBindingElement() }), new BindingParameterCollection());
            var clientCredential = new ClientCredentials();
            clientCredential.ClientCertificate.Certificate = Certificates.TestCertificate;
            bindingContext.BindingParameters.Add(clientCredential);

            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.TestCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var sut = new OioWsTrustBindingElement(new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate), StsAuthenticationCase.SignatureCase);

            // Act
            var result = sut.BuildChannelFactory<IRequestChannel>(bindingContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result is OioWsTrustChannelFactory);
        }
    }
}