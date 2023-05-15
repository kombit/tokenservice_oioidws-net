using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.SoapCore.Utils;
using Digst.OioIdws.Common.TestUtils;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Digst.OioIdws.SoapCore.Test
{
    [TestClass]
    public class SoapSignatureTest
    {
        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void Soap11_SignatureTest()
        {
            // Arrange
            var nsm = CreateXmlNamespaceManager(true);
            var rtsSoapMessageNotSigned = XDocument.Load(@"Data\Soap11.xml");
            var idOfElementsThatMustBeSigned = new List<string>
                {
                    IdValue.BodyIdValue,
                    IdValue.ActionIdValue,
                    IdValue.MessageIdIdValue,
                    IdValue.ReplyToIdValue,
                    IdValue.LibertyFrameworkIdValue,
                    IdValue.ToIdValue,
                    IdValue.TimeStampIdValue,
                    IdValue.SecurityTokenReferenceElementIdValue
                };
            var token = new XmlDocument();
            token.Load(@"Data\securityToken.xml");
            var cert = Certificates.TestCertificate;
            var envelopVersion = EnvelopeVersion.Soap11;
            GenericXmlSecurityToken referenceSecurityToken = new GenericXmlSecurityToken(token.DocumentElement, null, DateTime.UtcNow, DateTime.Now, null, null, null);
            var securityTokenReferenceElement = rtsSoapMessageNotSigned.XPathSelectElement("/s:Envelope/s:Header/o:Security/o:SecurityTokenReference", nsm);

            // Act
            var rtsSoapMessageSigned = XmlSignatureUtils.SignDocument(rtsSoapMessageNotSigned, referenceSecurityToken,
                idOfElementsThatMustBeSigned, cert, securityTokenReferenceElement, envelopVersion);

            // Assert
            Assert.IsNotNull(rtsSoapMessageSigned);

            var signedSecurityElement = rtsSoapMessageSigned.Document?.XPathSelectElement("/s:Envelope/s:Header/o:Security", nsm);
            Assert.IsNotNull(signedSecurityElement);
            var signatureElement = signedSecurityElement
                .Descendants().First(x=>x.Name.LocalName == "Signature");
            Assert.IsNotNull(signatureElement);

            var references = signatureElement.Descendants().Where(x => x.Name.LocalName == "Reference");
            Assert.IsTrue(references.Any());

            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#body"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#action"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#msgid"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#replyto"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#lib"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#to"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#sec-ts"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#sec-str"));
        }

        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void Soap12_SignatureTest()
        {
            // Arrange
            var nsm = CreateXmlNamespaceManager(false);
            var rtsSoapMessageNotSigned = XDocument.Load(@"Data\Soap12.xml");
            var idOfElementsThatMustBeSigned = new List<string>
                {
                    IdValue.BodyIdValue,
                    IdValue.ActionIdValue,
                    IdValue.MessageIdIdValue,
                    IdValue.ReplyToIdValue,
                    IdValue.LibertyFrameworkIdValue,
                    IdValue.ToIdValue,
                    IdValue.TimeStampIdValue,
                    IdValue.SecurityTokenReferenceElementIdValue
                };
            var token = new XmlDocument();
            token.Load(@"Data\securityToken.xml");
            var cert = Certificates.TestCertificate;
            var envelopVersion = EnvelopeVersion.Soap12;
            GenericXmlSecurityToken referenceSecurityToken = new GenericXmlSecurityToken(token.DocumentElement, null, DateTime.UtcNow, DateTime.Now, null, null, null);
            var securityTokenReferenceElement = rtsSoapMessageNotSigned.XPathSelectElement("/s:Envelope/s:Header/o:Security/o:SecurityTokenReference", nsm);

            // Act
            var rtsSoapMessageSigned = XmlSignatureUtils.SignDocument(rtsSoapMessageNotSigned, referenceSecurityToken,
                idOfElementsThatMustBeSigned, cert, securityTokenReferenceElement, envelopVersion);

            // Assert
            Assert.IsNotNull(rtsSoapMessageSigned);

            var signedSecurityElement = rtsSoapMessageSigned.Document?.XPathSelectElement("/s:Envelope/s:Header/o:Security", nsm);
            Assert.IsNotNull(signedSecurityElement);
            var signatureElement = signedSecurityElement
                .Descendants().First(x => x.Name.LocalName == "Signature" && x.Parent?.Name.LocalName != "Assertion");
            Assert.IsNotNull(signatureElement);

            var references = signatureElement.Descendants().Where(x => x.Name.LocalName == "Reference");
            Assert.IsTrue(references.Any());

            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#body"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#action"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#msgid"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#replyto"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#lib"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#to"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#sec-ts"));
            Assert.IsTrue(references.Any(x => x.Attribute("URI")?.Value == "#sec-str"));
        }

        private static XmlNamespaceManager CreateXmlNamespaceManager(bool isSoap11)
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("a", Namespaces.WsaNamespace);
            namespaceManager.AddNamespace("s", isSoap11 ? Namespaces.S11Namespace : Namespaces.S12Namespace);
            namespaceManager.AddNamespace("o", Namespaces.Wsse10Namespace);
            namespaceManager.AddNamespace("vs", Namespaces.VsDebuggerNamespace);
            namespaceManager.AddNamespace("vcf", Namespaces.WcfDiagnosticsNamespace);
            namespaceManager.AddNamespace("wst13", Namespaces.Wst13Namespace);
            namespaceManager.AddNamespace("wst14", Namespaces.Wst14Namespace);
            namespaceManager.AddNamespace("u", Namespaces.WsuNamespace);
            namespaceManager.AddNamespace("ds", Namespaces.SignatureNamespace);

            return namespaceManager;
        }
    }
}
