using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Xml;

namespace Digst.OioIdws.Wsp.SubjectConfirmationDataSupport
{
    public class SubjectConfirmationDataSaml2SecurityTokenHandler : Saml2SecurityTokenHandler
    {
        /// <summary>
        /// Override Confirmation Data validation (which is not supported for the example WCF service) to ignore the Error ID4157 because Saml2SecurityTokenHandler can not handle SubjectConfirmationData by default.
        /// </summary>
        /// <param name="confirmationData"></param>
        protected override void ValidateConfirmationData(Saml2SubjectConfirmationData confirmationData)
        {
            try
            {
                base.ValidateConfirmationData(confirmationData);
            }
            catch (Exception ex)
            {
                if ((!ex.Message.Contains("ID4157")) || (confirmationData.Recipient == null))
                    throw;
            }
        }

        public override SecurityToken ReadToken(XmlReader reader)
        {
            Saml2Assertion assertion = ReadAssertion(reader);

            ReadOnlyCollection<SecurityKey> keys = ResolveSecurityKeys(assertion, Configuration.ServiceTokenResolver);

            // Resolve signing token if one is present. It may be deferred and signed by reference.
            SecurityToken issuerToken;
            TryResolveIssuerToken(assertion, Configuration.IssuerTokenResolver, out issuerToken);

            return new StrTransformedSaml2SecurityToken(assertion, keys, issuerToken);
        }

        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            CertificateValidator = new OcesX509CertificateValidator();
            return base.ValidateToken(token);
        }
    }
}
