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
[TestCategory(Constants.UnitTest)]
public class OioIdwsSoapBindingTest
{
    [TestMethod]
    public void OioIdwsSoapBinding_ReturnCorrectBindingElementCollection()
    {
        // Arrange
        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.StsCertificate);

        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.StsCertificate);

        var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10),
            stsTokenServiceConfiguration, "https://test.service");
        var soapBinding = new OioIdwsSoapBinding(tokenParams);
        // Act

        var bindingElementCollection = soapBinding.CreateBindingElements();

        // Assert
        Assert.IsNotNull(bindingElementCollection);

        Assert.AreEqual(4, bindingElementCollection.Count);
        Assert.IsTrue(bindingElementCollection.Any(x => x is OioIdwsSoapBindingElement));
        Assert.IsTrue(bindingElementCollection.Any(x => x is TextMessageEncodingBindingElement));
        Assert.IsTrue(bindingElementCollection.Any(x => x is HttpsTransportBindingElement));
        Assert.IsTrue(bindingElementCollection.Any(x => x is TransportSecurityBindingElement));
    }
}
