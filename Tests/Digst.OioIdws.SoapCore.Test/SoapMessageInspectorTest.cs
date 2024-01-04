using Digst.OioIdws.Common.TestUtils;
using System.ServiceModel.Channels;
using Digst.OioIdws.CommonCore;
using System.Xml.Linq;
using Digst.OioIdws.SoapCore.MessageInspectors;
using Digst.OioIdws.CommonCore.Constants;

namespace Digst.OioIdws.SoapCore.Test;

[TestClass]
[TestCategory(Constants.UnitTest)]
public class SoapMessageInspectorTest
{
    [TestMethod]
    public void AfterReceiveRequest_ValidateWsAddressingHeaders_Successful()
    {
        // Arrange
        var messageXml = XDocument.Load(@"Data\Soap12.xml");
        var message = Message.CreateMessage(MessageVersion.Soap12WSAddressingAugust2004, "Test");
        message = messageXml.ToMessage(message);

        // Act
        var inspector = new SoapMessageInspector(false);
        var result = inspector.AfterReceiveRequest(ref message, null, null);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void BeforeSendReply_Ensure_MessageID()
    {
        // Arrange
        var messageXml = XDocument.Load(@"Data\Soap12.xml");
        var message = Message.CreateMessage(MessageVersion.Soap12WSAddressingAugust2004, "Test");
        message = messageXml.ToMessage(message);

        // Act
        var inspector = new SoapMessageInspector(false);
        inspector.BeforeSendReply(ref message, null);

        // Assert
        var headers = message.Headers;
        Assert.IsTrue(headers.Any(x => x.Name == WsAddressing.WsAddressingMessageId));
    }

    [TestMethod]
    public void BeforeSendRequest_Ensure_MessageID_LibertyFrameworkHeader_And_Expect_Headers()
    {
        // Arrange
        var messageXml = XDocument.Load(@"Data\Soap12.xml");
        var message = Message.CreateMessage(MessageVersion.Soap12WSAddressingAugust2004, "Test");
        message = messageXml.ToMessage(message);

        // Act
        var inspector = new SoapMessageInspector(true);
        inspector.BeforeSendRequest(ref message, null);

        // Assert
        var headers = message.Headers;
        Assert.IsTrue(headers.Any(x => x.Name == WsAddressing.WsAddressingMessageId));
        Assert.IsTrue(headers.Any(x => x is LibertyFrameworkHeader));

        var httpRequestMessage = message.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
        Assert.IsNotNull(httpRequestMessage);
        Assert.AreEqual("100-continue", httpRequestMessage.Headers["Expect"]);
    }

    [TestMethod]
    public void AfterReceiveReply_ValidateWsAddressingHeadersReceivedFromWsp()
    {
        // Arrange
        var messageXml = XDocument.Load(@"Data\WspResponse.xml");
        var reply = Message.CreateMessage(MessageVersion.Soap12WSAddressingAugust2004, "Test");
        reply = messageXml.ToMessage(reply);

        // Act
        var inspector = new SoapMessageInspector(false);
        inspector.AfterReceiveReply(ref reply, null);

        // Assert
        // The WsAddressingHeaders is validated
        Assert.IsTrue(reply.Headers.Any(
                    x =>
                        WsAddressing.WsAddressingRelatesTo == x.Name));
    }
}
