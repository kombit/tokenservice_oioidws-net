using Digst.OioIdws.CommonCore.Constants;
using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;

namespace Digst.OioIdws.OioWsTrustCore
{
    /// <summary>
    /// This SecurityTokenManager implementation is responsible for returning a correct custom SecurityTokenProvider to request for a token
    /// </summary>
    public class OioWsTrustChannelSecurityTokenManager : WSTrustChannelSecurityTokenManager
    {
        private readonly OioWsTrustChannelClientCredentials _wsTrustChannelClientCredentials;

        /// <summary>
        /// Instantiates a <see cref="WSTrustChannelSecurityTokenManager"/>.
        /// </summary>
        /// <param name="wsTrustChannelClientCredentials"> the WSTrustChannelClientCredentials that can serve up a SecurityTokenProvider to use.</param>
        public OioWsTrustChannelSecurityTokenManager(OioWsTrustChannelClientCredentials wsTrustChannelClientCredentials)
            : base(wsTrustChannelClientCredentials)
        {
            _wsTrustChannelClientCredentials = wsTrustChannelClientCredentials ?? throw new ArgumentNullException(nameof(wsTrustChannelClientCredentials));
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

            if (tokenRequirement.TryGetProperty(SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty, out SecurityTokenParameters issuedSecurityTokenParameters) &&
                issuedSecurityTokenParameters is WSTrustTokenParameters)
            {
                // This is when we need to call an STS to get a token
                // alternatively, use the built-in WSTrustChannelSecurityTokenProvider to avoid reflection
                // and modify Xml message at OioWsTrustChannel.Request using OioWsTrustMessageTransformer
                var provider = new OioWSTrustChannelSecurityTokenProvider(tokenRequirement);
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                var bindingAttr = BindingFlags.NonPublic | BindingFlags.Instance;
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                var propertyInfo = provider.GetType().GetProperty("ClientCredentials", bindingAttr);
                propertyInfo.SetValue(provider, _wsTrustChannelClientCredentials.ClientCredentials);

                provider.CustomClientCredentials = _wsTrustChannelClientCredentials.ClientCredentials;

                return provider;
            }
            // If the original ChannelFactory had a ClientCredentials instance, defer to that
            else if (_wsTrustChannelClientCredentials.SecurityTokenManager != null)
            {
                return _wsTrustChannelClientCredentials.SecurityTokenManager.CreateSecurityTokenProvider(tokenRequirement);
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
