using System;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.OioWsTrustCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Digst.OioIdws.WscCore.OioWsTrust
{
    /// <summary>
    /// This factory class can be used to generate a <see cref="StsTokenServiceConfiguration"/> configuration based on a <see cref="OioIdwsWcfConfigurationSection"/> configuration.
    /// </summary>
    public static class TokenServiceConfigurationFactory
    {
        /// <summary>
        /// This method creates a new instance of the StsTokenServiceConfiguration class by taking an instance of OioIdwsWcfConfigurationSection as input.
        /// </summary>
        /// <param name="wscConfiguration">An instance of <see cref="OioIdwsWcfConfigurationSection"/></param>
        /// <returns>An instance of <see cref="StsTokenServiceConfiguration"/></returns>
        /// <exception cref="ArgumentNullException">When the input wscConfiguration parameter is null</exception>
        public static StsTokenServiceConfiguration CreateConfiguration(OioIdwsWcfConfigurationSection wscConfiguration)
        {
            if (wscConfiguration == null)
            {
                throw new ArgumentNullException("wscConfiguration");
            }

            var soapVersion = EnvelopeVersion.Soap11;
            if (wscConfiguration.WspSoapVersion == "1.2")
            {
                soapVersion = EnvelopeVersion.Soap11;
            }

            X509Certificate2 stsCertificate = CertificateUtil.GetCertificate(wscConfiguration.StsCertificate);
            X509Certificate2 serviceCertificate = CertificateUtil.GetCertificate(wscConfiguration.ServiceCertificate);
            X509Certificate2 clientCertificate = CertificateUtil.GetCertificate(wscConfiguration.ClientCertificate);

            var stsConfiguration = new StsConfiguration(wscConfiguration.StsEndpointAddress, wscConfiguration.StsEntityIdentifier,
                                                        wscConfiguration.Cvr, stsCertificate);
            var wspConfiguration = new WspConfiguration(wscConfiguration.WspEndpoint, wscConfiguration.WspEndpointID,
                                                        soapVersion, serviceCertificate);

            var tokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, clientCertificate)
            {
                SendTimeout = wscConfiguration.DebugMode ? TimeSpan.FromDays(1) : (TimeSpan?)null,
                TokenLifeTimeInMinutes = wscConfiguration.TokenLifeTimeInMinutes,
                MaxReceivedMessageSize = wscConfiguration.MaxReceivedMessageSize,
                IncludeLibertyHeader = wscConfiguration.IncludeLibertyHeader
            };

            InitializeOtherOptionalParameters(wscConfiguration, stsConfiguration, tokenServiceConfiguration);

            return tokenServiceConfiguration;
        }

        /// <summary>
        /// This method creates a new instance of the StsTokenServiceConfiguration class using the oioIdwsWcfConfiguration section specified in the application configuration file.
        /// </summary>
        /// <param name="wscConfiguration">An instance of <see cref="OioIdwsWcfConfigurationSection"/></param>
        /// <returns>An instance of <see cref="StsTokenServiceConfiguration"/></returns>
        /// <exception cref="ArgumentNullException">When the input wscConfiguration parameter is null</exception>
        public static StsTokenServiceConfiguration CreateConfiguration()
        {
            var wscConfiguration =
                (OioIdwsWcfConfigurationSection)System.Configuration.ConfigurationManager.GetSection("oioIdwsWcfConfiguration");

            return CreateConfiguration(wscConfiguration);
        }

        private static void InitializeOtherOptionalParameters(OioIdwsWcfConfigurationSection wscConfiguration, StsConfiguration stsConfiguration, StsTokenServiceConfiguration tokenServiceConfiguration)
        {
            if (wscConfiguration.OboCertificate != null &&
                (!string.IsNullOrEmpty(wscConfiguration.OboCertificate.FindValue) || !string.IsNullOrEmpty(wscConfiguration.OboCertificate.FilePath)))
            {
                stsConfiguration.OboCertificate = CertificateUtil.GetCertificate(wscConfiguration.OboCertificate);
            }

            if (wscConfiguration.CacheClockSkewInSeconds.HasValue)
                tokenServiceConfiguration.CacheClockSkew =
                    TimeSpan.FromSeconds((double)wscConfiguration.CacheClockSkewInSeconds);
        }
    }
}