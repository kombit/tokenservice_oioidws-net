using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Security.Cryptography.Xml;
using System;
using System.Linq;

namespace Digst.OioIdws.SoapCore.StrCustomization
{
    /// <summary>
    /// Implement the STR transform because it is not supported by .NET Core
    /// The STR transform is a complex one. This implementation only supports the exact case that the OioWstrust/OioIdws profiles need
    /// We know exactly what element of a request that an STR element needs to reference to and what algorithm to use, so we calculate it
    /// in advance. This class does nothing more than returning a valid STR XML element
    /// </summary>
    public class StrTransform : Transform
    {
        private readonly Type[] _inputTypes = { typeof(Stream), typeof(XmlDocument), typeof(XmlNodeList) };
        private readonly Type[] _outputTypes = { typeof(Stream) };
        private readonly byte[] _digestedValue;

        /// <summary>
        /// Initializes a StrTransform instance
        /// </summary>
        /// <param name="digestedValue">Digested value of the referenced Security Token Reference element</param>
        /// <param name="inclusiveNamespacesPrefixList"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public StrTransform(byte[] digestedValue, string inclusiveNamespacesPrefixList)
        {
            _digestedValue = digestedValue ?? throw new ArgumentNullException(nameof(digestedValue));

            InclusiveNamespacesPrefixList = inclusiveNamespacesPrefixList;
            Algorithm = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#STR-Transform";
        }

        public string InclusiveNamespacesPrefixList { get; private set; }

        public override Type[] InputTypes
        {
            get { return _inputTypes; }
        }

        public override Type[] OutputTypes
        {
            get { return _outputTypes; }
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
            // Do nothing
        }

        public override void LoadInput(object obj)
        {
            // Do nothing
        }

        protected override XmlNodeList GetInnerXml()
        {
            MemoryStream ms = new MemoryStream(); 
            XmlDictionaryWriter xmlDictionaryWriter = XmlDictionaryWriter.CreateTextWriter(Stream.Null, Encoding.UTF8, false);
            xmlDictionaryWriter.StartCanonicalization(ms, includeComments: false, null);

            new TransformationParameters().WriteTo(xmlDictionaryWriter);
            xmlDictionaryWriter.EndCanonicalization();
            xmlDictionaryWriter.Flush();

            var wrapperDocument = new XmlDocument();
            ms.Position = 0;
            wrapperDocument.Load(ms);
            return wrapperDocument.ChildNodes;
        }

        public override object GetOutput()
        {
            return new MemoryStream(this._digestedValue);
        }

        public override object GetOutput(Type type)
        {
            if (type != typeof(Stream) && !type.IsSubclassOf(typeof(Stream)))
                throw new ArgumentException("Incorrect input type of a Cryptography Xml Transform", nameof(type));

            return GetOutput();
        }

        public override byte[] GetDigestedOutput(HashAlgorithm hash)
        {
            return _digestedValue;
        }

        class TransformationParameters
        {
            public TransformationParameters()
            {
            }

            public void WriteTo(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement("o", "TransformationParameters", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");  //<wsse:TransformationParameters>
                writer.WriteStartElement("", "CanonicalizationMethod", "http://www.w3.org/2000/09/xmldsig#");
                writer.WriteStartAttribute("Algorithm", null);
                writer.WriteString("http://www.w3.org/2001/10/xml-exc-c14n#");
                writer.WriteEndAttribute();
                writer.WriteEndElement(); // CanonicalizationMethod 
                writer.WriteEndElement(); // TransformationParameters
            }
        }
    }
}