using Digst.OioIdws.CommonCore.Constants;
using Microsoft.IdentityModel.Protocols.WsFed;
using Microsoft.IdentityModel.Protocols.WsTrust;
using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;

namespace Digst.OioIdws.OioWsTrustCore
{
    /// <summary>
    /// This class is responsible for creating a WSTrustRequest object that has UseKey and OnBehalfOfSecurityToken token
    /// The alternative is to modify Xml message at OioWsTrustChannel.Request using OioWsTrustMessageTransformer, However, there is no SecurityTokenRequirement information on the OioWsTrustChannel.
    /// <see cref="WSTrustChannelSecurityTokenProvider"/> has been designed to work with the <see cref="WSFederationHttpBinding"/> to send a WsTrust message to obtain a SecurityToken from an STS. The SecurityToken is
    /// added as an IssuedToken on the outbound WCF message.
    /// </summary>
    public class OioWSTrustChannelSecurityTokenProvider : WSTrustChannelSecurityTokenProvider
    {
        /// <summary>
        /// Gets the <see cref="SecurityTokenRequirement"/>
        /// </summary>
        internal SecurityTokenRequirement SecurityTokenRequirement
        {
            get;
        }

        internal Microsoft.IdentityModel.Tokens.SecurityToken? OnBehalfOfSecurityToken
        {
            get;
        }

        internal X509SecurityToken? ProxyOnBehalfOfSecurityToken
        {
            get;
        }

        internal WSTrustTokenParameters WSTrustTokenParameters { get; }

        /// <summary>
        ///  The client credentials from BindingParameters passed by ChannelFactory
        /// </summary>
        public ClientCredentials? CustomClientCredentials { get; set; }

        public OioWSTrustChannelSecurityTokenProvider(SecurityTokenRequirement tokenRequirement) : base(tokenRequirement)
        {
            SecurityTokenRequirement = tokenRequirement ?? throw new ArgumentNullException(nameof(tokenRequirement));

            IssuedSecurityTokenParameters issuedSecurityTokenParameters = SecurityTokenRequirement.GetProperty<IssuedSecurityTokenParameters>(SecurityTokenRequirementConstants.IssuedSecurityTokenParametersProperty);
            var wsTrustTokenParameters = issuedSecurityTokenParameters as WSTrustTokenParameters;
            if (wsTrustTokenParameters == null)
            {
                throw new ArgumentException("issuedSecurityTokenParameters is of incorrect type", nameof(tokenRequirement));
            }

            WSTrustTokenParameters = wsTrustTokenParameters;

            if (SecurityTokenRequirement.TryGetProperty(SecurityTokenRequirementConstants.OnBehalfOfProperty, out Microsoft.IdentityModel.Tokens.SecurityToken securityToken))
            {
                OnBehalfOfSecurityToken = securityToken;
            }

            if (SecurityTokenRequirement.TryGetProperty(SecurityTokenRequirementConstants.ProxyOnBehalfOfProperty, out X509SecurityToken proxySecurityToken))
            {
                ProxyOnBehalfOfSecurityToken = proxySecurityToken;
            }
        }

        protected override WsTrustRequest CreateWsTrustRequest()
        {
            WsTrustRequest wstrustRequest = base.CreateWsTrustRequest();

            if (SecurityTokenRequirement.TryGetProperty(SecurityTokenRequirementConstants.IssuerProperty, out string stsEntityIdentifier)
                && !string.IsNullOrEmpty(stsEntityIdentifier))
            {
                wstrustRequest.Issuer = new Microsoft.IdentityModel.Protocols.WsAddressing.EndpointReference(stsEntityIdentifier);
            }

            // to force WCF to *not* set entropy to the wstrustRequest, there must be an instance of SecurityBindingElement created in OioWsTrustBinding
            // and set its EntropyKeyMode to Server (see WSTrustChannelSecurityTokenProvider.InitializeKeyEntropyMode)
            // but adding a SecurityBindingElement to the elements list in OioWsTrustBinding is very difficult
            // the easiest way is still to remove the Entropy element via XML manipulation

            wstrustRequest.KeySizeInBits = null;
            wstrustRequest.ComputedKeyAlgorithm = null;

            BuildCustomElements(wstrustRequest);
            
            return wstrustRequest;
        }

        /// <summary>
        /// This method is here for the sake of writing unit tests easily
        /// </summary>
        /// <param name="wstrustRequest"></param>
        public void BuildCustomElements(WsTrustRequest wstrustRequest)
        {
            if (wstrustRequest == null)
            {
                throw new ArgumentNullException(nameof(wstrustRequest));
            }

            if (OnBehalfOfSecurityToken != null)
            {
                wstrustRequest.OnBehalfOf = new SecurityTokenElement(OnBehalfOfSecurityToken);
            }
            else if(ProxyOnBehalfOfSecurityToken != null)
            {
                AddAdditionalXmlElement(wstrustRequest, "OnBehalfOf", ProxyOnBehalfOfSecurityToken.Certificate);
            }
            
            //if (SecurityTokenRequirement.TryGetProperty(SecurityTokenRequirementConstants.CvrProperty, out string cvr)) // TODO Vi skal ikke understøtte claims for nu. Så derfor er dette kommenteret ud. 
            //{
            //    wstrustRequest.Claims = new Claims("http://docs.oasis-open.org/wsfed/authorization/200706/authclaims",
            //        new List<ClaimType>
            //        {
            //            new ClaimType
            //            {
            //                Uri = "dk:gov:saml:attribute:CvrNumberIdentifier",
            //                IsOptional = false,
            //                Value = cvr
            //            }
            //        });
            //}

            if (SecurityTokenRequirement.TryGetProperty(SecurityTokenRequirementConstants.LifetimeProperty, out Lifetime lifetime))
            {
                wstrustRequest.Lifetime = lifetime;
            }

            BuildUseKeyElement(wstrustRequest);
        }

        private void BuildUseKeyElement(WsTrustRequest wstrustRequest)
        {
            if (CustomClientCredentials != null)
            {
                AddAdditionalXmlElement(wstrustRequest, "UseKey", CustomClientCredentials.ClientCertificate.Certificate);
            }
        }

        private void AddAdditionalXmlElement(WsTrustRequest wstrustRequest, string elementName, X509Certificate2 certificate)
        {
            StringBuilder sb = new StringBuilder();

            using (var useKeyWriter = XmlWriter.Create(sb))
            {
                var x = certificate.GetRSAPublicKey().ToXmlString(false);


                var xml = new XmlDocument();
                xml.LoadXml(x);
                var rsaKeyValue = xml.FirstChild;
                var modulus = rsaKeyValue.ChildNodes.Item(0);
                var exponent = rsaKeyValue.ChildNodes.Item(1);

                useKeyWriter.WriteStartElement("trust", elementName, Namespaces.Wst13Namespace);
                useKeyWriter.WriteStartElement("ds", "KeyInfo", Namespaces.SignatureNamespace);
                useKeyWriter.WriteStartElement("ds", "KeyValue", Namespaces.SignatureNamespace);
                useKeyWriter.WriteStartElement("ds", "RSAKeyValue", Namespaces.SignatureNamespace);
                useKeyWriter.WriteElementString("Modulus", Namespaces.SignatureNamespace, modulus.InnerText);
                useKeyWriter.WriteElementString("Exponent", Namespaces.SignatureNamespace, exponent.InnerText);

                useKeyWriter.WriteEndElement();
                useKeyWriter.WriteEndElement();
                useKeyWriter.WriteEndElement();
                useKeyWriter.WriteEndElement();
                useKeyWriter.Flush();
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sb.ToString());
            wstrustRequest.AdditionalXmlElements.Add(doc.DocumentElement);
        }
    }
}
