using Digst.OioIdws.Wsp.BasicPrivilegeProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.ServiceModel;
using System.Text;

namespace Digst.OioIdws.WspSvcExample
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "HelloWorld" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select HelloWorld.svc or HelloWorld.svc.cs at the Solution Explorer and start debugging.
    public class HelloWorld : IHelloWorld
    {
        private const string OioSaml2PrivilegeClaimType = "dk:gov:saml:attribute:Privileges_intermediate";
        private const string OioSaml3PrivilegeClaimType = "https://data.gov.dk/model/core/eid/privilegesIntermediate";

        public string HelloSign(string name)
        {
            return $"Hello Sign {name}. Your claims are:\n{GetClaims()}";
        }

        public string HelloSignError(string name)
        {
            throw new FaultException<string>("DetailInfo",
                $"Hello SignError {name}. You can read signed but not encrypted SOAP faults ... nice!");
        }

        private string GetClaims()
        {
            var identity = (ClaimsIdentity)(OperationContext.Current.ClaimsPrincipal.Identity);

            var stringBuilder = new StringBuilder();
            foreach (var claim in identity.Claims)
            {
                // According to XML spec https://www.w3.org/TR/REC-xml/#sec-line-ends 
                // "To simplify the tasks of applications, the XML processor must behave as if it normalized all line breaks in external parsed entities (including the document entity) on input, before parsing, by translating both the two-character sequence #xD #xA and any #xD that is not followed by #xA to a single #xA character."
                // Somehow when \r\n is used, WCF encodes '\r' into '&#xD;' and take it into account when signing the serialized message. However, when verifying signature, SignedXml always ignores the \r (which is correct per the spec) so the signature becomes invalid.
                // stringBuilder.AppendLine uses \r\n which causes that signature validation problem. The workaround that we use in this example is to use \n for end of line.

                stringBuilder.Append($"Type: {claim.Type}, Value: {claim.Value}\n");

                if (claim.Type == OioSaml2PrivilegeClaimType || claim.Type == OioSaml3PrivilegeClaimType)
                {
                    var privilegeList = BasicPrivilegeProfileDeserializer.DeserializeBase64EncodedPrivilegeList(claim.Value);

                    foreach (var privilegeGroup in privilegeList.PrivilegeGroups)
                    {
                        stringBuilder.Append($"\tPrivilege group scope: {privilegeGroup.Scope}\n");
                        foreach (var privilege in privilegeGroup.Privilege)
                        {
                            stringBuilder.Append($"\t\tPrivilege: {privilege}\n");
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}
