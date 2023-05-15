using System.ComponentModel;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.WscCore.OioWsTrust
{
    /// <summary>
    /// XML Configuration object which reads data from the oioIdwsWcfConfiguration configuration section in app.config
    /// </summary>
    public class OioIdwsWcfConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Endpoint address of STS. E.g. https://adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed
        /// </summary>
        [ConfigurationProperty("stsEndpointAddress", IsRequired = true)]
        public string StsEndpointAddress
        {
            get
            {
                return (string)this["stsEndpointAddress"];
            }
            set
            {
                this["stsEndpointAddress"] = value;
            }
        }

        /// <summary>
        /// Entity Identifier of STS. E.g. https://adgangsstyring.eksterntest-stoettesystemerne.dk/
        /// </summary>
        [ConfigurationProperty("stsEntityIdentifier", IsRequired = false)]
        public string StsEntityIdentifier
        {
            get
            {
                return (string)this["stsEntityIdentifier"];
            }
            set
            {
                this["stsEntityIdentifier"] = value;
            }
        }

        /// <summary>
        /// Endpoint ID of WSP. E.g. https://wsp.oioidws-net.dk/
        /// </summary>
        [ConfigurationProperty("wspEndpointID", IsRequired = true)]
        public string WspEndpointID
        {
            get
            {
                return (string)this["wspEndpointID"];
            }
            set
            {
                this["wspEndpointID"] = value;
            }
        }

        /// <summary>
        /// Endpoint of WSP. E.g. https://digst.oioidws.wsp:9899/HelloWorld
        /// </summary>
        [ConfigurationProperty("wspEndpoint", IsRequired = true)]
        public string WspEndpoint
        {
            get
            {
                return (string)this["wspEndpoint"];
            }
            set
            {
                this["wspEndpoint"] = value;
            }
        }

        /// <summary>
        /// The specific SOAP version to call the WSP. Supported values include "1.1" and "1.2"
        /// If it is not specified, default value "1.1" is used.
        /// </summary>
        [ConfigurationProperty("wspSoapVersion", IsRequired = false)]
        [DefaultValue("1.1")]
        public string WspSoapVersion
        {
            get
            {
                return (string)this["wspSoapVersion"];
            }
            set
            {
                this["wspSoapVersion"] = value;
            }
        }

        /// <summary>
        /// Token life time can be specified in minutes. Default life time is chossen by STS if nothing is specified (8 hours according to the specification at the time of this writing).
        /// If specified, according to specification the STS is not obligated to honor this range and may return a token with a shorter life time in RSTR.
        /// All values above 480 minutes (8 hours) will result in a token life time of 8 hours.
        /// </summary>
        [ConfigurationProperty("tokenLifeTimeInMinutes", IsRequired = false)]
        public int? TokenLifeTimeInMinutes
        {
            get
            {
                return (int?)this["tokenLifeTimeInMinutes"];
            }
            set
            {
                this["tokenLifeTimeInMinutes"] = value;
            }
        }

        /// <summary>
        /// Specifies max size of message received in bytes. If not set, the default value of <see cref="TransportBindingElement.MaxReceivedMessageSize"/> is used.
        /// </summary>
        [ConfigurationProperty("maxReceivedMessageSize", IsRequired = false)]
        public int? MaxReceivedMessageSize
        {
            get
            {
                return (int?)this["maxReceivedMessageSize"];
            }
            set
            {
                this["maxReceivedMessageSize"] = value;
            }
        }

        /// <summary>
        /// If set to true the call timeout to the STS is set to 1 day. This is needed when a developer wants to do debugging and needs more than 1 minute to do the debugging.
        /// </summary>
        [ConfigurationProperty("debugMode", DefaultValue = false, IsRequired = false)]
        public bool DebugMode
        {
            get
            {
                return (bool)this["debugMode"];
            }
            set
            {
                this["debugMode"] = value;
            }
        }

        /// <summary>
        /// If set to true, the SOAP envelop to call WSP will include a Liberty Header
        /// </summary>
        [ConfigurationProperty("includeLibertyHeader", DefaultValue = false, IsRequired = false)]
        public bool IncludeLibertyHeader
        {
            get
            {
                return (bool)this["includeLibertyHeader"];
            }
            set
            {
                this["includeLibertyHeader"] = value;
            }
        }

        /// <summary>
        /// Represents the client certificate including the private key. This should be either a MOCES, FOCES or VOCES certificate.
        /// </summary>
        [ConfigurationProperty("clientCertificate", IsRequired = true)]
        public Certificate ClientCertificate
        {
            get
            {
                return (Certificate) this["clientCertificate"];
            }
            set
            {
                this["clientCertificate"] = value;
            }
        }

        /// <summary>
        /// Represents the onbehalfof certificate for the Proxy OBO request.
        /// </summary>
        [ConfigurationProperty("oboCertificate", IsRequired = false)]
        public Certificate OboCertificate
        {
            get
            {
                return (Certificate)this["oboCertificate"];
            }
            set
            {
                this["oboCertificate"] = value;
            }
        }

        /// <summary>
        /// Represents the STS certificate containing only the public key. This should be a FOCES certificate.
        /// </summary>
        [ConfigurationProperty("stsCertificate", IsRequired = true)]
        public Certificate StsCertificate
        {
            get
            {
                return (Certificate)this["stsCertificate"];
            }
            set
            {
                this["stsCertificate"] = value;
            }
        }

        /// <summary>
        /// Represents the service certificate containing only the public key.
        /// </summary>
        [ConfigurationProperty("serviceCertificate", IsRequired = true)]
        public Certificate ServiceCertificate
        {
            get
            {
                return (Certificate)this["serviceCertificate"];
            }
            set
            {
                this["serviceCertificate"] = value;
            }
        }

        /// <summary>
        /// This is used to determine how many seconds before the token actually expires ... the token should be removed from the cache.
        /// E.g. if token will expire in 100 seconds and <see cref="CacheClockSkewInSeconds"/> is set to 10 seconds ... then the token will be removed from the cache after 90 seconds.
        /// If not set ... the default value is 300 seconds.
        /// This configuration setting is only used in conjunction with <see cref="TokenServiceCache"/>
        /// </summary>
        [ConfigurationProperty("cacheClockSkewInSeconds", IsRequired = false)]
        public int? CacheClockSkewInSeconds
        {
            get
            {
                return (int?)this["cacheClockSkewInSeconds"];
            }
            set
            {
                this["cacheClockSkewInSeconds"] = value;
            }
        }

        /// <summary>
        /// Provide the dk:gov:saml:attribute:CvrNumberIdentifier claim for <see cref="WsTrustRequest"/>
        /// </summary>
        [ConfigurationProperty("cvr", IsRequired = false)]
        public string Cvr
        {
            get
            {
                return (string)this["cvr"];
            }
            set
            {
                this["cvr"] = value;
            }
        }
    }
}
