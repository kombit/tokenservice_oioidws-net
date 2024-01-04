using Digst.OioIdws.SoapCore.Tokens;
using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace Digst.OioIdws.SoapCore
{
    /// <summary>
    /// The way WSTrust in WCF Core works is that it uses a SecurityTokenProvider to request an STS for a token and
    /// use that token to request a service
    /// Because we have a token already obtained from the STS previously, this custom provider "just" returns the token
    /// </summary>
    public class FederatedTokenSecurityTokenProvider : SecurityTokenProvider
    {
        private readonly FederatedSecurityTokenParameters _federatedSecurityTokenParameters;

        public FederatedTokenSecurityTokenProvider(FederatedSecurityTokenParameters federatedSecurityTokenParameters)
        {
            _federatedSecurityTokenParameters = federatedSecurityTokenParameters ?? throw new ArgumentNullException(nameof(federatedSecurityTokenParameters));
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return _federatedSecurityTokenParameters.SecurityToken;
        }
    }
}
