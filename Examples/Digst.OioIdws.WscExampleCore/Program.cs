using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.WscCore.OioWsTrust;
using Digst.OioIdws.WscExampleCommon;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Text;
using System.Text.Json;

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

            try
            {
                // Get token from STS.
                IStsTokenService stsTokenService = new StsTokenServiceCache(stsConfiguration);
                GenericXmlSecurityToken securityToken;
                try
                {
                    securityToken = (GenericXmlSecurityToken)stsTokenService.GetToken();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Logger.Instance.Error(e.Message, e);
                    throw;
                }
                Console.WriteLine("Token: " + securityToken.TokenXml.OuterXml);

                // Configure HTTP client to use mTLS
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                httpClientHandler.ClientCertificates.Add(stsConfiguration.ClientCertificate);
                httpClientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                using (var client = new HttpClient(httpClientHandler))
                {
                    // Authorize against service
                    var assertionString = securityToken.TokenXml.OuterXml;
                    var content = new StringContent("saml-token=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(assertionString)));

                    var authorizeResult = client.PostAsync("https://portalapi.signatur.dk/token", content).Result;
                    if(!authorizeResult.IsSuccessStatusCode)
                    {
                        throw new Exception(authorizeResult.ToString());
                    }

                    // Parse response
                    var authorizeContent = authorizeResult.Content.ReadAsStringAsync().Result;
                    var accessToken = JsonSerializer.Deserialize<WspAccessToken>(authorizeContent);


                    // Call service
                    // Add Auhtorization header.
                    var jsonContent = new StringContent(@"{""PeriodFrom"":""2023-05-01T00:00:00"",""PeriodTo"":""2023-07-01T23:59:59""}", Encoding.UTF8, "application/json");
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri("https://portalapi.signatur.dk/recruitment/statistics/1"),
                        Method = HttpMethod.Post,
                        Content = jsonContent
                    };
                    request.Headers.Add("Authorization", "Holder-of-key " +  accessToken?.AccessToken);
                    request.Headers.Add("accept", "application/json");
                    
                    // Call service and print response
                    var serviceResult = client.SendAsync(request).Result;

                    if(!serviceResult.IsSuccessStatusCode)
                    {
                        throw new Exception(serviceResult.ToString());
                    }
                    Console.WriteLine(serviceResult.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Logger.Instance.Error(ex.Message, ex);
                Console.ReadKey();
                return;
            }

            Console.ReadKey();
        }
    }
}
