using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.SoapCore;
using Digst.OioIdws.WscCore.OioWsTrust;
using Digst.OioIdws.WscExampleCommon;
using Microsoft.IdentityModel.Tokens.Saml2;
using ServiceReference;
using System;
using System.IdentityModel.Tokens;
using System.Threading;
using System.Xml;

namespace Digst.OioIdws.WscLocalTokenExampleCore
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Log4NetLogger.Initialize();

            StsTokenServiceConfiguration stsConfiguration = TokenServiceConfigurationFactory.CreateConfiguration();
            IHelloWorld channelWithIssuedToken;

            try
            {
                // Request to STS to get a security token to use as an local token
                IStsTokenService stsTokenService = new StsTokenServiceCache(stsConfiguration);
                GenericXmlSecurityToken securityToken = (GenericXmlSecurityToken)stsTokenService.GetToken();
                Console.WriteLine("Direct token: " + securityToken.TokenXml.OuterXml);

                var xmlReader = new XmlNodeReader(securityToken.TokenXml);
                var saml2Serializer = new Saml2Serializer();
                var saml2Assertion = saml2Serializer.ReadAssertion(xmlReader);
                var saml2SecurityToken = new Saml2SecurityToken(saml2Assertion);

                // Use KOMBIT STS to retrieve a token for the WSP by passing the local token.
                // The local token service must have been registered with KOMBIT STS
                // so that its entity ID and signing certificate is known and
                // trusted by KOMBIT STS
                var wspToken = (GenericXmlSecurityToken)stsTokenService.GetTokenWithOnBehalfOf(saml2SecurityToken);
                Console.WriteLine("Normal OBO token: " + wspToken.TokenXml.OuterXml);

                // We now have a token from Kombit STS issued specifically for the web service provider (WSP)
                // Although encrypted so that only the WSP will be able to decrypt, it contains our subject
                // and attributes, and possibly more attributes added by Kombit STS

                //// Set up a communications channel with the WSP based on the token
                channelWithIssuedToken = FederatedChannelFactoryExtensions.CreateChannelWithIssuedToken<IHelloWorld>(wspToken, stsConfiguration);
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
