using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.SoapCore.Tokens;
using Digst.OioIdws.Common.TestUtils;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.OioWsTrustCore;

namespace Digst.OioIdws.SoapCore.Test;

[TestClass]
public class FederatedChannelSecurityTokenManagerTest
{
    [TestMethod]
    public void CreateSecurityTokenProvider_Return_FederatedTokenSecurityTokenProvider()
    {
        // Arrange
        var securityTokenRequirement = new SecurityTokenRequirement();
        securityTokenRequirement.Properties[SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty] = new IssuedSecurityTokenParameters();

        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.TestCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

        WSTrustTokenParameters tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10), stsTokenServiceConfiguration, "https://test.service");
        securityTokenRequirement.Properties[SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty] = tokenParams;

        var federatedChannelClientCredentials = new FederatedChannelClientCredentials();
        var sut = new FederatedChannelSecurityTokenManager(federatedChannelClientCredentials);

        // Act

        var tokenProvider = sut.CreateSecurityTokenProvider(securityTokenRequirement);

        // Assert

        Assert.IsInstanceOfType(tokenProvider, typeof(FederatedTokenSecurityTokenProvider));
    }
}