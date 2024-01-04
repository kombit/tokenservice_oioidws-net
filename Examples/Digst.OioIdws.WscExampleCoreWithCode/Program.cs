using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.SoapCore;
using Digst.OioIdws.WscCore.OioWsTrust;
using log4net.Config;
using ServiceReference;
using System;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;

namespace Digst.OioIdws.WscExampleCoreWithCode
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            OioIdwsWcfConfigurationSection wscConfiguration = new OioIdwsWcfConfigurationSection
            {

                StsEndpointAddress = "https://n2adgangsstyring.eksterntest-stoettesystemerne.dk/runtime/services/kombittrust/14/certificatemixed",
                StsEntityIdentifier = "https://saml.n2adgangsstyring.eksterntest-stoettesystemerne.dk/runtime",
                StsCertificate = new Certificate
                {
                   StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                   StoreName = System.Security.Cryptography.X509Certificates.StoreName.TrustedPeople,
                   X509FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint,
                   FindValue = "7002cf221d1d3979eca623599e43e0b6b4c8920c"
                },

                WspEndpoint = "https://kombitwsp12.azurewebsites.net/HelloWorld.svc",
                WspEndpointID = "http://wsp12.oioidws-net.dk/service/service/1",
                WspSoapVersion = "1.2",
                ServiceCertificate = new Certificate
                {
                    StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    StoreName = System.Security.Cryptography.X509Certificates.StoreName.TrustedPeople,
                    X509FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint,
                    FindValue = "ca30025a4981147505b8d7a59052ac40c7033688"
                },
                ClientCertificate = new Certificate 
                {
                    StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    StoreName = System.Security.Cryptography.X509Certificates.StoreName.My,
                    X509FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint,
                    FindValue = "79cfb7a45b74f4c64ea408503002df7d1143ef40"
                },

                Cvr = "11111111",
                TokenLifeTimeInMinutes = 5,
                IncludeLibertyHeader = true,
            };

            StsTokenServiceConfiguration stsConfiguration = TokenServiceConfigurationFactory.CreateConfiguration(wscConfiguration);
            IHelloWorld channelWithIssuedToken;

            // Retrieve token and create channel to call WSP
            try
            {
                IStsTokenService stsTokenService = new StsTokenServiceCache(stsConfiguration);
                var securityToken = (GenericXmlSecurityToken)stsTokenService.GetToken();
                Console.WriteLine("Direct token: " + securityToken.TokenXml.OuterXml);

                channelWithIssuedToken = FederatedChannelFactoryExtensions.CreateChannelWithIssuedToken<IHelloWorld>(securityToken, stsConfiguration);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Instance.Error(ex.Message, ex);
                Console.ReadKey();
                return;
            }

            try
            {
                // Invoke a WSP operation which requires signature but not encryption.
                Console.WriteLine(channelWithIssuedToken.HelloSignAsync("Schultz").Result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Logger.Instance.Error(e.Message, e);

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                    Logger.Instance.Error(e.InnerException.Message, e.InnerException);
                }
            }

            //Checking that SOAP faults can be read.
            try
            {
                var error = channelWithIssuedToken.HelloSignErrorAsync("Schultz").Result;
                Console.WriteLine(error);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                    Logger.Instance.Error(e.InnerException.Message, e.InnerException);
                }
                else
                {
                    Console.WriteLine(e.Message);
                    Logger.Instance.Error(e.Message, e);
                }
            }

            Console.ReadKey();
        }
    }
}
