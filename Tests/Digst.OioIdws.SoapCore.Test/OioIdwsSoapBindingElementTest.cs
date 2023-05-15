using Digst.OioIdws.SoapCore.Bindings;
using Digst.OioIdws.SoapCore.Tokens;
using Digst.OioIdws.Common.TestUtils;
using Digst.OioIdws.SoapCore.Test;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.OioWsTrustCore;

namespace Digst.OioIdws.SoapCore.Test;

[TestClass]
public class OioIdwsSoapBindingElementTest
{
    [TestMethod]
    public void BuildChannelFactory_Require_ClientCertificate()
    {
        // Arrange
        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.TestCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

        var bindingContext = new BindingContext(new CustomBinding(), new BindingParameterCollection());
        bindingContext.BindingParameters.Add(new ClientCredentials()); //ClientCredentials without Client certificate
        var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
            stsTokenServiceConfiguration, "https://test.service");
        var sut = new OioIdwsSoapBindingElement(tokenParams);
        // Act

        // Assert
        Assert.ThrowsException<InvalidOperationException>(() => sut.BuildChannelFactory<IRequestChannel>(bindingContext));
    }


    [TestMethod]
    public void BuildChannelFactory_With_ClientCertificate_ReturnCorrectFactory()
    {
        // Arrange
        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.TestCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

        var bindingContext = new BindingContext(new CustomBinding(new List<BindingElement> { new HttpsTransportBindingElement() }), new BindingParameterCollection());
        var clientCredential = new FederatedChannelClientCredentials();
        clientCredential.ClientCertificate.Certificate = Certificates.TestCertificate;
        bindingContext.BindingParameters.Add(clientCredential);
        var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
            stsTokenServiceConfiguration, "https://test.service");

        var sut = new OioIdwsSoapBindingElement(tokenParams);

        // Act
        var result = sut.BuildChannelFactory<IRequestChannel>(bindingContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(OioIdwsSoapChannelFactory));
    }
}