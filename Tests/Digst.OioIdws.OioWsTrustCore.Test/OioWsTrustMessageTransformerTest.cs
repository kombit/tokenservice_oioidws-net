using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.OioWsTrustCore.ProtocolChannel;
using Digst.OioIdws.Common.TestUtils;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class OioWsTrustMessageTransformerTest
    {
        [TestMethod]
        public void ModifyMessageAccordingToStsNeedsTest()
        {
            // Setup
            var xml = XDocument.Load("Data/Request.xml");
            var requestMessage = Message.CreateMessage(MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10), "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue");
            requestMessage = xml.ToMessage(requestMessage);

            var stsConfiguration = new StsConfiguration("https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed", "teststsid", "12345678", Certificates.TestCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var config = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

            var sut = new OioWsTrustMessageTransformer(config);

            // Act

            var result = sut.ModifyMessageAccordingToStsNeeds(requestMessage);

            // Assert
            var resultXml = result.ToXml();
            var resultXmlString = resultXml.ToString();

            // wsu namespace
            Assert.Contains("xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"", resultXmlString);

            // message id header
            Assert.Contains("<a:MessageID wsu:Id=\"msgid\">uuid", resultXmlString);

            // to header
            Assert.Contains("<a:To wsu:Id=\"to\"", resultXmlString);

            Assert.HasCount(4, result.Headers);
            Assert.AreEqual("Security", result.Headers[3].Name);

            Assert.DoesNotContain("<trust:Lifetime>", resultXmlString);
            Assert.DoesNotContain("<trust:Entropy>", resultXmlString);
            Assert.Contains("<trust:Issuer>", resultXmlString);
        }

        [TestMethod]
        public void ModifyMessageAccordingToWsTrust_ReplyAttack_Detect_Test()
        {
            // Setup
            var messageXml = XDocument.Load(@"Data\Response.xml");
            var reply = Message.CreateMessage(MessageVersion.Soap12WSAddressingAugust2004, "Test");
            reply = messageXml.ToMessage(reply);

            var stsConfiguration = new StsConfiguration("https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var config = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

            var signature = "NHPGrZNCF9TVErBfiYz3/eCrUuFsP0HHUx7ryT3KA/0ogUioOKqViYXjcTekmklA6ZJ9VRnqvVL1sDpwUyO/Y1vvJX0EJAt1O4pEQRudh96rlQaEfWy+A5iKHnWnbKeUBosber4FbTe51eHPa+szIllC7scNDYUBqWbPku9/2d0UgM9lsH4W6VaLCorHd7Uxr9qA7bNDkgHR4JtDlXbyx6etVc56U0056wnpZxoAy/a3YckXi3frsX/NJlsbKoYP08ZW2I/TWjzOE0lLn6VSTwyZDqm0K05qDusQHo8SY1PlVec3rajnIzSs0R9H8DeOwL7T3dJomb5ELTAmrgq7iA==";
            
            config.ReplayAttackCache.Set(signature, Array.Empty<byte>(), new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.Add(TimeSpan.FromMinutes(5))
            });

            var sut = new OioWsTrustMessageTransformer(config);

            // Act
            Assert.ThrowsExactly<MessageSecurityException>(() => sut.ValidateAndModifyMessageAccordingToWsTrust(reply, reply.Headers.RelatesTo), "Replay attack detected. Response of message id: uuid:febfb086-36a4-430c-888a-9dbd3ed55738");

            // Assert
        }

        [TestMethod]
        public void ModifyMessageAccordingToWsTrustTest_ExpirationValidation()
        {
            // Setup
            var xml = XDocument.Load("Data/Response.xml");
            var response = Message.CreateMessage(MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10), "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTRC/IssueFinal");
            response = xml.ToMessage(response);
            var certificate = Certificates.TestCertificate;
            
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var config = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

            var sut = new OioWsTrustMessageTransformer(config);

            // Act


            //Assert
            Assert.ThrowsExactly<MessageSecurityException>(() => sut.ValidateAndModifyMessageAccordingToWsTrust(response, new UniqueId()), "SOAP message has expired");
        }

        [TestMethod]
        public void ModifyMessageAccordingToWsTrustTest_SignatureError()
        {
            // Setup
            var xml = XDocument.Load("Data/Response.xml");

            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("s", Namespaces.S12Namespace);
            namespaceManager.AddNamespace("wsse", Namespaces.Wsse10Namespace);
            namespaceManager.AddNamespace("wsu", Namespaces.WsuNamespace);
            namespaceManager.AddNamespace("wst", Namespaces.Wst13Namespace);
            namespaceManager.AddNamespace("d", Namespaces.SignatureNamespace);
            namespaceManager.AddNamespace("wsa", Namespaces.WsaNamespace);

            var messageExpireTimeElement = xml.XPathSelectElement("/s:Envelope/s:Header/wsse:Security/wsu:Timestamp/wsu:Expires", namespaceManager);
            if (messageExpireTimeElement != null)
            {
                messageExpireTimeElement.Value = messageExpireTimeElement.Value.Replace("2022", "2023"); // <= break signature validation
            }

            var response = Message.CreateMessage(MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10), "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTRC/IssueFinal");
            response = xml.ToMessage(response);
            var certificate = Certificates.TestCertificate;
            
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);

            var config = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

            var sut = new OioWsTrustMessageTransformer(config);

            // Act


            //Assert
            Assert.ThrowsExactly<MessageSecurityException>(() => sut.ValidateAndModifyMessageAccordingToWsTrust(response, new UniqueId()), "SOAP signature received from STS does not validate");
        }
    }
}