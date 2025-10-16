using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Digst.OioIdws.Common.TestUtils;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class MessageExtensionsTest
    {
        [TestMethod]
        public void ToXml_WithValidMessage_ReturnsXDocument()
        {
            // Arrange
            var message = Message.CreateMessage(MessageVersion.Soap11, "test");

            // Act
            var result = message.ToXml();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<XDocument>(result);
            Assert.AreEqual("Envelope", result.Root?.Name.LocalName);
        }

        [TestMethod]
        public void ToXml_ThrowsWhenMessageIsNull()
        {
            // Arrange
            Message? message = null;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => message!.ToXml());
        }

        [TestMethod]
        public void ToXmlDocument_WithValidMessage_ReturnsXmlDocument()
        {
            // Arrange
            var message = Message.CreateMessage(MessageVersion.Soap11, "test");

            // Act
            var result = message.ToXmlDocument();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<XmlDocument>(result);
            Assert.AreEqual("Envelope", result.DocumentElement?.LocalName);
        }

        [TestMethod]
        public void ToXmlDocument_ThrowsWhenMessageIsNull()
        {
            // Arrange
            Message? message = null;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => message!.ToXmlDocument());
        }

        [TestMethod]
        public void ToXmlDocument_WithValidXDocument_ReturnsXmlDocument()
        {
            // Arrange
            var xdoc = XDocument.Parse("<root><child>test</child></root>");

            // Act
            var result = xdoc.ToXmlDocument();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<XmlDocument>(result);
            Assert.AreEqual("root", result.DocumentElement?.LocalName);
            Assert.AreEqual("test", result.DocumentElement?.FirstChild?.InnerText);
        }

        [TestMethod]
        public void ToXmlDocument_ThrowsWhenXDocumentIsNull()
        {
            // Arrange
            XDocument? xdoc = null;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xdoc!.ToXmlDocument());
        }

        [TestMethod]
        public void ToXDocument_WithValidXmlDocument_ReturnsXDocument()
        {
            // Arrange
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<root><child>test</child></root>");

            // Act
            var result = xmlDoc.ToXDocument();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<XDocument>(result);
            Assert.AreEqual("root", result.Root?.Name.LocalName);
            Assert.AreEqual("test", result.Root?.Element("child")?.Value);
        }

        [TestMethod]
        public void ToXDocument_ThrowsWhenXmlDocumentIsNull()
        {
            // Arrange
            XmlDocument? xmlDoc = null;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xmlDoc!.ToXDocument());
        }

        [TestMethod]
        public void ToXmlElement_WithValidXElement_ReturnsXmlElement()
        {
            // Arrange
            var xElement = XElement.Parse("<root xmlns='urn:test'><child>test</child></root>");

            // Act
            var result = xElement.ToXmlElement();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<XmlElement>(result);
            Assert.AreEqual("root", result.LocalName);
            Assert.AreEqual("urn:test", result.NamespaceURI);
            Assert.AreEqual("test", result.FirstChild?.InnerText);
        }

        [TestMethod]
        public void ToXmlElement_ThrowsWhenXElementIsNull()
        {
            // Arrange
            XElement? xElement = null;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xElement!.ToXmlElement());
        }

        [TestMethod]
        public void RemoveIfExist_RemovesElement_WhenExists()
        {
            // Arrange
            var xdoc = XDocument.Parse("<root><child>1</child><other>2</other></root>");
            var nsMgr = new XmlNamespaceManager(new NameTable());

            // Act
            xdoc.RemoveIfExist("/root/child", nsMgr);

            // Assert
            Assert.IsNull(xdoc.Root?.Element("child"));
            Assert.IsNotNull(xdoc.Root?.Element("other"));
        }

        [TestMethod]
        public void RemoveIfExist_DoesNothing_WhenElementDoesNotExist()
        {
            // Arrange
            var originalXml = "<root><child>1</child></root>";
            var xdoc = XDocument.Parse(originalXml);
            var nsMgr = new XmlNamespaceManager(new NameTable());

            // Act
            xdoc.RemoveIfExist("/root/nonexistent", nsMgr);

            // Assert
            Assert.IsNotNull(xdoc.Root?.Element("child"));
            Assert.AreEqual(originalXml, xdoc.ToString(SaveOptions.DisableFormatting));
        }

        [TestMethod]
        public void RemoveIfExist_WorksWithNamespaces()
        {
            // Arrange
            var xdoc = XDocument.Parse("<root xmlns:ns='urn:test'><ns:child>1</ns:child><other>2</other></root>");
            var nsMgr = new XmlNamespaceManager(new NameTable());
            nsMgr.AddNamespace("ns", "urn:test");

            // Act
            xdoc.RemoveIfExist("/root/ns:child", nsMgr);

            // Assert
            Assert.IsNull(xdoc.XPathSelectElement("/root/ns:child", nsMgr));
            Assert.IsNotNull(xdoc.Root?.Element("other"));
        }

        [TestMethod]
        public void RemoveIfExist_ThrowsWhenXDocumentIsNull()
        {
            // Arrange
            XDocument? xdoc = null;
            var nsMgr = new XmlNamespaceManager(new NameTable());

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xdoc!.RemoveIfExist("/root/child", nsMgr));
        }

        [TestMethod]
        public void RemoveIfExist_ThrowsWhenElementPathIsNull()
        {
            // Arrange
            var xdoc = XDocument.Parse("<root><child /></root>");
            var nsMgr = new XmlNamespaceManager(new NameTable());

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xdoc.RemoveIfExist(null!, nsMgr));
        }

        [TestMethod]
        public void RemoveIfExist_ThrowsWhenNamespaceManagerIsNull()
        {
            // Arrange
            var xdoc = XDocument.Parse("<root><child /></root>");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xdoc.RemoveIfExist("/root/child", null!));
        }

        [TestMethod]
        public void RemoveChildElement_RemovesChild_WhenExists()
        {
            // Arrange
            var xElement = XElement.Parse("<root xmlns='urn:test'><child>1</child><other>2</other></root>");

            // Act
            xElement.RemoveChildElement("child", "urn:test");

            // Assert
            Assert.IsNull(xElement.Element(XName.Get("child", "urn:test")));
            Assert.IsNotNull(xElement.Element(XName.Get("other", "urn:test")));
        }

        [TestMethod]
        public void RemoveChildElement_DoesNothing_WhenChildDoesNotExist()
        {
            // Arrange
            var xElement = XElement.Parse("<root xmlns='urn:test'><child>1</child></root>");
            var originalValue = xElement.ToString();

            // Act
            xElement.RemoveChildElement("nonexistent", "urn:test");

            // Assert
            Assert.IsNotNull(xElement.Element(XName.Get("child", "urn:test")));
            Assert.AreEqual(originalValue, xElement.ToString());
        }

        [TestMethod]
        public void RemoveChildElement_WorksWithDifferentNamespaces()
        {
            // Arrange
            var xElement = XElement.Parse("<root xmlns:ns1='urn:test1' xmlns:ns2='urn:test2'><ns1:child>1</ns1:child><ns2:child>2</ns2:child></root>");

            // Act
            xElement.RemoveChildElement("child", "urn:test1");

            // Assert
            Assert.IsNull(xElement.Element(XName.Get("child", "urn:test1")));
            Assert.IsNotNull(xElement.Element(XName.Get("child", "urn:test2")));
        }

        [TestMethod]
        public void RemoveChildElement_ThrowsWhenChildElementNameIsNull()
        {
            // Arrange
            var xElement = XElement.Parse("<root xmlns='urn:test'><child>1</child></root>");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xElement.RemoveChildElement(null!, "urn:test"));
        }

        [TestMethod]
        public void RemoveChildElement_ThrowsWhenNamespaceIsNull()
        {
            // Arrange
            var xElement = XElement.Parse("<root xmlns='urn:test'><child>1</child></root>");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xElement.RemoveChildElement("child", null!));
        }

        [TestMethod]
        public void ToMessage_WithValidXDocument_ReturnsMessage()
        {
            // Arrange
            var xml = XDocument.Parse(@"
                <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                    <soap:Header></soap:Header>
                    <soap:Body>
                        <TestRequest>
                            <TestData>Sample Data</TestData>
                        </TestRequest>
                    </soap:Body>
                </soap:Envelope>");
            var originalMessage = Message.CreateMessage(MessageVersion.Soap11, "test");

            // Act
            var result = xml.ToMessage(originalMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<Message>(result);
        }

        [TestMethod]
        public void ToMessage_WithValidXmlDocument_ReturnsMessage()
        {
            // Arrange
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(@"
                <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                    <soap:Header></soap:Header>
                    <soap:Body>
                        <TestRequest>
                            <TestData>Sample Data</TestData>
                        </TestRequest>
                    </soap:Body>
                </soap:Envelope>");
            var originalMessage = Message.CreateMessage(MessageVersion.Soap11, "test");

            // Act
            var result = xmlDoc.ToMessage(originalMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<Message>(result);
        }

        [TestMethod]
        public void ToMessage_ThrowsWhenXDocumentIsNull()
        {
            // Arrange
            XDocument? xml = null;
            var message = Message.CreateMessage(MessageVersion.Soap11, "test");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xml!.ToMessage(message));
        }

        [TestMethod]
        public void ToMessage_ThrowsWhenXmlDocumentIsNull()
        {
            // Arrange
            XmlDocument? xmlDoc = null;
            var message = Message.CreateMessage(MessageVersion.Soap11, "test");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xmlDoc!.ToMessage(message));
        }

        [TestMethod]
        public void ToMessage_ThrowsWhenOriginalMessageIsNull()
        {
            // Arrange
            var xml = XDocument.Parse(@"
                <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                    <soap:Header></soap:Header>
                    <soap:Body></soap:Body>
                </soap:Envelope>");

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => xml.ToMessage(null!));
        }

        [TestMethod]
        public void ToXml_ToMessage_RoundTrip_PreservesContent()
        {
            // Arrange
            var originalXml = XDocument.Parse(@"
                <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                    <soap:Header></soap:Header>
                    <soap:Body>
                        <TestRequest>
                            <TestData>Sample Data</TestData>
                        </TestRequest>
                    </soap:Body>
                </soap:Envelope>");

            var message = originalXml.ToMessage(Message.CreateMessage(MessageVersion.Soap11, "test"));

            // Act
            var resultXml = message.ToXml();

            // Assert
            Assert.IsNotNull(resultXml);
            Assert.Contains("TestRequest", resultXml.ToString());
            Assert.Contains("Sample Data", resultXml.ToString());
        }

        [TestMethod]
        public void ToXmlDocument_ToXDocument_RoundTrip_PreservesContent()
        {
            // Arrange
            var originalXml = "<root xmlns='urn:test'><child>Sample Data</child></root>";
            var xdoc = XDocument.Parse(originalXml);

            // Act
            var xmlDoc = xdoc.ToXmlDocument();
            var resultXDoc = xmlDoc.ToXDocument();

            // Assert
            Assert.IsNotNull(resultXDoc);
            Assert.AreEqual("root", resultXDoc.Root?.Name.LocalName);
            Assert.AreEqual("urn:test", resultXDoc.Root?.Name.NamespaceName);
            Assert.AreEqual("Sample Data", resultXDoc.Root?.Element(XName.Get("child", "urn:test"))?.Value);
        }

        [TestMethod]
        public void ToXmlElement_PreservesNamespaces()
        {
            // Arrange
            var xElement = XElement.Parse(@"
                <root xmlns='urn:default' xmlns:ns1='urn:test1'>
                    <child>value1</child>
                    <ns1:child>value2</ns1:child>
                </root>");

            // Act
            var result = xElement.ToXmlElement();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("root", result.LocalName);
            Assert.AreEqual("urn:default", result.NamespaceURI);
            Assert.AreEqual(2, result.ChildNodes.Count);
        }
    }
}