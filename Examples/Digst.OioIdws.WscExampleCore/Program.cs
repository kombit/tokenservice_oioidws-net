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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Digst.OioIdws.WscExampleCore
{
    public class WspAccessToken
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

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
                var securityToken = (GenericXmlSecurityToken)stsTokenService.GetToken();
                Console.WriteLine("Token: " + securityToken.TokenXml.OuterXml);

                // COnfigure HTTP client to use mTLS
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                httpClientHandler.ClientCertificates.Add(stsConfiguration.ClientCertificate);
                httpClientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                using (var client = new HttpClient(httpClientHandler))
                {
                    // Authorize
                    var content = new StringContent("saml-token=" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(securityToken.TokenXml.OuterXml)));

                    var authorizeResult = client.PostAsync("https://videoapi.vconf-stage.dk/videoapi/token", content).Result;
                    var c = authorizeResult.Content.ReadAsStringAsync().Result;
                    var accessToken = JsonSerializer.Deserialize<WspAccessToken>(c);


                    // Call service
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri("https://videoapi.vconf-stage.dk/videoapi/meetings/b2670f63-04e0-46ec-bf61-1cbd0baa15d3"),
                    };
                    request.Headers.Add("Authorization", "Holder-of-key " +  accessToken?.AccessToken);
                    
                    var serviceResult = client.SendAsync(request).Result;
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
