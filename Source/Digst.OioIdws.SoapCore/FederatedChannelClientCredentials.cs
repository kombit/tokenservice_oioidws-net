using System.IdentityModel.Selectors;
using System.ServiceModel.Description;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;

namespace Digst.OioIdws.SoapCore
{
    /// <summary>
    /// A custom ClientCredentials that uses <see cref="FederatedChannelSecurityTokenManager"/> as the SecurityTokenManager
    /// </summary>
    public class FederatedChannelClientCredentials : WSTrustChannelClientCredentials
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FederatedChannelClientCredentials()
            : base()
        {
        }

        /// <summary>
        /// Creates a shallow copy of 'other'.
        /// </summary>
        /// <param name="other">The OioIdwsChannelClientCredentials to copy.</param>
        protected FederatedChannelClientCredentials(FederatedChannelClientCredentials other)
            : base(other)
        {
            SecurityTokenManager = other.SecurityTokenManager;
        }

        /// <summary>
        /// Crates an instance of <see cref="FederatedChannelClientCredentials"/> with specifying a <see cref="ClientCredentials"/>
        /// </summary>
        /// <param name="clientCredentials">The <see cref="SecurityTokenManager"/> from this parameter will be used to
        /// create the <see cref="SecurityTokenProvider"/> in the case the <see cref="SecurityTokenParameters"/> in the channel are not a <see cref="WSTrustTokenParameters"/></para></remarks>
        public FederatedChannelClientCredentials(ClientCredentials clientCredentials)
            : base(clientCredentials)
        {
        }

        /// <summary>
        /// Creates a shallow clone of this.
        /// </summary>
        protected override ClientCredentials CloneCore()
        {
            return new FederatedChannelClientCredentials(this);
        }

        /// <summary>
        /// Returns a <see cref="SecurityTokenManager"/> to use on this channel.
        /// </summary>
        /// <remarks>The <see cref="SecurityTokenManager"/> is responsible to return the <see cref="SecurityTokenProvider"/> to obtain the issued token.
        /// <para>If <see cref="ClientCredentials"/> was passed to the constructor, then that <see cref="SecurityTokenManager"/> will be used to
        /// create the <see cref="SecurityTokenProvider"/> in the case the <see cref="SecurityTokenParameters"/> in the channel are not a <see cref="WSTrustTokenParameters"/></para></remarks>
        /// <returns>An instance of <see cref="WSTrustChannelSecurityTokenManager" />.</returns>
        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            if (ClientCredentials != null)
                SecurityTokenManager = ClientCredentials.CreateSecurityTokenManager();

            return new FederatedChannelSecurityTokenManager((FederatedChannelClientCredentials)Clone());
        }

        internal SecurityTokenManager? SecurityTokenManager { get; private set; }
    }
}
