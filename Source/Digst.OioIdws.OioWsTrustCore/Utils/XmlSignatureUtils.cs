using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel.Security;
using System.Xml;
using System.Xml.Linq;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.CommonCore.Logging;

namespace Digst.OioIdws.OioWsTrustCore.Utils
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
        public static XDocument SignDocument(XDocument xDoc, IEnumerable<string> ids, X509Certificate2 cert)
        {
            // Convert to XmlDocument as SignedXml only understands this type.
            var doc = xDoc.ToXmlDocument();

            // Apply private key, canonicalization method and signature method
            var signedXml = new SignedXmlWithIdResolvement(doc);
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
            signedXml.SignedInfo.SignatureMethod = Sha256SignatureAlgorithms.XmlDsigMoreRsaSha256Url;
            signedXml.SigningKey = cert.PrivateKey;

            // Make a reference for each element that must be signed.
            foreach (var id in ids)
            {
                var reference = new Reference("#" + id);
                reference.AddTransform(new XmlDsigExcC14NTransform());
                reference.DigestMethod = Sha256SignatureAlgorithms.XmlEncSha256Url;
                signedXml.AddReference(reference);
            }

            // Include a reference to the certificate
            signedXml.KeyInfo.AddClause(new KeyInfoNode(BuildSecurityTokenReferenceElement(doc)));

            signedXml.ComputeSignature();

            return ToXDocument(doc, signedXml);
        }

        /// <summary>
        /// Verify signature of a response
        /// </summary>
        /// <param name="xDocument">The Xml document to verify signature</param>
        /// <param name="cert">The public certificate to use to verify signature.</param>
        /// <returns>Return true if the signature is valid. Otherwise, return false</returns>
        /// <exception cref="MessageSecurityException"></exception>
        public static bool VerifySignature(XDocument xDocument, X509Certificate2 cert)
        {
            XmlDocument xmlDocument = xDocument.ToXmlDocument();

            var nodeList = xmlDocument.GetElementsByTagName("Signature", Namespaces.SignatureNamespace);
            if (nodeList.Count == 0)
            {
                Logger.Instance.Error($"Document does not contain a signature to verify: {xmlDocument.OuterXml}");
                throw new MessageSecurityException("Document does not contain a signature to verify.");
            }

            var signatureElement = nodeList.OfType<XmlElement>().FirstOrDefault(x => x.ParentNode.LocalName == "Security" &&
                x.ParentNode.ParentNode != null && x.ParentNode.ParentNode.LocalName == "Header");

            if (signatureElement == null)
            {
                Logger.Instance.Error($"Document does not contain a signature to verify: {xmlDocument.OuterXml}");
                throw new MessageSecurityException("Document does not contain a signature to verify.");
            }

            var signedXml = new SignedXmlWithIdResolvement(xmlDocument);
            signedXml.LoadXml(signatureElement);

            if (signedXml.CheckSignature(cert, true))
            {
                // Verify that all attributes/elements mandated by the specs is signed and it is at the correct position
                signatureElement.VerifyRequiredSignatureElements(xDocument);
                return true;
            }

            return false;
        }

        private static XmlElement BuildSecurityTokenReferenceElement(XmlDocument doc)
        {
            var referenceElement = doc.CreateElement(Namespaces.WssePrefix,
                "Reference",
                Namespaces.Wsse10Namespace);
            referenceElement.SetAttribute("URI", "#sec-binsectoken"); // Attribute must be in the empty namespace.
            referenceElement.SetAttribute("ValueType", Common.X509V3TokenProfile);
            var securityTokenReferenceElement = doc.CreateElement(Namespaces.WssePrefix, "SecurityTokenReference",
                Namespaces.Wsse10Namespace);
            securityTokenReferenceElement.AppendChild(referenceElement);

            return securityTokenReferenceElement;
        }

        private static XDocument ToXDocument(XmlDocument doc, SignedXmlWithIdResolvement signedXml)
        {
            // Append the computed signature. The signature must be placed as the sibling of the BinarySecurityToken element.
            var nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace(Namespaces.S12Prefix, Namespaces.S12Namespace);
            nsManager.AddNamespace(Namespaces.WssePrefix, Namespaces.Wsse10Namespace);
            var securityNode = doc.SelectSingleNode("/" + Namespaces.S12Prefix + ":Envelope/" + Namespaces.S12Prefix + ":Header/" + Namespaces.WssePrefix + ":Security", nsManager);
            var binarySecurityTokenNode = doc.SelectSingleNode("/" + Namespaces.S12Prefix + ":Envelope/" + Namespaces.S12Prefix + ":Header/" + Namespaces.WssePrefix + ":Security/" + Namespaces.WssePrefix + ":BinarySecurityToken", nsManager);
            securityNode.InsertAfter(doc.ImportNode(signedXml.GetXml(), true), binarySecurityTokenNode);

            return doc.ToXDocument();
        }
    }
}
