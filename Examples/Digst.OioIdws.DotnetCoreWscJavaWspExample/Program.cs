using System;
using System.IdentityModel.Tokens;
using System.Threading;
using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.SoapCore;
using Digst.OioIdws.WscCore.OioWsTrust;
using Digst.OioIdws.WscExampleCommon;
using ServiceReference;

namespace Digst.OioIdws.DotnetCoreWscJavaWspExample
{
    static class Program
    {
        static void Main(string[] args)
        {
            Log4NetLogger.Initialize();

            // To ensure that the WSP is up and running.
            Thread.Sleep(1000);
            
            StsTokenServiceConfiguration stsConfiguration = TokenServiceConfigurationFactory.CreateConfiguration();
            HelloWorldPortType channelWithIssuedToken;

            // Retrieve token and create channel to call Java WSP
            try
            {
                IStsTokenService stsTokenService = new StsTokenServiceCache(stsConfiguration);
                var securityToken = (GenericXmlSecurityToken)stsTokenService.GetToken();

                Console.WriteLine("Security Token:" + securityToken.TokenXml.OuterXml);
                channelWithIssuedToken = FederatedChannelFactoryExtensions.CreateChannelWithIssuedToken<HelloWorldPortType>(securityToken, stsConfiguration);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Instance.Error(ex.Message, ex);
                Console.ReadKey();
                return;
            }

            try
            {
                var helloWorldRequestJohn = new HelloWorldRequest("John");
                var response = channelWithIssuedToken.HelloWorld(helloWorldRequestJohn);
                Console.WriteLine(response.response);
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

            Console.ReadLine();
        }
    }
}
