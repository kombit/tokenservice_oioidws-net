using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.SoapCore.Bindings;
using Digst.OioIdws.SoapCore.Tokens;
using Digst.OioIdws.Common.TestUtils;
using Digst.OioIdws.OioWsTrustCore.Test;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Digst.OioIdws.CommonCore;
using System.Xml.Linq;
using System.Xml;
using System.ServiceModel.Security;
using Digst.OioIdws.OioWsTrustCore;

namespace Digst.OioIdws.SoapCore.Test
{
    [TestClass]
    public class OioIdwsSoapChannelTest
    {
        [TestMethod]
        public void Request_Will_ModifyMessageAccordingToWspNeeds_Before_Sending_Request()
        {
            // Arrange
            var sut = CreateSut();
            var messageXml = XDocument.Load(@"Data\MessageBeforeTransforming.xml");
            var message = Message.CreateMessage(MessageVersion.Soap11WSAddressingAugust2004, "Test");
            message = messageXml.ToMessage(message);
            var id = new UniqueId(Guid.Parse("0cbeecb3-8fe4-40c2-837d-db77ffce3801"));
            message.Headers.MessageId = id;
            message.Headers.RelatesTo = id;
            // Act

            var response = sut.Request(message); //TestRequestChannel only return the reponse loading from the WspResponse.xml file

            // Assert
            Assert.IsNotNull(response);

            var xDocument = response.ToXml();
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xDocument.ToString());
            var signatureNodeList = xmlDocument.GetElementsByTagName("Signature", Namespaces.SignatureNamespace);
            Assert.IsNotNull(signatureNodeList);
            Assert.AreEqual(2, signatureNodeList.Count); // One is in Header's Assertion
        }

        [TestMethod]
        public void BeginRequest_Will_ModifyMessageAccordingToWspNeeds_Before_Sending_Request()
        {
            // Arrange
            var sut = CreateSut();
            var messageXml = XDocument.Load(@"Data\MessageBeforeTransforming.xml");
            var message = Message.CreateMessage(MessageVersion.Soap11, "Test");
            message = messageXml.ToMessage(message);

            // Act

            var response = sut.BeginRequest(message, null, null) as Task<Message>; //TestRequestChannel only return the 'modified message' as a response

            // Assert
            Assert.IsNotNull(response);

            var xDocument = response.Result.ToXml();
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xDocument.ToString());
            var signatureNodeList = xmlDocument.GetElementsByTagName("Signature", Namespaces.SignatureNamespace);
            Assert.IsNotNull(signatureNodeList);
            Assert.AreEqual(2, signatureNodeList.Count); // One is in Header's Assertion
        }

        [TestMethod]
        public void EndRequest_Will_Failed_To_Validate_MessageId()
        {
            // Arrange
            var sut = CreateSut();
            
            var messageXml = XDocument.Load(@"Data\WspResponse.xml");
            var reply = Message.CreateMessage(MessageVersion.Soap12WSAddressingAugust2004, "Test");
            reply = messageXml.ToMessage(reply);

            Assert.ThrowsException<MessageSecurityException>(() => sut.EndRequest(Task<Message>.FromResult(reply)), "RelatesTo header block is required and its value MUST be set to the value of the < wsa:MessageID > header block of the prior-received message.");
        }

        [TestMethod]
        public void EndRequest_Will_Failed_Because_Of_MessageId_Missing()
        {
            // Arrange
            var sut = CreateSut();

            var messageXml = XDocument.Load(@"Data\WspResponse.xml");
            var reply = Message.CreateMessage(MessageVersion.Soap12WSAddressingAugust2004, "Test");
            reply = messageXml.ToMessage(reply);

            Func<object, Message> func = (state) =>
            {
                return reply;
            };
            var asyncResult = new Task<Message>(func, new UniqueId("uuid:0cbeecb3-8fe4-40c2-837d-db77ffce3801"));
            asyncResult.Start();
            Assert.ThrowsException<MessageSecurityException>(() => sut.EndRequest(asyncResult), "The <wsa:MessageID> header block MUST be included in the SOAP header.");
        }

        private static OioIdwsSoapChannel CreateSut()
        {
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.StsCertificate);

            var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, Certificates.TestCertificate);

            var bindingContext = new BindingContext(new CustomBinding(new List<BindingElement> { new HttpsTransportBindingElement() }), new BindingParameterCollection());
            var clientCredential = new FederatedChannelClientCredentials();
            clientCredential.ClientCertificate.Certificate = Certificates.TestCertificate;
            bindingContext.BindingParameters.Add(clientCredential);
            var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
                stsTokenServiceConfiguration, "https://test.service");

            var bindingElement = new OioIdwsSoapBindingElement(tokenParams);
            var result = bindingElement.BuildChannelFactory<IRequestChannel>(bindingContext);

            var sut = new OioIdwsSoapChannel(new OioIdwsSoapChannelFactory(result, tokenParams), new TestRequestChannel(), tokenParams);
            return sut;
        }
    }
}
