using Digst.OioIdws.CommonCore.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Digst.OioIdws.CommonCore;
using System.Xml.XPath;
using System.ServiceModel.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Digst.OioIdws.Common.TestUtils;

namespace Digst.OioIdws.IntegrationTests
{
    [TestClass]
    public class TrustTests
    {
        // Wait 10 minutes:
        //   5 minutes token time + 
        //   5 minutes clockskew + 
        //   10 seconds extra 
        // to be sure that token is expired
        private const int _wait = 610000;

        private readonly StsTokenServiceConfiguration _configuration;

        private string original = string.Empty;
        private string tampered = string.Empty;

        public TrustTests()
        {
            var exeLocation = Assembly.GetExecutingAssembly().Location;
            var configuration = ConfigurationManager.OpenExeConfiguration(exeLocation);
            var configSection = (WscCore.OioWsTrust.OioIdwsWcfConfigurationSection)configuration.GetSection("oioIdwsWcfConfiguration");
            _configuration = TokenServiceConfigurationFactory.CreateConfiguration(configSection);
            _configuration.SslCertificateAuthentication.CertificateValidationMode = X509CertificateValidationMode.None;
        }

        [TestMethod]
        public void TotalFlowSucessTest()
        {
            ProxyServerHelper.ExecuteWithProxy(() =>
            {
                // Arrange
                IStsTokenService stsTokenService =
                new StsTokenService(_configuration);

                // Act
                var securityToken = stsTokenService.GetToken();

                // Assert
                Assert.IsNotNull(securityToken);
            });
        }

        [TestMethod]
        public void TotalFlowSucessTest_GetFromCacheIfExists()
        {
            ProxyServerHelper.ExecuteWithProxy(() =>
            {
                // Arrange
                IStsTokenService stsTokenService =
                new StsTokenServiceCache(_configuration);

                // Act
                SecurityToken securityToken1 = stsTokenService.GetToken();
                SecurityToken securityToken2 = stsTokenService.GetToken();

                // Assert
                Assert.AreSame(securityToken1, securityToken2);
            });
        }

        private bool _isCorrectAlg = false;
        [TestMethod]
        public void OioWsTrustRequestWithCorrectSigningAlgorithm()
        {
            // Arrange
            IStsTokenService stsTokenService =
                new StsTokenService(
                    _configuration
                );

            ProxyServerHelper.ExecuteWithBeforeRequest(() =>
            {
                stsTokenService.GetToken();
                Assert.IsTrue(_isCorrectAlg);
            }, OnValidateRequest);
        }

        private bool _hasContentType = false;
        [TestMethod]
        public void OioWsTrustResponseWithCorrectContentTypeHeader()
        {
            // Arrange
            IStsTokenService stsTokenService =
                new StsTokenService(
                    _configuration
                );
            ProxyServerHelper.ExecuteWithBeforeResponse(() =>
            {
                stsTokenService.GetToken();

                Assert.IsTrue(_hasContentType);
            }, OnBeforeResponse);
        }

        string tamperAction = string.Empty;
        [TestMethod]
        [DataRow("NoSignature")]
        [DataRow("ChangeSignature")]
        [DataRow("ChangeMessage")]
        public void OioWsTrustResponseWithTamperedMessage(string action)
        {
            // Arrange
            IStsTokenService stsTokenService =
                new StsTokenService(
                    _configuration
                );
            ProxyServerHelper.ExecuteWithBeforeResponse(() =>
            {
                this.tamperAction = action;

                switch (action)
                {
                    case "NoSignature":
                        Assert.ThrowsException<MessageSecurityException>(() => stsTokenService.GetToken());
                        break;

                    case "ChangeSignature":
                        Assert.ThrowsException<FormatException>(() => stsTokenService.GetToken());
                        break;
                    case "ChangeMessage":
                        Assert.ThrowsException<XmlException>(() => stsTokenService.GetToken());
                        break;
                }
            }, OnTamperResponse);
        }

        [TestMethod]
        [DataRow("<auth:Value>11111111</auth:Value>", "<auth:Value>11111112</auth:Value>")]
        [DataRow("<a:Action s:mustUnderstand=\"1\"", "<a:Action s:mustUnderstand=\"0\"")]
        [DataRow("uuid:", "tamperedmessageid")]
        public void OioWsTrustRequestFailDueToMessageTamperingTest(string original, string tampered)
        {
            IStsTokenService stsTokenService =
               new StsTokenService(
                   _configuration
               );

            var proxyServer = new ProxyServer();
            try
            {
                var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);

                proxyServer.CertificateManager.CreateRootCertificate(true);
                proxyServer.CertificateManager.TrustRootCertificate(true);
                proxyServer.CertificateManager.TrustRootCertificateAsAdmin(true);

                proxyServer.AddEndPoint(explicitEndPoint);

                proxyServer.BeforeRequest += OnTamperRequest;

                proxyServer.Start();
                proxyServer.SetAsSystemProxy(explicitEndPoint, ProxyProtocolType.AllHttp);

                this.original = original;
                this.tampered = tampered;
                var ex = Assert.ThrowsException<FaultException>(() => stsTokenService.GetToken());
                Assert.AreEqual("An error occurred when verifying security for the message.", ex.Message);
            }
            finally
            {
                proxyServer.BeforeRequest -= OnTamperRequest;
                proxyServer.Stop();
                proxyServer.Dispose();
            }
        }

        private async Task OnTamperResponse(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == _configuration.StsConfiguration.EndpointAddress)
            {
                var body = await e.GetResponseBodyAsString();
                var doc = XDocument.Parse(body);

                switch (this.tamperAction)
                {
                    case "NoSignature":
                        doc.RemoveIfExist("/s:Envelope/s:Header/o:Security/ds:Signature", CreateXmlNamespaceManager());
                        break;
                    case "ChangeSignature":
                        var signature = doc.XPathSelectElement("/s:Envelope/s:Header/o:Security/ds:Signature", CreateXmlNamespaceManager());
                        var child = signature?.DescendantNodes().OfType<XElement>().First(x => x.Name.LocalName == "SignatureValue");
                        if (child != null)
                        {
                            child.Value = child.Value.Remove(0).Insert(0, "B");
                        }
                        break;
                    case "ChangeMessage":
                        doc.RemoveIfExist("/s:Envelope/s:Header/a:RelatesTo", CreateXmlNamespaceManager());

                        break;
                }
                // Tamper message
                e.SetResponseBodyString(doc.ToString());
            }
        }

        private static XmlNamespaceManager CreateXmlNamespaceManager()
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("a", Namespaces.WsaNamespace);
            namespaceManager.AddNamespace("s", Namespaces.S12Namespace);
            namespaceManager.AddNamespace("o", Namespaces.Wsse10Namespace);
            namespaceManager.AddNamespace("vs", Namespaces.VsDebuggerNamespace);
            namespaceManager.AddNamespace("vcf", Namespaces.WcfDiagnosticsNamespace);
            namespaceManager.AddNamespace("wst13", Namespaces.Wst13Namespace);
            namespaceManager.AddNamespace("wst14", Namespaces.Wst14Namespace);
            namespaceManager.AddNamespace("u", Namespaces.WsuNamespace);
            namespaceManager.AddNamespace("ds", Namespaces.SignatureNamespace);

            return namespaceManager;
        }

        private async Task OnTamperRequest(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == _configuration.StsConfiguration.EndpointAddress)
            {
                var requestBody = await e.GetRequestBodyAsString();

                // Tamper message
                e.SetRequestBodyString(requestBody.Replace(original, tampered));
            }
        }

        private async Task OnWaitRequest(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == _configuration.StsConfiguration.EndpointAddress)
            {
                await Task.Delay(_wait);
            }

            await Task.CompletedTask;
        }

        private async Task OnValidateRequest(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == _configuration.StsConfiguration.EndpointAddress)
            {
                var requestBody = await e.GetRequestBodyAsString();

                _isCorrectAlg = requestBody.Contains("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
            }

            await Task.CompletedTask;
        }

        private async Task OnBeforeResponse(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == _configuration.StsConfiguration.EndpointAddress)
            {
                var headers = e.HttpClient.Response.Headers;
                _hasContentType = headers.Any(x => x.Name == "Content-Type" && x.Value == "application/soap+xml; charset=utf-8");
            }

            await Task.CompletedTask;
        }

        [TestMethod]
        public void OioWsTrustRequestExpiredTest()
        {
            // Arrange
            IStsTokenService stsTokenService =
                new StsTokenService(
                    _configuration
                );

            ProxyServerHelper.ExecuteWithBeforeResponse(() =>
            {
                var ex = Assert.ThrowsException<FaultException>(() => stsTokenService.GetToken());
                Assert.AreEqual("An error occurred when verifying security for the message.", ex.Message);
            }, OnWaitRequest);
        }

        [TestMethod]
        public void OioWsTrustResponseExpiredTest()
        {
            // Arrange
            IStsTokenService stsTokenService =
                new StsTokenService(
                    _configuration
                );

            ProxyServerHelper.ExecuteWithBeforeResponse(() =>
            {
                var ex = Assert.ThrowsException<MessageSecurityException>(() => stsTokenService.GetToken());
                StringAssert.Contains("SOAP message has expired.", ex.Message);
            }, OnWaitRequest);
        }

        [TestMethod]
        public void OioWsTrust_ExpiredProxyOboToken_ThrowsException()
        {
            ProxyServerHelper.ExecuteWithProxy(() =>
            {
                // Arrange
                IStsTokenService stsTokenService =
                new StsTokenService(_configuration);

                // Act & Assert
                Assert.ThrowsException<Microsoft.IdentityModel.Tokens.SecurityTokenValidationException>(() => stsTokenService.GetTokenWithProxyOnBehalfOf(new X509SecurityToken(Certificates.ExpiredCertificate)));
            });
        }
    }
}
