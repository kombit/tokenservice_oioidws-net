using Digst.OioIdws.SoapCore.Tokens;
using Digst.OioIdws.Common.TestUtils;
using Digst.OioIdws.SoapCore.Test;
using System.ServiceModel.Channels;
using System.ServiceModel;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.OioWsTrustCore;

namespace Digst.OioIdws.SoapCore.Test;

[TestClass]
public class FederatedTokenSecurityTokenProviderTest
{

    [TestMethod]
    public void GetToken_ReturnTheToken_InParameters()
    {
        // Arrange
        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.TestCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

        var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
            stsTokenServiceConfiguration, "https://test.service");
        var sut = new FederatedTokenSecurityTokenProvider(tokenParams);

        // Act
        var securityToken = sut.GetToken(TimeSpan.FromSeconds(1));

        // Assert
        Assert.IsNotNull(securityToken);
        Assert.IsTrue(securityToken is TestSecurityToken);
    }
}