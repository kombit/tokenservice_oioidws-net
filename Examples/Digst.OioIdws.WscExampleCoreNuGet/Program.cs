using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.SoapCore;
using Digst.OioIdws.WscCore.OioWsTrust;
using log4net.Config;
using ServiceReference;
using System;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.Runtime.Caching;

namespace Digst.OioIdws.WscExampleCoreNuGet
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            // Setup Log4Net configuration by loading it from configuration file. 
            // log4net is not necessary and is only being used for demonstration.
            XmlConfigurator.Configure();
            StsTokenServiceConfiguration stsConfiguration = TokenServiceConfigurationFactory.CreateConfiguration();
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
