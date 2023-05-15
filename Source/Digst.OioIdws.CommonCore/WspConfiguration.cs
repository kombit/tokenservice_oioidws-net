using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace Digst.OioIdws.CommonCore
{
    public class WspConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WspConfiguration"/> class.
        /// </summary>
        public WspConfiguration(string wspEndpointAddress, string wspEndpointId, EnvelopeVersion wspSoapVersion, X509Certificate2 wspCertificate)
        {
            if (string.IsNullOrEmpty(wspEndpointAddress))
                throw new ArgumentNullException(nameof(wspEndpointAddress));
            if (string.IsNullOrEmpty(wspEndpointId))
                throw new ArgumentNullException(nameof(wspEndpointId));

            EndpointAddress = wspEndpointAddress;
            EndpointId = wspEndpointId;
            SoapVersion = wspSoapVersion ?? throw new ArgumentNullException(nameof(wspSoapVersion));
            ServiceCertificate = wspCertificate ?? throw new ArgumentNullException(nameof(wspCertificate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WspConfiguration"/> class.
        /// </summary>
        public WspConfiguration(WspConfiguration other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            EndpointId = other.EndpointId;
            EndpointAddress = other.EndpointAddress;
            SoapVersion = other.SoapVersion;
            ServiceCertificate = other.ServiceCertificate;
        }

        /// <summary>
        /// Endpoint ID of WSP. E.g. https://saml.nnit001.dmz.inttest
        /// </summary>
        public string EndpointId { get; }

        /// <summary>
        /// Endpoint of WSP. E.g. https://wsp.dk/service/query/2
        /// </summary>
        public string EndpointAddress { get; }

        /// <summary>
        /// The specific SOAP version to call the WSP.
        /// </summary>
        public EnvelopeVersion SoapVersion { get; }

        /// <summary>
        /// Represents the service certificate containing only the public key.
        /// </summary>
        public X509Certificate2 ServiceCertificate { get; }
    }
}
