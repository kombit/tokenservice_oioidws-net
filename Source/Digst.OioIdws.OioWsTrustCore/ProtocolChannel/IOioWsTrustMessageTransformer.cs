using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.Xml;

namespace Digst.OioIdws.OioWsTrustCore.ProtocolChannel
{
    /// <summary>
    /// Transforms the messages to be compliant with the Signature case scenario of the specification [Snitfladebeskrivelse-STS-OIO WS Trust][Snitfladebeskrivelse-STS-OnBehalfOf Proxy]
    /// </summary>
    public interface IOioWsTrustMessageTransformer
    {
        /// <summary>
        /// Transforms a default WCF/WIF RST request into a proprietary request that KOMBIT STS understands.
        /// </summary>
        /// <param name="request">The default RST request from WCF/WIF</param>
        Message ModifyMessageAccordingToStsNeeds(Message request);

        /// <summary>
        /// Transforms a proprietary KOMBIT RSTR response into a standard WCF/WIF RSTR response.
        /// Validating of the response must also done. Thus, signature validation, replay attack validation and expiry time validation.
        /// </summary>
        /// <param name="response">The response from KOMBIT STS</param>
        /// <param name="matchedMessageId">The unique message Id to validate against the Response's RelatesTo</param>
        Message ValidateAndModifyMessageAccordingToWsTrust(Message response, UniqueId matchedMessageId);
    }
}
