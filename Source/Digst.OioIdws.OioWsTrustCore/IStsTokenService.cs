using System.IdentityModel.Tokens;

namespace Digst.OioIdws.OioWsTrustCore
{
    /// <summary>
    /// Used for retrieving a token from KOMBIT STS. The token can then be used to call WSP's (Web Service Providers).
    /// </summary>
    public interface IStsTokenService
    {
        /// <summary>
        /// This method is used in the signature case scenario where a WSC wants to fetch a token representing the WSC itself.
        /// The STS endpoint, client certificate and WSP endpointID are configured in the configuration file.
        /// This method is thread safe.
        /// </summary>
        /// <returns>Returns a token.</returns>
        SecurityToken GetToken();

        /// <summary>
        /// Gets a proxy OBO token
        /// </summary>
        /// <param name="bootstrapToken">A <see cref="X509SecurityToken"/> token which wraps around the proxy certificate</param>
        SecurityToken GetTokenWithProxyOnBehalfOf(Microsoft.IdentityModel.Tokens.SecurityToken proxyOboToken);

        /// <summary>
        /// Gets an OBO token
        /// </summary>
        /// <param name="oboToken">A token issued by a trusted STS, which is effectively is the one you get by using the <see cref="GetToken"/> method</param>
        SecurityToken GetTokenWithOnBehalfOf(Microsoft.IdentityModel.Tokens.SecurityToken oboToken);
    }
}