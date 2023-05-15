using Digst.OioIdws.CommonCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Xml;
using System;
using System.Collections.Specialized;
using System.IdentityModel.Selectors;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace Digst.OioIdws.OioWsTrustCore
{
    /// <summary>
    /// Configuration for the security token service
    /// </summary>
    public class StsTokenServiceConfiguration : IStsTokenServiceConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StsTokenServiceConfiguration"/> class.
        /// </summary>
        public StsTokenServiceConfiguration(StsConfiguration stsConfiguration,
            WspConfiguration wspConfiguration,
            X509Certificate2 clientCertificate)
        {
            StsConfiguration = stsConfiguration ?? throw new ArgumentNullException(nameof(stsConfiguration));
            WspConfiguration = wspConfiguration ?? throw new ArgumentNullException(nameof(wspConfiguration));
            ClientCertificate = clientCertificate ?? throw new ArgumentNullException(nameof(clientCertificate));

            CacheClockSkew = TimeSpan.FromSeconds(60);
            _replayAttackCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StsTokenServiceConfiguration"/> class.
        /// </summary>
        public StsTokenServiceConfiguration(StsTokenServiceConfiguration other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            StsConfiguration = new StsConfiguration(other.StsConfiguration);
            WspConfiguration = new WspConfiguration(other.WspConfiguration);
            ClientCertificate = other.ClientCertificate;

            SendTimeout = other.SendTimeout;
            TokenLifeTimeInMinutes = other.TokenLifeTimeInMinutes;
            CacheClockSkew = other.CacheClockSkew;
            _replayAttackCache = other.ReplayAttackCache;   // C# compiler cannot detect that when setting via the ReplayAttackCache property, the _replayAttackCache field is set
            MaxReceivedMessageSize = other.MaxReceivedMessageSize;
            IncludeLibertyHeader = other.IncludeLibertyHeader;
            SslCertificateAuthentication = other.SslCertificateAuthentication.DeepClone();
            StsCertificateAuthentication = other.StsCertificateAuthentication.DeepClone();
            WspCertificateAuthentication = other.WspCertificateAuthentication.DeepClone();
        }

        /// <summary>
        /// Configuration containing all necessary information to request an STS service to obtain a token for a Wsp endpoint calling.
        /// </summary>
        public StsConfiguration StsConfiguration { get; }

        /// <summary>
        /// Configuration containing all Wsp related information such as the Wsp endpoint Id, address, supported SOAP version and service certificate.
        /// </summary>
        public WspConfiguration WspConfiguration { get; }

        /// <summary>
        /// Token life time can be specified in minutes. Default life time is chossen by STS if nothing is specified (8 hours according to the specification at the time of this writing).
        /// If specified, according to specification the STS is not obligated to honor this range and may return a token with a shorter life time in RSTR.
        /// All values above 480 minutes (8 hours) will result in a token life time of 8 hours from STS.
        /// All values below 1 minute will result in a token life time of 8 hours from STS.
        /// Should be longer than <see cref="CacheClockSkew"/>. Otherwise, the STS token will never be cached.
        /// </summary>
        public int? TokenLifeTimeInMinutes { get; set; }

        /// <summary>
        /// Specifies max size of message received in bytes. If not set, default value on <see cref="TransportBindingElement.MaxReceivedMessageSize"/> are used.
        /// </summary>
        public int? MaxReceivedMessageSize { get; set; }

        /// <summary>
        /// Specifies the timeout of a SOAP request
        /// </summary>
        public TimeSpan? SendTimeout { get; set; }

        /// <summary>
        /// Represents the client certificate including the private key. This should be either a MOCES, FOCES or VOCES certificate.
        /// </summary>
        public X509Certificate2 ClientCertificate { get; }

        /// <summary>
        /// This is used to determine how long before the token actually expires ... the token should be removed from the cache.
        /// E.g. if token will expire in 100 seconds and <see cref="CacheClockSkew"/> is set to 10 seconds ... then the token will be removed from the cache after 90 seconds.
        /// If not set ... the default value is 60 seconds.
        /// This configuration setting is only used in conjunction with <see cref="StsTokenServiceCache"/>
        /// Should be shorter than <see cref="TokenLifeTimeInMinutes"/>. Otherwise, the access token will never be cached. 
        /// </summary>
        public TimeSpan CacheClockSkew { get; set; }

        /// <summary>
        /// Specify that if the SOAP's header include a liberty Framework header
        /// </summary>
        public bool IncludeLibertyHeader { get; set; }

        private IDistributedCache _replayAttackCache;
        /// <summary>
        /// Default in-memory replay attack cache
        /// </summary>
        public IDistributedCache ReplayAttackCache
        {
            get
            {
                return _replayAttackCache;
            }
            set
            {
                _replayAttackCache = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Configures an instance of <see cref="X509CertificateValidator"/> to validate the STS' certificate(s).
        /// The default value is X509CertificateValidatorFactory.ChainTrust
        ///     which does online revocation check using entire certificate chain validation.
        /// To customize certificate validation, you can make your own X509CertificateValidator and set it to this property
        /// </summary>
        public X509ServiceCertificateAuthentication StsCertificateAuthentication { get; } = new X509ServiceCertificateAuthentication();

        /// <summary>
        /// Configures an instance of <see cref="X509CertificateValidator"/> to validate the WSP' certificate(s).
        /// The default value is X509CertificateValidatorFactory.ChainTrust
        ///     which does online revocation check using entire certificate chain validation.
        /// To customize certificate validation, you can make your own X509CertificateValidator and set it to this property
        /// </summary>
        public X509ServiceCertificateAuthentication WspCertificateAuthentication { get; } = new X509ServiceCertificateAuthentication();

        /// <summary>
        /// Configures an instance of <see cref="X509CertificateValidator"/> to validate the SSL certificate.
        /// The default value is X509CertificateValidatorFactory.ChainTrust
        ///     which does online revocation check using entire certificate chain validation.
        /// To customize certificate validation, you can make your own X509CertificateValidator and set it to this property
        /// </summary>
        public X509ServiceCertificateAuthentication SslCertificateAuthentication { get; } = new X509ServiceCertificateAuthentication();
    }
}
