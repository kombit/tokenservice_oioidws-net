using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.SoapCore.Tokens;
using Digst.OioIdws.SoapCore.Utils;

namespace Digst.OioIdws.SoapCore.Bindings
{
    /// <summary>
    /// This class implements the signature case part of specification [OIO-IDWS-SOAP].
    /// It expects a standard SOAP 1.1 and 1.2 message and transforms it to a format that [OIO-IDWS-SOAP] compliant-services understand and vice versa.
    /// </summary>
    public class OioIdwsSoapMessageTransformer
    {
        private readonly FederatedSecurityTokenParameters _federatedSecurityTokenParameters;
        private readonly string sNamespace;

        /// <summary>
        /// Creates a new instance of OioIdwsSoapMessageTransformer
        /// </summary>
        /// <param name="clientCertificate">...</param>
        public OioIdwsSoapMessageTransformer(FederatedSecurityTokenParameters federatedSecurityTokenParameters)
        {
            _federatedSecurityTokenParameters = federatedSecurityTokenParameters ?? throw new ArgumentNullException(nameof(federatedSecurityTokenParameters));
            
            if (_federatedSecurityTokenParameters.MessageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                sNamespace = Namespaces.S11Namespace;
            }
            else
            {
                sNamespace = Namespaces.S12Namespace;
            }
        }

        /// <summary>
        /// Apply modifications to an input message to make it compliant to WSP's needs: adding required headers, apply correct signature
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Message ModifyMessageAccordingToWspNeeds(Message request)
        {
            // Convert Message into a XML document that can be manipulated
            var xDocument = request.ToXml();

            // Log request before being manipulated)
            Logger.Instance.Trace("Request send to a WSP before being manipulated:\n" + xDocument);

            // Manipulate XML
            ManipulateHeader(xDocument);
            xDocument = SignMessage(xDocument, _federatedSecurityTokenParameters.SecurityToken, _federatedSecurityTokenParameters.ClientCertificate);

            // Log RST after being manipulated
            Logger.Instance.Trace("RST send to WSP after being manipulated:\n" + xDocument);

            // Convert XML back to a Message
            return xDocument.ToMessage(request);
        }

        /// <summary>
        /// After a signature is verified to be valid, remove it form the security header to skip .NET's default validation which is not able to handle such signature
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Message ModifyMessageToSkipDefautValidation(Message response)
        {
            // Convert Message into a XML document that can be manipulated
            var xDocument = response.ToXml();

            // Log request before being manipulated)
            Logger.Instance.Trace("Response from WSP before being manipulated:\n" + xDocument);

            // Manipulate XML
            RemoveSignature(xDocument);

            // Log RST after being manipulated
            Logger.Instance.Trace("Response from WSP after being manipulated:\n" + xDocument);

            // Convert XML back to a Message
            response = xDocument.ToMessage(response);

            return response;
        }
        
        private XmlNamespaceManager CreateXmlNamespaceManager()
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("a", Namespaces.WsaNamespace);
            namespaceManager.AddNamespace("s", sNamespace);
            namespaceManager.AddNamespace("o", Namespaces.Wsse10Namespace);
            namespaceManager.AddNamespace("vs", Namespaces.VsDebuggerNamespace);
            namespaceManager.AddNamespace("vcf", Namespaces.WcfDiagnosticsNamespace);
            namespaceManager.AddNamespace("wst13", Namespaces.Wst13Namespace);
            namespaceManager.AddNamespace("wst14", Namespaces.Wst14Namespace);
            namespaceManager.AddNamespace("u", Namespaces.WsuNamespace);
            namespaceManager.AddNamespace("ds", Namespaces.SignatureNamespace);

            return namespaceManager;
        }

        private void ManipulateHeader(XDocument xDocument)
        {
            var namespaceManager = CreateXmlNamespaceManager();

            // The spec states that all header elements (also those not used by the WSP) must be included in the signature. Hence, we need to remove the debugger element.
            // Remove VS debugger element. It is only present when running in debug mode. So removing the element is just to make life easier for developers.
            xDocument.RemoveIfExist("/s:Envelope/s:Header/vs:VsDebuggerCausalityData", namespaceManager);

            // Remove Diagnostics tracing element. It is only present when WCF Diagnostics are enabled. So removing the element is just to make life easier for developers.
            xDocument.RemoveIfExist("/s:Envelope/s:Header/vcf:ActivityId", namespaceManager);

            var assertionElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/o:Security", namespaceManager).Elements().Skip(1).First();
            string assertionId = assertionElement.Attribute("ID").Value;
            var newKeyIdentifierElement = new XElement(XName.Get("KeyIdentifier", Namespaces.Wsse10Namespace));
            newKeyIdentifierElement.Add(new XAttribute("ValueType", Common.SamlValueType));
            newKeyIdentifierElement.Value = assertionId;

            var securityTokenReferenceElement = new XElement(XName.Get("SecurityTokenReference", Namespaces.Wsse10Namespace));
            securityTokenReferenceElement.Add(new XAttribute(XName.Get("TokenType", Namespaces.Wsse11Namespace), Common.OasisSaml2TokenType));
            securityTokenReferenceElement.Add(newKeyIdentifierElement);

            assertionElement.AddAfterSelf(securityTokenReferenceElement);

            var messageIdElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/a:MessageID", namespaceManager);

            var replyToElement = new XElement(XName.Get("ReplyTo", Namespaces.WsaNamespace));
            var replyToAddressElement = new XElement(XName.Get("Address", Namespaces.WsaNamespace));
            replyToAddressElement.SetValue("http://www.w3.org/2005/08/addressing/anonymous");
            replyToElement.AddFirst(replyToAddressElement);

            var toElement = new XElement(XName.Get("To", Namespaces.WsaNamespace));
            toElement.SetValue(_federatedSecurityTokenParameters.WspEndpoint);
            toElement.Add(new XAttribute(XName.Get("mustUnderstand", sNamespace), "1"));// TO element would have been added after this step by the inner channel, so we add it here manually and use "manualaddressing = true" to avoid it being added again later

            messageIdElement.AddAfterSelf(replyToElement);

            replyToElement.AddAfterSelf(toElement);
        }

        private void RemoveSignature(XDocument xDocument)
        {
            var namespaceManager = CreateXmlNamespaceManager();
            xDocument.RemoveIfExist("/s:Envelope/s:Header/o:Security/ds:Signature", namespaceManager);
            xDocument.RemoveIfExist("/s:Envelope/s:Header/o:Security/o:BinarySecurityToken", namespaceManager);
        }

        private XDocument SignMessage(XDocument xDocument, GenericXmlSecurityToken referenceSecurityToken, X509Certificate2 clientCertificate)
        {
            // Add id's to elements that needs to be signed.
            var namespaceManager = CreateXmlNamespaceManager();
            var actionElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/a:Action", namespaceManager);
            var msgElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/a:MessageID", namespaceManager);
            var toElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/a:To", namespaceManager);
            var replyToElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/a:ReplyTo", namespaceManager);
            var timeStampElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/o:Security/u:Timestamp", namespaceManager);
            var securityTokenReferenceElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/o:Security/o:SecurityTokenReference", namespaceManager);
            var bodyElement = xDocument.XPathSelectElement("/s:Envelope/s:Body", namespaceManager);

            var idXName = XName.Get("Id", Namespaces.WsuNamespace);
            actionElement.Add(new XAttribute(idXName, IdValue.ActionIdValue));
            msgElement.Add(new XAttribute(idXName, IdValue.MessageIdIdValue));
            toElement.Add(new XAttribute(idXName, IdValue.ToIdValue));
            replyToElement.Add(new XAttribute(idXName, IdValue.ReplyToIdValue));
            timeStampElement.RemoveAttributes();
            timeStampElement.Add(new XAttribute(idXName, IdValue.TimeStampIdValue));
            bodyElement.Add(new XAttribute(idXName, IdValue.BodyIdValue));
            securityTokenReferenceElement.Add(new XAttribute(idXName, IdValue.SecurityTokenReferenceElementIdValue));

            var idOfElementsThatMustBeSigned = new List<string>
                {
                    IdValue.BodyIdValue,
                    IdValue.ActionIdValue,
                    IdValue.MessageIdIdValue,
                    IdValue.ReplyToIdValue,
                    IdValue.ToIdValue,
                    IdValue.TimeStampIdValue,
                    IdValue.SecurityTokenReferenceElementIdValue
                };

            HandleLibertyFrameworkHeader(xDocument, namespaceManager, idXName, idOfElementsThatMustBeSigned);
            xDocument = RemoveExistingSignatureAndSign(xDocument, referenceSecurityToken, clientCertificate, namespaceManager, securityTokenReferenceElement, idOfElementsThatMustBeSigned);

            return xDocument;
        }

        private XDocument RemoveExistingSignatureAndSign(XDocument xDocument, GenericXmlSecurityToken referenceSecurityToken, X509Certificate2 clientCertificate, XmlNamespaceManager namespaceManager, XElement securityTokenReferenceElement, List<string> idOfElementsThatMustBeSigned)
        {
            // Remove existing Signature
            var securityElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/o:Security", namespaceManager);
            securityElement.RemoveChildElement("Signature", Namespaces.SignatureNamespace);

            xDocument = XmlSignatureUtils.SignDocument(xDocument, referenceSecurityToken, idOfElementsThatMustBeSigned, clientCertificate, securityTokenReferenceElement, _federatedSecurityTokenParameters.MessageVersion.Envelope);
            return xDocument;
        }

        private static void HandleLibertyFrameworkHeader(XDocument xDocument, XmlNamespaceManager namespaceManager, XName idXName, List<string> idOfElementsThatMustBeSigned)
        {
            var libertyHeaderElement = xDocument.XPathSelectElement("/s:Envelope/s:Header", namespaceManager).Elements().FirstOrDefault(e => e.Name.LocalName == "Framework");
            if (libertyHeaderElement != null)
            {
                libertyHeaderElement.Add(new XAttribute(idXName, IdValue.LibertyFrameworkIdValue));
                idOfElementsThatMustBeSigned.Add(IdValue.LibertyFrameworkIdValue);
            }
        }
    }
}