using System.IdentityModel.Selectors;
using System.ServiceModel.Description;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;

namespace Digst.OioIdws.OioWsTrustCore
{
    /// <summary>
    /// A custom WSTrustChannelClientCredentials that is responsible for creating an instance of OioWsTrustChannelSecurityTokenManager 
    /// </summary>
    public class OioWsTrustChannelClientCredentials : WSTrustChannelClientCredentials
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OioWsTrustChannelClientCredentials()
            : base()
        {
        }

        /// <summary>
        /// Creates a shallow copy of 'other'.
        /// </summary>
        /// <param name="other">The WSTrustChannelClientCredentials to copy.</param>
        protected OioWsTrustChannelClientCredentials(OioWsTrustChannelClientCredentials other)
            : base(other)
        {
            SecurityTokenManager = other.SecurityTokenManager;
        }

        /// <summary>
        /// Crates an instance of <see cref="WSTrustChannelClientCredentials"/> with specifying a <see cref="ClientCredentials"/>
        /// </summary>
        /// <param name="clientCredentials">The <see cref="SecurityTokenManager"/> from this parameter will be used to
        /// create the <see cref="SecurityTokenProvider"/> in the case the <see cref="SecurityTokenParameters"/> in the channel are not a <see cref="WSTrustTokenParameters"/></para></remarks>
        public OioWsTrustChannelClientCredentials(ClientCredentials clientCredentials)
            : base(clientCredentials)
        {
        }

        /// <summary>
        /// Creates a shallow clone of this.
        /// </summary>
        protected override ClientCredentials CloneCore()
        {
            return new OioWsTrustChannelClientCredentials(this);
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

            return new OioWsTrustChannelSecurityTokenManager((OioWsTrustChannelClientCredentials)Clone());
        }

        internal SecurityTokenManager? SecurityTokenManager { get; private set; }
    }
}
