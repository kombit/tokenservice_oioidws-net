using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.SoapCore;
using Digst.OioIdws.WscCore.OioWsTrust;
using Digst.OioIdws.WscExampleCommon;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ServiceReference;
using System;
using System.IdentityModel.Tokens;

namespace Digst.OioIdws.WscExampleCore
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Log4NetLogger.Initialize();

            // demonstrate how to use a custom Replay attack cache. Otherwise, you can simply use:
            //   [StsTokenServiceConfiguration stsConfiguration = TokenServiceConfigurationFactory.CreateConfiguration();]
            var replayAttackCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            StsTokenServiceConfiguration stsConfiguration = TokenServiceConfigurationFactory.CreateConfiguration();
            stsConfiguration.ReplayAttackCache = replayAttackCache;

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
