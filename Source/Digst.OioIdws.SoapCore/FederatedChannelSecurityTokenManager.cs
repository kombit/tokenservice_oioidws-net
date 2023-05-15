using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.SoapCore.Tokens;
using System;
using System.IdentityModel.Selectors;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;

namespace Digst.OioIdws.SoapCore
{
    /// <summary>
    /// This SecurityTokenManager implementation is responsible for returning a correct custom SecurityTokenProvider <see cref="FederatedTokenSecurityTokenProvider"/> to call a WSP
    /// </summary>
    public class FederatedChannelSecurityTokenManager : WSTrustChannelSecurityTokenManager
    {
        private readonly FederatedChannelClientCredentials _federatedChannelClientCredentials;

        /// <summary>
        /// Instantiates a <see cref="FederatedChannelSecurityTokenManager"/>.
        /// </summary>
        /// <param name="wsTrustChannelClientCredentials"> the WSTrustChannelClientCredentials that can serve up a SecurityTokenProvider to use.</param>
        public FederatedChannelSecurityTokenManager(FederatedChannelClientCredentials federatedChannelClientCredentials)
            : base(federatedChannelClientCredentials)
        {
            _federatedChannelClientCredentials = federatedChannelClientCredentials ?? throw new ArgumentNullException(nameof(federatedChannelClientCredentials));
        }

        /// <summary>
        /// Make use of this extensibility point for returning a custom SecurityTokenProvider when SAML tokens are specified in the tokenRequirement
        /// </summary>
        /// <param name="tokenRequirement">A SecurityTokenRequirement  </param>
        /// <returns>The appropriate SecurityTokenProvider</returns>
        public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
        {
            if (tokenRequirement == null)
                throw new ArgumentNullException(nameof(tokenRequirement));

            if (tokenRequirement.TryGetProperty(SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty, out SecurityTokenParameters securityTokenParameters) &&
                securityTokenParameters is FederatedSecurityTokenParameters federatedSecurityTokenParameters)
            {
                // this is when we use a token to call a WSP. Because the token is available already, we need a provider that just returns the token
                return new FederatedTokenSecurityTokenProvider(federatedSecurityTokenParameters);
            }
            // If the original ChannelFactory had a ClientCredentials instance, defer to that
            else if (_federatedChannelClientCredentials.SecurityTokenManager != null)
            {
                return _federatedChannelClientCredentials.SecurityTokenManager.CreateSecurityTokenProvider(tokenRequirement);
            }
            // This means ClientCredentials was replaced with WSTrustChannelClientCredentials in the ChannelFactory so defer
            // to base class to create other token providers.
            else
            {
                return base.CreateSecurityTokenProvider(tokenRequirement);
            }
        }
    }
}
