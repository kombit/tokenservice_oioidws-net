using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;

namespace Digst.OioIdws.Wsp.SubjectConfirmationDataSupport
{
    /// <summary>
    ///     Extends the Saml2SecurityToken class to supported STR-transformed IssuedToken request and Liberty Basic SOAP
    ///     binding specification.
    ///     As-is, WCF can't process such a message:
    ///     <Assertion ID="_f23ef5f3-9efb-40f0-bf38-758d3a9589db" IssueInstant="2015-05-09T16:06:38.484Z" Version="2.0"
    ///         xmlns="urn:oasis:names:tc:SAML:2.0:assertion">
    ///         ...
    ///     </Assertion>
    ///     <o:SecurityTokenReference b:TokenType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0"
    ///         u:Id="_str_f23ef5f3-9efb-40f0-bf38-758d3a9589db"
    ///         xmlns:b="http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd">
    ///         <o:KeyIdentifier ValueType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID">_f23ef5f3-9efb-40f0-bf38-758d3a9589db</o:KeyIdentifier>
    ///     </o:SecurityTokenReference>
    ///     As-is, WCF uses the value of the KeyIdentifier node as the Id of the resolved SecurityKeyIdentifierClause. This
    ///     *is* the correct behavior, except the fact that WCF also has code that
    ///     verifies uniqueness of the header elements and the fact that according to the specs, that KeyIdentifier value is
    ///     set according to the Assertion's ID.
    ///     The workaround is to customize the SecurityKeyIdentifierClause.Id to bypass the uniqueness check. After that,
    ///     override how the KeyIdentifierClause is matched so that the security token can be resolved.
    /// </summary>
    public class StrTransformedSaml2SecurityToken : Saml2SecurityToken
    {
        private const string IdPrefix = "_avoid_duplicate_";

        /// <summary>
        ///     Instantiates a object of type StrTransformedSaml2SecurityToken
        /// </summary>
        /// <param name="assertion">A Saml2 assertion </param>
        public StrTransformedSaml2SecurityToken(Saml2Assertion assertion)
            : base(assertion)
        {
        }

        /// <summary>
        ///     Instantiates a object of type StrTransformedSaml2SecurityToken
        /// </summary>
        /// <param name="assertion">A Saml2 assertion </param>
        /// <param name="keys">Security keys</param>
        /// <param name="issuerToken">Issuer token</param>
        public StrTransformedSaml2SecurityToken(Saml2Assertion assertion, ReadOnlyCollection<SecurityKey> keys,
            SecurityToken issuerToken)
            : base(assertion, keys, issuerToken)
        {
        }

        /// <summary>
        ///     Overrides the Id property to prefix it with a special string to avoid error during uniqueness check.
        /// </summary>
        public override string Id
        {
            get { return IdPrefix + base.Id; }
        }

        /// <summary>
        ///     Tries to match a KeyIdentifierClause with taking IdPrefix into account
        /// </summary>
        /// <param name="keyIdentifierClause"></param>
        /// <returns></returns>
        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (!Saml2AssertionKeyIdentifierClause.Matches(Id, keyIdentifierClause))
            {
                if (!Saml2AssertionKeyIdentifierClause.Matches(Id.Replace(IdPrefix, string.Empty), keyIdentifierClause))
                    return base.MatchesKeyIdentifierClause(keyIdentifierClause);
            }
            return true;
        }
    }
}