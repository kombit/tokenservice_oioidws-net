using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.OioWsTrustCore;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class OioWSTrustChannelSecurityTokenManagerTest
    {
        [TestMethod]
        public void CreateSecurityTokenProvider_Return_OioWSTrustChannelSecurityTokenProvider()
        {
            // Arrange
            var securityTokenRequirement = new SecurityTokenRequirement();
            securityTokenRequirement.Properties[SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty] = new IssuedSecurityTokenParameters();
            WSTrustTokenParameters tokenParams = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(new CustomBinding(), new EndpointAddress("https://localhost"));
            tokenParams.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
            tokenParams.TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
            tokenParams.KeyType = SecurityKeyType.AsymmetricKey;
            securityTokenRequirement.Properties[SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty] = tokenParams;

            var clientCredentials = new ClientCredentials();
            var oioWsTrustChannelClientCredentials = new OioWsTrustChannelClientCredentials(clientCredentials);
            var sut = new OioWsTrustChannelSecurityTokenManager(oioWsTrustChannelClientCredentials);

            // Act

            var tokenProvider = sut.CreateSecurityTokenProvider(securityTokenRequirement);

            // Assert

            Assert.IsInstanceOfType(tokenProvider, typeof(OioWSTrustChannelSecurityTokenProvider));

            var provider = (OioWSTrustChannelSecurityTokenProvider)tokenProvider;
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var propertyInfo = provider.GetType().GetProperty("ClientCredentials", bindingAttr);
            var setClientCredentials = propertyInfo!.GetValue(provider);

            Assert.AreSame(clientCredentials, setClientCredentials);
        }
    }
}