using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Text;

namespace Digst.OioIdws.CommonCore
{
    /// <summary>
    /// Defines all necessary settings that a client application needs to call an STS
    /// </summary>
    public interface IStsTokenServiceConfiguration
    {
        /// <summary>
        /// Represents the client certificate including the private key. This should be either a MOCES, FOCES or VOCES certificate.
        /// </summary>
        X509Certificate2 ClientCertificate { get; }

        /// <summary>
        /// Configuration containing all necessary information to request an STS service to obtain a token for a Wsp endpoint calling.
        /// </summary>
        public StsConfiguration StsConfiguration { get; }

        /// <summary>
        /// Configuration containing all Wsp related information such as the Wsp endpoint Id, address, supported SOAP version and service certificate.
        /// </summary>
        public WspConfiguration WspConfiguration { get; }

        /// <summary>
        /// Specifies max size of message received in bytes. If not set, default value on <see cref="TransportBindingElement.MaxReceivedMessageSize"/> are used.
        /// </summary>
        int? MaxReceivedMessageSize { get; set; }

        /// <summary>
        /// Specify that if the SOAP's header include a liberty Framework header
        /// </summary>
        bool IncludeLibertyHeader { get; set; }

        /// <summary>
        /// Default in-memory replay attack cache
        /// </summary>
        IDistributedCache ReplayAttackCache { get; set; }

        /// <summary>
        /// Configures an instance of <see cref="X509ServiceCertificateAuthentication"/> to validate the STS' certificate(s).
        /// The default value is X509CertificateValidatorFactory.ChainTrust
        ///     which does online revocation check using entire certificate chain validation.
        /// To customize certificate validation, you can make your own X509CertificateValidator and set it to this property
        /// </summary>
        X509ServiceCertificateAuthentication StsCertificateAuthentication { get; }

        /// <summary>
        /// Configures an instance of <see cref="X509ServiceCertificateAuthentication"/> to validate the WSP' certificate(s).
        /// The default value is X509CertificateValidatorFactory.ChainTrust
        ///     which does online revocation check using entire certificate chain validation.
        /// To customize certificate validation, you can make your own X509CertificateValidator and set it to this property
        /// </summary>
        public X509ServiceCertificateAuthentication WspCertificateAuthentication { get; }

        /// <summary>
        /// Configures an instance of <see cref="X509CertificateValidator"/> to validate the SSL certificate.
        /// The default value is X509CertificateValidatorFactory.ChainTrust
        ///     which does online revocation check using entire certificate chain validation.
        /// To customize certificate validation, you can make your own X509CertificateValidator and set it to this property
        /// </summary>
        public X509ServiceCertificateAuthentication SslCertificateAuthentication { get; }
    }
}
