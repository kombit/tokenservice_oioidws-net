using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.SoapCore;
using Digst.OioIdws.WscCore.OioWsTrust;
using Digst.OioIdws.WscExampleCommon;
using log4net.Config;
using ServiceReference;
using System;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.WscProxyOBOExampleCore
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Log4NetLogger.Initialize();

            StsTokenServiceConfiguration stsConfiguration = TokenServiceConfigurationFactory.CreateConfiguration();
            X509Certificate2? oboCertificate = stsConfiguration.StsConfiguration.OboCertificate;

            // Retrieve token
            IStsTokenService stsTokenService = new StsTokenServiceCache(stsConfiguration);
            if (oboCertificate == null)
            {
                throw new ArgumentException("oboCertificate configuration is missing.");
            }

            IHelloWorld channelWithIssuedToken;
            try
            {
                var securityToken = (GenericXmlSecurityToken)stsTokenService.GetTokenWithProxyOnBehalfOf(new X509SecurityToken(oboCertificate));
                Console.WriteLine("Proxy OBO token: " + securityToken.TokenXml.OuterXml);

                // Set up a communications channel with the WSP based on the token
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

            // Invoke a WSP operation which expects signed requests and which will fail with a signed but *not* encrypted SOAP fault
            // SOAP faults are unencrypted by WSP only if special care is taken.
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
