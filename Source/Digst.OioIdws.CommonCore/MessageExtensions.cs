using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Digst.OioIdws.CommonCore
{
    /// <summary>
    /// Contains extension methods that can convert WCF's messages object from/to XmlDocument/XDocument
    /// as well as some Xml verification and manipulation
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Convert a SOAP <see cref="Message"/> to <see cref="XDocument"/>
        /// </summary>
        /// <param name="request"></param>
        public static XDocument ToXml(this Message request)
        {
            XDocument xDocument;
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlDictionaryWriter = CreateTextWriter(memoryStream))
                {
                    request.WriteMessage(xmlDictionaryWriter);
                    xmlDictionaryWriter.Flush();
                    memoryStream.Position = 0; // Needed in order for XDocument.Load to read stream from beginning.
                    xDocument = XDocument.Load(memoryStream);
                }
            }
            return xDocument;
        }

        /// <summary>
        /// Convert a SOAP <see cref="Message"/> to <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="request"></param>
        public static XmlDocument ToXmlDocument(this Message request)
        {
            XmlDocument doc;
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlDictionaryWriter = CreateTextWriter(memoryStream))
                {
                    request.WriteMessage(xmlDictionaryWriter);
                    xmlDictionaryWriter.Flush();
                    memoryStream.Position = 0; // Needed in order for XDocument.Load to read stream from beginning.

                    doc = new XmlDocument();
                    doc.Load(memoryStream);
                }
            }
            return doc;
        }

        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            // This conversion is not as effective as using XmlDocument.CreateReader().
            // However, using xDocument.CreateReader() makes the response signature from the STS invalid. The signature created by this class is not affected by using xDocument.CreateReader().
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xDocument.ToString());
            return xmlDocument;
        }

        /// <summary>
        /// Convert a XmlDocument to an XDocument. XDocument is easier for manipulation purposes, while XmlDocument is easier for signature signing/verification purpose
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                // The node reader is disposed but not before the XDocument has been loaded.
                return XDocument.Load(nodeReader);
            }
        }

        /// <summary>
        /// Convert a XElement to an XmlElement. Using ToString instead of CreateReader is the correct way to reserve namespaces that OIOIDWS requires
        /// </summary>
        /// <param name="xElement"></param>
        /// <returns></returns>
        public static XmlElement ToXmlElement(this XElement xElement)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xElement.ToString());
            return xmlDocument.DocumentElement;
        }

        /// <summary>
        /// Create a <see cref="Message"/> from a <see cref="XNode"/>.
        /// The SOAP version and properties are copied from the input <see cref="Message"/>.
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="request"></param>
        public static Message ToMessage(this XNode xNode, Message request)
        {
            return ToMessage(request, xmlWriter => xNode.WriteTo(xmlWriter));
        }

        /// <summary>
        /// Create a <see cref="Message"/> from a <see cref="XmlDocument"/>.
        /// The SOAP version and properties are copied from the input <see cref="Message"/>.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Message ToMessage(this XmlDocument doc, Message request)
        {
            return ToMessage(request, xmlWriter => doc.WriteTo(xmlWriter));
        }

        /// <summary>
        /// Remove an element from a <see cref="XDocument"/> if exists
        /// </summary>
        /// <param name="xDocument"></param>
        /// <param name="elementPath">the XPath to the element</param>
        /// <param name="namespaceManager"></param>
        public static void RemoveIfExist(this XDocument xDocument, string elementPath, XmlNamespaceManager namespaceManager)
        {
            var element = xDocument.XPathSelectElement(elementPath, namespaceManager);
            if (element != null)
            {
                element.Remove();
            }
        }

        /// <summary>
        /// Remove a child element from a <see cref="XDocument"/> if exists
        /// </summary>
        /// <param name="xElement"></param>
        /// <param name="childElementName">The child element name</param>
        /// <param name="ns">The child element namespace</param>
        public static void RemoveChildElement(this XElement xElement, string childElementName, string ns)
        {
            var element = xElement.Element(XName.Get(childElementName, ns));
            if (element != null)
            {
                element.Remove();
            }
        }

        private static Message ToMessage(Message request, Action<XmlWriter> action)
        {
            byte[] messageAsBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var xw = XmlWriter.Create(memoryStream, BuildXmlWriterSettings()))
                {
                    action(xw);
                }
                // No need to set position to 0 as ToArray does not use this property.
                messageAsBytes = memoryStream.ToArray();
            }

            var xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(messageAsBytes,
                XmlDictionaryReaderQuotas.Max);
            var newMessage = Message.CreateMessage(xmlDictionaryReader, int.MaxValue, request.Version);
            newMessage.Properties.CopyProperties(request.Properties);
            return newMessage;
        }

        private static XmlDictionaryWriter CreateTextWriter(Stream stream)
        {
            return XmlDictionaryWriter.CreateDictionaryWriter(XmlWriter.Create(stream, BuildXmlWriterSettings()));
        }

        private static XmlWriterSettings BuildXmlWriterSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.UTF8,
                CloseOutput = true,
                NewLineHandling = NewLineHandling.None
            };

            return settings;
        }
    }
}
