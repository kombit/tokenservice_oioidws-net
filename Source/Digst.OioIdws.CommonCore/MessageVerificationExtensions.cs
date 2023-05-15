using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.CommonCore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Digst.OioIdws.CommonCore
{
    /// <summary>
    /// Contains extension methods for verifying the required signature elements in an XML document.
    /// </summary>
    public static class MessageVerificationExtensions
    {
        /// <summary>
        /// Verifies the required signature elements in the specified XML element for the given XML document, assuming the SOAP 1.2 envelope version.
        /// </summary>
        /// <param name="signatureElement">The XML element containing the signature to verify.</param>
        /// <param name="xDocument">The XML document to verify the signature against.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="signatureElement"/> is null.</exception>
        public static void VerifyRequiredSignatureElements(this XmlElement signatureElement, XDocument xDocument)
        {
            if (signatureElement == null)
            {
                throw new ArgumentNullException(nameof(signatureElement));
            }

            signatureElement.VerifyRequiredSignatureElements(xDocument, EnvelopeVersion.Soap12);
        }

        /// <summary>
        /// Verifies the required signature elements in the specified XML element for the given XML document and SOAP envelope version.
        /// </summary>
        /// <param name="signatureElement">The XML element containing the signature to verify.</param>
        /// <param name="xDocument">The XML document to verify the signature against.</param>
        /// <param name="envelopeVersion">The version of the SOAP envelope to assume.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="signatureElement"/> is null.</exception>
        public static void VerifyRequiredSignatureElements(this XmlElement signatureElement, XDocument xDocument, EnvelopeVersion envelopeVersion)
        {
            if (signatureElement == null)
                throw new ArgumentNullException(nameof(signatureElement));

            XmlNamespaceManager namespaceManager = CreateNamespaceManager(envelopeVersion);

            var signedInfo = XElement.Parse(signatureElement.OuterXml).Elements().First(x => x.Name.LocalName == "SignedInfo");
            var refUries = signedInfo.Elements().Where(x => x.Name.LocalName == "Reference")
                .Select(x => x.Attribute("URI").Value).ToList();

            // Ensure that the Body (at correct position - not wrapped) is signed
            VerifyRequiredElement(xDocument, refUries, "/s:Envelope/s:Body", namespaceManager, "Body '{0}' is required and must be signed.");

            // Action
            var xAction = xDocument.XPathSelectElement("/s:Envelope/s:Header/wsa:Action", namespaceManager);
            // As "OIO IDWS SOAP profile (V1.1)", The <wsa:Action> header is no longer mandated.
            if (xAction != null)
            {
                VerifyRequiredElement(xDocument, refUries, "/s:Envelope/s:Header/wsa:Action", namespaceManager,
                "Action '{0}' is required and must be signed.");
            }

            var isWsTrustMessage = xDocument.XPathSelectElement("/s:Envelope/s:Body/wst:RequestSecurityTokenResponseCollection", namespaceManager) != null;
            // MessageID is only required for the SOAP response from WSP ([OIO-IDWS-SOAP]), not STS. KOMBIT STS response does not include the MessageID header
            if (!isWsTrustMessage)
            {
                VerifyRequiredElement(xDocument, refUries, "/s:Envelope/s:Header/wsa:MessageID", namespaceManager, "MessageID '{0}' is required and must be signed.");
            }

            // RelatesTo
            VerifyRequiredElement(xDocument, refUries, "/s:Envelope/s:Header/wsa:RelatesTo", namespaceManager, "RelatesTo '{0}' is required and must be signed.");

            // Timestamp
            VerifyRequiredElement(xDocument, refUries, "/s:Envelope/s:Header/wsse:Security/wsu:Timestamp", namespaceManager, "Security's Timestamp '{0}' is required and must be signed.");
        }

        private static XmlNamespaceManager CreateNamespaceManager(EnvelopeVersion envelopeVersion)
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("s", envelopeVersion == EnvelopeVersion.Soap11 ? Namespaces.S11Namespace : Namespaces.S12Namespace);
            namespaceManager.AddNamespace("wsse", Namespaces.Wsse10Namespace);
            namespaceManager.AddNamespace("wsu", Namespaces.WsuNamespace);
            namespaceManager.AddNamespace("wsa", Namespaces.WsaNamespace);
            namespaceManager.AddNamespace("wst", Namespaces.Wst13Namespace);
            return namespaceManager;
        }

        private static void VerifyRequiredElement(XDocument xDocument, List<string> refUries, string path, XmlNamespaceManager namespaceManager, string errorTemplate)
        {
            var element = xDocument.XPathSelectElement(path, namespaceManager);
            var id = $"#{element?.Attribute(XName.Get("Id", Namespaces.WsuNamespace))?.Value}";
            if (string.IsNullOrEmpty(id) || !refUries.Any(uri => uri == id))
            {
                Logger.Instance.Error(string.Format(errorTemplate, id));
                throw new MessageSecurityException(string.Format(errorTemplate, id));
            }
        }
    }
}
