using Digst.OioIdws.CommonCore;
using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel.Federation;
using System.ServiceModel.Security.Tokens;

namespace Digst.OioIdws.SoapCore.Tokens
{
    /// <summary>
    /// A SecurityTokenParameters class that holds a security token issued by an STS
    /// The base class SecurityTokenParameters does not have an accessible constructor, so need to inherit from WSTrustTokenParameters
    /// Only a handful properties of the base class are used
    /// </summary>
    public class FederatedSecurityTokenParameters : WSTrustTokenParameters
    {
        public GenericXmlSecurityToken SecurityToken { get; }

        public MessageVersion MessageVersion { get; }

        public X509Certificate2 ClientCertificate { get; }

        public X509Certificate2 ServerCertificate { get; }

        public IStsTokenServiceConfiguration StsTokenServiceConfiguration { get; }

        public string WspEndpoint { get; }

        public int? MaxReceivedMessageSize { get; set; }

        public bool IncludeLibertyHeader { get; set; }

        public FederatedSecurityTokenParameters(GenericXmlSecurityToken securityToken, MessageVersion messageVersion, IStsTokenServiceConfiguration stsTokenServiceConfiguration, string wspEndpoint)
        {
            if (stsTokenServiceConfiguration == null)
            {
                throw new ArgumentNullException(nameof(stsTokenServiceConfiguration));
            }

            base.RequireDerivedKeys = false;
            SecurityToken = securityToken ?? throw new ArgumentNullException(nameof(securityToken));
            MessageVersion = messageVersion ?? throw new ArgumentNullException(nameof(messageVersion));
            StsTokenServiceConfiguration = stsTokenServiceConfiguration;
            ClientCertificate = stsTokenServiceConfiguration.ClientCertificate;
            ServerCertificate = stsTokenServiceConfiguration.WspConfiguration.ServiceCertificate;

            if (string.IsNullOrEmpty(wspEndpoint))
                throw new ArgumentNullException(nameof(wspEndpoint));

            WspEndpoint = wspEndpoint;
        }

        protected FederatedSecurityTokenParameters(FederatedSecurityTokenParameters other) : base(other)
        {
            base.RequireDerivedKeys = false;
            SecurityToken = other.SecurityToken;
            MessageVersion = other.MessageVersion;
            StsTokenServiceConfiguration = other.StsTokenServiceConfiguration;
            ClientCertificate = other.ClientCertificate;
            ServerCertificate = other.ServerCertificate;
            WspEndpoint = other.WspEndpoint;
            MaxReceivedMessageSize = other.MaxReceivedMessageSize;
            IncludeLibertyHeader = other.IncludeLibertyHeader;
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new FederatedSecurityTokenParameters(this);
        }
    }
}
