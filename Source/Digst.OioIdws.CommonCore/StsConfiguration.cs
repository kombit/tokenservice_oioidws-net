using System;
using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.CommonCore
{
    /// <summary>
    /// Contains all settings needed to call an STS
    /// </summary>
    public class StsConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StsConfiguration"/> class.
        /// </summary>
        public StsConfiguration(string stsEndpointAddress, string stsEntityIdentifier, string cvr, X509Certificate2 stsCertificate)
        {
            if (string.IsNullOrEmpty(stsEndpointAddress))
                throw new ArgumentNullException(nameof(stsEndpointAddress));
            if (string.IsNullOrEmpty(stsEntityIdentifier))
                throw new ArgumentNullException(nameof(stsEntityIdentifier));
            if (string.IsNullOrEmpty(cvr))
                throw new ArgumentNullException(nameof(cvr));

            EndpointAddress = stsEndpointAddress;
            EntityIdentifier = stsEntityIdentifier;
            Cvr = cvr;
            Certificate = stsCertificate ?? throw new ArgumentNullException(nameof(stsCertificate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StsConfiguration"/> class.
        /// </summary>
        public StsConfiguration(StsConfiguration other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            EndpointAddress = other.EndpointAddress;
            EntityIdentifier = other.EntityIdentifier;
            Cvr = other.Cvr;
            Certificate = other.Certificate;
            OboCertificate = other.OboCertificate;
        }

        /// <summary>
        /// Endpoint address of STS. E.g. https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed
        /// </summary>
        public string EndpointAddress { get; }

        /// <summary>
        /// Entity Identifier of STS. E.g. https://adgangsstyring.eksterntest-stoettesystemerne.dk/
        /// </summary>
        public string EntityIdentifier { get; }

        /// <summary>
        /// To get a token from KOMBIT STS, the requestor needs to specify a CVR
        ///     <trust:Claims Dialect="http://docs.oasis-open.org/wsfed/authorization/200706/authclaims" xmlns:auth="http://docs.oasis-open.org/wsfed/authorization/200706">
        ///       <auth:ClaimType Uri = "dk:gov:saml:attribute:CvrNumberIdentifier" Optional="false">
        ///          <auth:Value>12345678</auth:Value>
        ///          </auth:ClaimType>
        ///     </trust:Claims>
        /// 
        /// </summary>
        public string Cvr { get; }

        /// <summary>
        /// Represents the STS certificate containing only the public key. This should be a FOCES certificate.
        /// </summary>
        public X509Certificate2 Certificate { get; }

        /// <summary>
        /// Represents the onbehalfof certificate for the Proxy OBO request.
        /// This property is settable because the proxy obo certicate is only required when the Proxy OBO functionality is used
        /// </summary>
        public X509Certificate2? OboCertificate { get; set; }
    }
}
