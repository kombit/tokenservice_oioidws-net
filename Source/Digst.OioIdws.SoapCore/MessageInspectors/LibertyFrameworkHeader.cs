using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Digst.OioIdws.SoapCore.MessageInspectors
{
    /// <summary>
    /// A class that represents the liberty framework message header.
    /// </summary>
    [System.Serializable]
    public class LibertyFrameworkHeader : MessageHeader
    {
        public LibertyFrameworkHeader()
        {
            Name = LibBas.HeaderName;
            Namespace = LibBas.HeaderNameSpace;
            MustUnderstand = true; // Is set to true in the examples in [LIB-BAS]. However, not stated as either optional or mandatory.
        }

        public override bool MustUnderstand { get; }

        public override string Name { get; }

        public override string Namespace { get; }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteAttributeString(LibBas.ProfileName, LibBas.ProfileNameSpace, LibBas.ProfileValue);
            writer.WriteAttributeString(LibBas.VersionName, LibBas.VersionValue);
        }

        // Liberty Basic Soap Binding
        private static class LibBas
        {
            public const string HeaderName = "Framework";
            public const string HeaderNameSpace = "urn:liberty:sb:2006-08";
            public const string VersionName = "version";
            public const string VersionValue = "2.0";
            public const string ProfileName = "profile";
            public const string ProfileNameSpace = "urn:liberty:sb:profile";
            public const string ProfileValue = "urn:liberty:sb:profile:basic";
        }
    }
}
