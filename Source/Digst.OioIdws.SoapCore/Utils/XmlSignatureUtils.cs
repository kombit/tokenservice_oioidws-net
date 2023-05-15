using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.SoapCore.StrCustomization;

namespace Digst.OioIdws.SoapCore.Utils
{
    /// <summary>
    /// Utility methods involve XML processing
    /// </summary>
    public static class XmlSignatureUtils
    {
        /// <summary>
        /// Signs an XmlDocument with an xml signature using the signing certificate given as argument to the method.
        /// In case it is ever needed ... this link explains how to add namespace prefixes to the generated signature. http://stackoverflow.com/questions/12219232/xml-signature-ds-prefix
        /// </summary>
        /// <param name="xDoc">The XDocument to be signed</param>
        /// <param name="ids">The ids of the elements in the xmldocument that must be signed.</param>
        /// <param name="cert">The certificate used to sign the document</param>
        public static XDocument SignDocument(XDocument xDoc, GenericXmlSecurityToken referenceSecurityToken, 
            IEnumerable<string> ids, X509Certificate2 cert, XElement outerSecurityTokenReferenceElement,
            EnvelopeVersion envelopeVersion)
        {
            var sPrefix = envelopeVersion == EnvelopeVersion.Soap11 ? Namespaces.S11Prefix : Namespaces.S12Prefix;
            var sNamespace = envelopeVersion == EnvelopeVersion.Soap11 ? Namespaces.S11Namespace : Namespaces.S12Namespace;
            var securityNodePath = $"/{sPrefix}:Envelope/{sPrefix}:Header/o:Security";
            var securityTokenReferenceNodePath = $"{securityNodePath}/o:SecurityTokenReference";

            // Convert to XmlDocument as SignedXml only understands this type.
            var doc = xDoc.ToXmlDocument();
            SignedXmlWithIdResolvement signedXml = ComputeSignature(referenceSecurityToken, ids, cert, outerSecurityTokenReferenceElement, doc);

            // Append the computed signature. The signature must be placed as the sibling of the BinarySecurityToken element.
            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace(sPrefix, sNamespace);
            nsManager.AddNamespace("o", Namespaces.Wsse10Namespace);
            var securityNode = doc.SelectSingleNode(securityNodePath, nsManager);
            var securityTokenReferenceNode = doc.SelectSingleNode(securityTokenReferenceNodePath, nsManager);
            securityNode.InsertAfter(doc.ImportNode(signedXml.GetXml(), true), securityTokenReferenceNode);

            var signedDocument = doc.ToXDocument();

            return signedDocument;
        }

        private static SignedXmlWithIdResolvement ComputeSignature(GenericXmlSecurityToken referenceSecurityToken, IEnumerable<string> ids, X509Certificate2 cert, XElement outerSecurityTokenReferenceElement, XmlDocument doc)
        {
            // Apply private key, canonicalization method and signature method
            var signedXml = new SignedXmlWithIdResolvement(doc);
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = Sha256SignatureAlgorithms.XmlDsigMoreRsaSha256Url;
            signedXml.SigningKey = cert.PrivateKey;

            // Make a reference for each element that must be signed.
            foreach (var id in ids.Where(nodeId => nodeId != IdValue.SecurityTokenReferenceElementIdValue))
            {
                var reference = new Reference("#" + id);
                reference.AddTransform(new XmlDsigExcC14NTransform());
                reference.DigestMethod = Sha256SignatureAlgorithms.XmlEncSha256Url;
                signedXml.AddReference(reference);
            }

            AddStrTransform(referenceSecurityToken, signedXml);

            // Include a reference to the certificate
            var securityTokenReferenceElement = new XElement(outerSecurityTokenReferenceElement);
            var idAttribute = securityTokenReferenceElement.Attributes().First(attr => attr.Name.LocalName == "Id");
            idAttribute.Remove();

            signedXml.KeyInfo.AddClause(new KeyInfoNode(securityTokenReferenceElement.ToXmlElement()));

            signedXml.ComputeSignature();
            return signedXml;
        }

        /// <summary>
        /// Verify signature of a response
        /// </summary>
        /// <param name="xmlDocument">The Xml document to verify signature</param>
        /// <param name="cert">The public certificate to use to verify signature.</param>
        /// <param name="signaturePosition">There can be multiple signatures. We mostly care about the first signature found in the Security header</param>
        /// <param name="envelopeVersion">Whether the envelope is SOAP 1.1 or 1.2</param>
        /// <returns>Return true if the signature is valid. Otherwise, return false</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool VerifySignature(XmlDocument xmlDocument, X509Certificate2 cert, int signaturePosition, EnvelopeVersion envelopeVersion)
        {
            var signedXml = new SignedXmlWithIdResolvement(xmlDocument);
            var nodeList = xmlDocument.GetElementsByTagName("Signature", Namespaces.SignatureNamespace);
            if (nodeList.Count == 0)
            {
                Logger.Instance.Error($"Document does not contain a signature to verify: {xmlDocument.OuterXml}");
                throw new InvalidOperationException("Document does not contain a signature to verify.");
            }

            // Signature must be placed at the correct position
            var signatureElements = nodeList.OfType<XmlElement>().Where(x => x.ParentNode.LocalName == "Security" &&
                x.ParentNode.ParentNode != null && x.ParentNode.ParentNode.LocalName == "Header").ToArray();
            XmlElement signatureElement = signatureElements[signaturePosition];
            signedXml.LoadXml(signatureElement);

            if (signedXml.CheckSignature(cert, true))
            {
                signatureElement.VerifyRequiredSignatureElements(xmlDocument.ToXDocument(), envelopeVersion);
                return true;
            }

            return false;
        }

        private static void AddStrTransform(GenericXmlSecurityToken referenceSecurityToken, SignedXmlWithIdResolvement signedXml)
        {
            MemoryStream ms = new MemoryStream();
            XmlDictionaryWriter xmlDictionaryWriter = XmlDictionaryWriter.CreateTextWriter(Stream.Null, Encoding.UTF8, false);
            xmlDictionaryWriter.StartCanonicalization(ms, includeComments: false, null);
            referenceSecurityToken.TokenXml.WriteTo(xmlDictionaryWriter);
            xmlDictionaryWriter.EndCanonicalization();

            var hash = ms.ToArray();    // get the exact array without extra 0 value at the end
            var strReference = new Reference("#" + IdValue.SecurityTokenReferenceElementIdValue);
            strReference.AddTransform(new StrTransform(hash, string.Empty));
            strReference.DigestMethod = Sha256SignatureAlgorithms.XmlEncSha256Url;
            signedXml.AddReference(strReference);
        }
    }
}
