using System;
using System.Collections.Generic;
using System.Text;

namespace Digst.OioIdws.CommonCore.Constants
{
    /// <summary>
    /// Defines constants that are used to build SOAP messages
    /// </summary>
    public static class Common
    {
        public const string SamlValueType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID";
        public const string OasisSaml2TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
        public const string X509V3TokenProfile = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";
        public const string Base64BinaryEncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";

        // Datetime formats
        public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ"; // Results in format 2015-01-14T14:50:24Z mandated by spec.
    }
}
