using Digst.OioIdws.SoapCore.Bindings;
using Digst.OioIdws.SoapCore.Tokens;
using Digst.OioIdws.Common.TestUtils;
using Digst.OioIdws.SoapCore.Test;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.OioWsTrustCore;

namespace Digst.OioIdws.SoapCore.Test;

[TestClass]
public class OioIdwsSoapBindingTest
{
    [TestMethod]
    public void OioIdwsSoapBinding_ReturnCorrectBindingElementCollection()
    {
        // Arrange
        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.StsCertificate);

        var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10),
            stsTokenServiceConfiguration, "https://test.service");
        var soapBinding = new OioIdwsSoapBinding(tokenParams);

        // Act
        var bindingElementCollection = soapBinding.CreateBindingElements();

        // Assert
        Assert.IsNotNull(bindingElementCollection);
        Assert.HasCount(4, bindingElementCollection);
        Assert.IsTrue(bindingElementCollection.Any(x => x is OioIdwsSoapBindingElement));
        Assert.IsTrue(bindingElementCollection.Any(x => x is TextMessageEncodingBindingElement));
        Assert.IsTrue(bindingElementCollection.Any(x => x is HttpsTransportBindingElement));
        Assert.IsTrue(bindingElementCollection.Any(x => x is TransportSecurityBindingElement));
    }

    [TestMethod]
    public void OioIdwsSoapBinding_Constructor_ThrowsWhenTokenParametersIsNull()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new OioIdwsSoapBinding(null!));
    }

    [TestMethod]
    public void OioIdwsSoapBinding_WithSoap11_CreatesCorrectBinding()
    {
        // Arrange
        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.StsCertificate);
        var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
            stsTokenServiceConfiguration, "https://test.service");

        // Act
        var soapBinding = new OioIdwsSoapBinding(tokenParams);
        var elements = soapBinding.CreateBindingElements();

        // Assert
        Assert.IsNotNull(soapBinding);
        Assert.IsTrue(elements.Any(x => x is OioIdwsSoapBindingElement));
    }
}
