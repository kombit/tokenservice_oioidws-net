using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Federation;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore.Bindings;
using Microsoft.IdentityModel.Protocols.WsTrust;

namespace Digst.OioIdws.OioWsTrustCore
{
    /// <summary>
    /// <see cref="IStsTokenService"/>
    /// </summary>
    public class StsTokenService : StsTokenServiceBase
    {
        private readonly StsTokenServiceConfiguration _config;
        public StsTokenService(StsTokenServiceConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // New object is created to ensure that configuration does not change. The alternative would be immutable objects.
            _config = new StsTokenServiceConfiguration(config);
        }

        protected internal override SecurityToken GetToken(StsAuthenticationCase stsAuthenticationCase, Microsoft.IdentityModel.Tokens.SecurityToken? authenticationToken)
        {
            Logger.Instance.Trace(
                $@"RequestToken called with the client certificate: {_config.ClientCertificate.SubjectName.Name} ({_config.ClientCertificate.Thumbprint})");
            Logger.Instance.Trace(
                $@"RequestToken called with the STS certificate: {_config.StsConfiguration.Certificate.SubjectName.Name} ({_config.StsConfiguration.Certificate.Thumbprint})");

            SecurityTokenProvider? tokenProvider = null;

            try
            {
                OioWsTrustBinding stsBinding = new OioWsTrustBinding(stsAuthenticationCase, _config);

                tokenProvider = CreateTokenProvider(stsAuthenticationCase, authenticationToken, stsBinding);
                ((ICommunicationObject)tokenProvider).Open();

                var securityToken = tokenProvider.GetToken(TimeSpan.FromMinutes(1));

                ((ICommunicationObject)tokenProvider).Close();

                return securityToken;
            }
            // Log all errors and rethrow
            catch (Exception e)
            {
                Logger.Instance.Error("Error occured while requesting token. See exception details!", e);

                if (tokenProvider != null)
                {
                    ((ICommunicationObject)tokenProvider).Close();
                }

                throw;
            }
        }

        private SecurityTokenProvider CreateTokenProvider(StsAuthenticationCase stsAuthenticationCase, Microsoft.IdentityModel.Tokens.SecurityToken? authenticationToken, CustomBinding stsBinding)
        {
            // Per https://devblogs.microsoft.com/dotnet/wsfederationhttpbinding-in-net-standard-wcf/
            // .NET Core uses WSFederationHttpBinding to get tokens and call services
            // WSFederationHttpBinding receives a WSTrustTokenParameters object to constructor.
            // WSFederationHttpBinding.CreateBindingElements uses WSTrustTokenParameters to create a WSFederationBindingElement
            // WSFederationBindingElement finds a WSTrustChannelClientCredentials from the BindingParameters (where does it come from?). If not found, create one
            //  and finally create a channelFactory object.
            // The new WSTrustChannelFactory is used to get a token which has a single IssueAsync method.
            // For our own code, OioWsTrustBindingElement is responsible for creating necessary binding elements.
            // By doing this way, we have better control of what we can pass around and how to build a compliant STS request

            // .NET Core does not support asymmetric binding, so it does not call the CreateSecurityTokenAuthenticator method to create an X509SecurityTokenAuthenticator to validate the STS service certificate
            // Implement a custom X509SecurityTokenAuthenticator is not an option because not all necessary types used by that abstract class is exposed to .NET Core
            _config.StsCertificateAuthentication.Validate(_config.StsConfiguration.Certificate);

            WSTrustTokenParameters tokenParams = CreateWSTrustTokenParameters(stsBinding);
            SecurityTokenManager tokenManager = CreateSecurityTokenManager();

            SecurityTokenRequirement tokenRequirement = CreateTokenRequirement(stsAuthenticationCase, authenticationToken, tokenParams);

            SecurityTokenProvider tokenProvider = tokenManager.CreateSecurityTokenProvider(tokenRequirement);
            return tokenProvider;
        }

        private SecurityTokenRequirement CreateTokenRequirement(StsAuthenticationCase stsAuthenticationCase, Microsoft.IdentityModel.Tokens.SecurityToken? authenticationToken, WSTrustTokenParameters tokenParams)
        {
            var tokenRequirement = new SecurityTokenRequirement();
            tokenRequirement.Properties[SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty] = tokenParams;
            tokenRequirement.Properties[SecurityTokenRequirementConstants.TargetAddressProperty] = new EndpointAddress(_config.WspConfiguration.EndpointId);
            tokenRequirement.Properties[SecurityTokenRequirementConstants.CvrProperty] = _config.StsConfiguration.Cvr;
            tokenRequirement.TokenType = Common.OasisSaml2TokenType;
            tokenRequirement.Properties[SecurityTokenRequirementConstants.IssuerProperty] = _config.StsConfiguration.EntityIdentifier;

            // Lifetime is only specified if it has been configured. Should result in a default life time (8 hours) on issued token if not specified. If specified, STS is not obligated to honor this range and may return a token with a shorter life time in RSTR.
            var currentTimeUtc = DateTime.UtcNow;
            if (_config.TokenLifeTimeInMinutes.HasValue)
            {
                tokenRequirement.Properties[SecurityTokenRequirementConstants.LifetimeProperty] = new Lifetime(null,
                    currentTimeUtc.AddMinutes(_config.TokenLifeTimeInMinutes.Value));
            }

            // OnBehalfOf is only set if a bootstrap token or local token is supplied.
            switch (stsAuthenticationCase)
            {
                case StsAuthenticationCase.SignatureCase:
                    break;
                case StsAuthenticationCase.OnBehalfOfCase:
                    ValidateAuthenticationToken(authenticationToken, currentTimeUtc);
                    tokenRequirement.Properties[SecurityTokenRequirementConstants.OnBehalfOfProperty] = authenticationToken;
                    break;

                case StsAuthenticationCase.ProxyOnBehalfOfCase:
                    ValidateAuthenticationToken(authenticationToken, currentTimeUtc);
                    tokenRequirement.Properties[SecurityTokenRequirementConstants.ProxyOnBehalfOfProperty] = authenticationToken;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stsAuthenticationCase), stsAuthenticationCase, null);
            }

            return tokenRequirement;
        }

        private SecurityTokenManager CreateSecurityTokenManager()
        {
            var clientCredentials = new OioWsTrustChannelClientCredentials();
            clientCredentials.ClientCertificate.Certificate = _config.ClientCertificate;
            clientCredentials.ServiceCertificate.SslCertificateAuthentication = _config.SslCertificateAuthentication;
            var trustCredentials = new OioWsTrustChannelClientCredentials(clientCredentials);
            var tokenManager = trustCredentials.CreateSecurityTokenManager();
            return tokenManager;
        }

        private WSTrustTokenParameters CreateWSTrustTokenParameters(CustomBinding stsBinding)
        {
            // WS2007 uses WSTrust 1.3 which is the right call
            WSTrustTokenParameters tokenParams = WSTrustTokenParameters.CreateWS2007FederationTokenParameters(stsBinding, new EndpointAddress(_config.StsConfiguration.EndpointAddress));
            tokenParams.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
            tokenParams.TokenType = Common.OasisSaml2TokenType;
            tokenParams.KeyType = SecurityKeyType.AsymmetricKey;
            return tokenParams;
        }

        private void ValidateAuthenticationToken(Microsoft.IdentityModel.Tokens.SecurityToken? authenticationToken, DateTime currentTimeUtc)
        {
            if(authenticationToken == null)
            {
                throw new ArgumentNullException(nameof(authenticationToken));
            }

            if (authenticationToken.ValidTo < currentTimeUtc)
            {
                var message = $"OBO token life time has expired. Please renew before trying again. OBO token ID: {authenticationToken.Id}, Valid to: {authenticationToken.ValidTo}, Current time: {currentTimeUtc}";
                Logger.Instance.Error(message);
                
                throw new Microsoft.IdentityModel.Tokens.SecurityTokenValidationException(message);
            }
        }
    }
}
