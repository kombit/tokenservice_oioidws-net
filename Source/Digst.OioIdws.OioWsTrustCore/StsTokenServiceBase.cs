

using System.IdentityModel.Tokens;

namespace Digst.OioIdws.OioWsTrustCore
{

    /// <summary>
    /// Base implementation of a security token service
    /// </summary>
    /// <seealso cref="Digst.OioIdws.OioWsTrustCore.IStsTokenService" />
    public abstract class StsTokenServiceBase : IStsTokenService
    {

        /// <summary>
        /// This method is used in the signature case scenario where a WSC wants to fetch a token representing the WSC itself.
        /// The STS endpoint, client certificate and WSP endpointID are configured in the configuration file.
        /// This method is thread safe.
        /// </summary>
        public SecurityToken GetToken()
        {
            return GetToken(StsAuthenticationCase.SignatureCase, null);
        }

        /// <summary>
        /// <see cref="IStsTokenService.GetToken()"/>
        /// </summary>
        protected internal abstract SecurityToken GetToken(StsAuthenticationCase stsAuthenticationCase, Microsoft.IdentityModel.Tokens.SecurityToken? authenticationToken );

        /// <summary>
        /// <see cref="IStsTokenService.GetTokenWithBootstrapToken()"/>
        /// </summary>
        public SecurityToken GetTokenWithProxyOnBehalfOf(Microsoft.IdentityModel.Tokens.SecurityToken proxyOboToken)
        {
            return GetToken(StsAuthenticationCase.ProxyOnBehalfOfCase, proxyOboToken);
        }

        /// <summary>
        /// <see cref="IStsTokenService.GetTokenWithOnBehalfOfToken()"/>
        /// </summary>
        public SecurityToken GetTokenWithOnBehalfOf(Microsoft.IdentityModel.Tokens.SecurityToken oboToken)
        {
            return GetToken(StsAuthenticationCase.OnBehalfOfCase, oboToken);
        }
    }
}