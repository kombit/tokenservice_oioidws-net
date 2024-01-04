using System.Security.Cryptography.Xml;
using System.Xml;
using Digst.OioIdws.CommonCore.Constants;

namespace Digst.OioIdws.CommonCore
{
    ///<summary>
    /// Needed in order to reference elements where the id attribute is prefixed with a namespace. E.g. wsu:Id
    ///</summary>
    public class SignedXmlWithIdResolvement : SignedXml
    {
        public SignedXmlWithIdResolvement(XmlDocument document) : base(document)
        {
        }

        /// <summary>
        /// Id attributes are prefixed with <see cref="Namespaces.WsuPrefix"/> and can not be resolved by the standard implementation. This implementation handles this.
        /// </summary>
        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            // Check to see if it's a standard ID reference
            var idElem = base.GetIdElement(document, idValue);

            if (idElem == null)
            {
                var nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace(Namespaces.WsuPrefix, Namespaces.WsuNamespace);

                idElem = document.SelectSingleNode("//*[@" + Namespaces.WsuPrefix + ":Id=\"" + idValue + "\"]", nsManager) as XmlElement;
            }

            if (idElem == null)
            {
                throw new XmlException($"The Id element '{idValue}' is not found.");
            }

            return idElem;
        }
    }
}