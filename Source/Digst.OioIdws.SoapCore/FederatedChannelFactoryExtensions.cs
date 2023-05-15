using Digst.OioIdws.CommonCore;
using Digst.OioIdws.SoapCore.Bindings;
using Digst.OioIdws.SoapCore.Tokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;

namespace Digst.OioIdws.SoapCore
{
    public static class FederatedChannelFactoryExtensions
    {
        /// <summary>
        /// An equivalent version of the familiar CreateChannelWithIssuedToken helper method in the .NET Framework
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="token">A security token which is issued by an STS</param>
        /// <param name="stsConfiguration">Configuration of the STS and the WSP</param>
        /// <returns>A channel to call service T</returns>
        public static T CreateChannelWithIssuedToken<T>(GenericXmlSecurityToken token, IStsTokenServiceConfiguration stsConfiguration)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (stsConfiguration == null)
                throw new ArgumentNullException(nameof(stsConfiguration));

            // IMPORTANT: https://devblogs.microsoft.com/dotnet/wsfederationhttpbinding-in-net-standard-wcf/
            // First, create the inner binding for communicating with the token issuer.
            // The security settings will be specific to the STS and should mirror what
            // would have been in an app.config in a .NET Framework scenario.

            var serverCertificate = stsConfiguration.WspConfiguration.ServiceCertificate;
            var messageVersion = MessageVersion.CreateVersion(stsConfiguration.WspConfiguration.SoapVersion, AddressingVersion.WSAddressing10);

            // Create a token parameters. The token is then used by FederatedChannelSecurityTokenManager to create an instance of FederatedTokenSecurityTokenProvider which returns the token immediately
            var tokenParameters = new FederatedSecurityTokenParameters(token, messageVersion, stsConfiguration, stsConfiguration.WspConfiguration.EndpointAddress)
            {
                MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10,
                MaxReceivedMessageSize = stsConfiguration.MaxReceivedMessageSize,
                IncludeLibertyHeader = stsConfiguration.IncludeLibertyHeader,
            };

            var bindingToCallService = new OioIdwsSoapBinding(tokenParameters);
            FederatedChannelFactory<T> factory = CreateFactory<T>(stsConfiguration, serverCertificate, bindingToCallService);

            // .NET Core does not support asymmetric binding, so it does not call the CreateSecurityTokenAuthenticator method to create an X509SecurityTokenAuthenticator to validate the service certificate
            // Implement a custom X509SecurityTokenAuthenticator is not an option because not all necessary types used by that abstract class is exposed to .NET Core
            stsConfiguration.WspCertificateAuthentication.Validate(serverCertificate);

            return factory.CreateChannel();
        }

        private static FederatedChannelFactory<T> CreateFactory<T>(IStsTokenServiceConfiguration stsConfiguration, X509Certificate2 serverCertificate, OioIdwsSoapBinding bindingToCallService)
        {
            // we need to create a client 
            var factory = new FederatedChannelFactory<T>(bindingToCallService, new EndpointAddress(stsConfiguration.WspConfiguration.EndpointAddress));
            factory.Credentials.ServiceCertificate.Authentication.CopyFrom(stsConfiguration.WspCertificateAuthentication);
            factory.Credentials.ServiceCertificate.SslCertificateAuthentication = stsConfiguration.SslCertificateAuthentication.DeepClone();

            string dnsName = serverCertificate.GetNameInfo(X509NameType.DnsName, false);
            EndpointIdentity identity = new DnsEndpointIdentity(dnsName);
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(stsConfiguration.WspConfiguration.EndpointAddress), identity);
            factory.Endpoint.Address = endpointAddress;
            factory.Credentials.ClientCertificate.Certificate = stsConfiguration.ClientCertificate;
            factory.Credentials.ServiceCertificate.ScopedCertificates.Add(endpointAddress.Uri, serverCertificate);
            return factory;
        }
    }
}
