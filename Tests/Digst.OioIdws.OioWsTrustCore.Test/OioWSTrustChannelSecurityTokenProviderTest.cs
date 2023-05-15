using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.Common.TestUtils;
using Microsoft.IdentityModel.Protocols.WsFed;
using Microsoft.IdentityModel.Protocols.WsTrust;
using Microsoft.IdentityModel.Tokens.Saml2;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class OioWSTrustChannelSecurityTokenProviderTest
    {
        [TestMethod]
        public void OioWSTrustChannelSecurityTokenProvider()
        {
            // Arrange
            var securityTokenRequirement = new SecurityTokenRequirement();
            securityTokenRequirement.Properties[SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty] = new IssuedSecurityTokenParameters();
            WSTrustTokenParameters tokenParams = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(new CustomBinding(), new EndpointAddress("https://localhost"));
            tokenParams.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
            tokenParams.TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
            tokenParams.KeyType = SecurityKeyType.AsymmetricKey;
            securityTokenRequirement.Properties[SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty] = tokenParams;
            securityTokenRequirement.Properties[SecurityTokenRequirementConstants.TargetAddressProperty] = new EndpointAddress("https://wsp");
            securityTokenRequirement.Properties[SecurityTokenRequirementConstants.CvrProperty] = "12345678";

            Lifetime lifetime = new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(30));
            securityTokenRequirement.Properties[SecurityTokenRequirementConstants.LifetimeProperty] = lifetime;

            Microsoft.IdentityModel.Tokens.SecurityToken securityToken = new Saml2SecurityToken(new Saml2Assertion(new Saml2NameIdentifier("testuser")));
            securityTokenRequirement.Properties[SecurityTokenRequirementConstants.OnBehalfOfProperty] = securityToken;

            var sut = new OioWSTrustChannelSecurityTokenProvider(securityTokenRequirement);
            sut.CustomClientCredentials = new ClientCredentials();
            sut.CustomClientCredentials.ClientCertificate.Certificate = Certificates.STSSignatureValidationTestCertificate;
            WsTrustRequest wsTrustRequest = new WsTrustRequest("Issue");

            // Act
            sut.BuildCustomElements(wsTrustRequest);

            // Assert
            Assert.IsNotNull(wsTrustRequest.Claims);
            Assert.AreEqual("http://docs.oasis-open.org/wsfed/authorization/200706/authclaims", wsTrustRequest.Claims.Dialect);
            ClaimType claimType = wsTrustRequest.Claims.ClaimTypes.Single();
            Assert.AreEqual("12345678", claimType.Value);
            Assert.AreEqual("dk:gov:saml:attribute:CvrNumberIdentifier", claimType.Uri);
            Assert.IsFalse(claimType.IsOptional);

            Assert.AreEqual(securityToken, wsTrustRequest.OnBehalfOf.SecurityToken);
            Assert.AreEqual(lifetime, wsTrustRequest.Lifetime);
            Assert.AreEqual("trust:UseKey", wsTrustRequest.AdditionalXmlElements.Single().Name);
        }
    }
}