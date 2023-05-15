using ServiceReference;
using FluentAssertions;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Reflection;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using System.Xml.Linq;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.OioWsTrustCore.Utils;
using Digst.OioIdws.Common.TestUtils;
using System.Xml.XPath;
using System.ServiceModel.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;

namespace Digst.OioIdws.IntegrationTests
{
    [TestClass]
    public class Soap12DotNetIntegrationTests : IntegrationTestBase
    {
        public Soap12DotNetIntegrationTests() : base("1.2")
        {
        }

        private const string HelloSignedFormat = "Hello Sign {0}. Your claims are:\n";
        private string _scenario = "DotNetIntegrationTests.";

        [TestMethod]
        public void TestSystemUserScenario()
        {
            ProxyServerHelper.ExecuteWithProxy(() =>
            {
                _scenario += "TestSystemUserScenario12";

                ConfigureSystemUserScenario();

                TestChannel();
            });
        }

        [TestMethod]
        public void TestSystemUserScenarioWithMaxReceivedMessageSize()
        {
            ProxyServerHelper.ExecuteWithProxy(() =>
            {
                _scenario += "TestSystemUserScenario12";

                Configuration.MaxReceivedMessageSize = 128;//bytes

                ConfigureSystemUserScenario();

                try
                {
                    TestChannel();
                }
                catch (AggregateException ex)
                {
                    StringAssert.Contains(ex.Message, "The maximum message size quota for incoming messages (128) has been exceeded. To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element.");
                }
            });
        }

        [TestMethod]
        public void TestSystemUserScenario_With_CustomReplayAttackCache()
        {
            ProxyServerHelper.ExecuteWithProxy(() =>
            {
                _scenario += "TestSystemUserScenario";
                var customReplayAttackCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions
                {
                    ExpirationScanFrequency = TimeSpan.FromSeconds(20),
                }));
                ConfigureSystemUserScenario(customReplayAttackCache);

                TestChannel();
            });
        }

        [TestMethod]
        public void TestLocalTokenScenario()
        {
            ProxyServerHelper.ExecuteWithProxy(() =>
            {
                _scenario += "TestLocalTokenScenario12";

                ConfigureLocalTokenScenario();

                TestChannel();
            });
        }

        [TestMethod]
        public void TestProxyOboScenario()
        {
            ProxyServerHelper.ExecuteWithProxy(() =>
            {
                _scenario += "TestProxyOboScenario12";

                ConfigureProxyOboScenario();

                TestChannel();
            });
        }

        private bool _containLibertyHeader = false;
        [TestMethod]
        public void IncludeLibertyHeaderTest()
        {
            // Arrange
            Configuration.IncludeLibertyHeader = true;

            ProxyServerHelper.ExecuteWithBeforeRequest(() =>
            {
                var channel = CreateChannel<IHelloWorld>();
                TestChannelMethod(channel.HelloSignAsync, HelloSignedFormat);

                Assert.IsTrue(_containLibertyHeader);
            }, OnValidateRequest);
        }

        private async Task OnValidateRequest(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == Configuration.WspConfiguration.EndpointAddress)
            {
                var requestBody = await e.GetRequestBodyAsString();
                _containLibertyHeader = requestBody.Contains("urn:liberty:sb:profile");
            }
        }

        private bool _hasCorrectContentType = false;
        [TestMethod]
        public void WspResponseHasCorrectContentType()
        {
            // Arrange

            ProxyServerHelper.ExecuteWithBeforeResponse(() =>
            {
                var channel = CreateChannel<IHelloWorld>();
                TestChannelMethod(channel.HelloSignAsync, HelloSignedFormat);

                Assert.IsTrue(_hasCorrectContentType);
            }, OnBeforeResponse);
        }

        private async Task OnBeforeResponse(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == Configuration.WspConfiguration.EndpointAddress)
            {
                var headers = e.HttpClient.Response.Headers;
                _hasCorrectContentType = headers.Any(x => x.Name == "Content-Type" && x.Value == "application/soap+xml; charset=utf-8");
            }

            await Task.CompletedTask;
        }

        [TestMethod]
        public void WspResponseResignTheMessage()
        {
            // Arrange
            ProxyServerHelper.ExecuteWithBeforeResponse(() =>
            {
                var channel = CreateChannel<IHelloWorld>();
                var exception = Assert.ThrowsException<AggregateException>(() => TestChannelMethod(channel.HelloSignAsync, HelloSignedFormat));
                var securityException = exception.InnerException as MessageSecurityException;
                Assert.AreEqual("SOAP signature received from WSP does not validate!", securityException?.Message);
            }, OnResignResponse);
        }

        private async Task OnResignResponse(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.Url == Configuration.WspConfiguration.EndpointAddress)
            {
                var body = await e.GetResponseBodyAsString();
                var doc = XDocument.Parse(body);
                doc.RemoveIfExist("/s:Envelope/s:Header/o:Security/ds:Signature", CreateXmlNamespaceManager());

                var timestamp = doc.XPathSelectElement("/s:Envelope/s:Header/o:Security/u:Timestamp", CreateXmlNamespaceManager());
                string? timestampId = "test";
                if (timestamp != null)
                {
                    timestampId = timestamp.Attribute(XName.Get("Id", Namespaces.WsuNamespace))?.Value;
                }

                var cert = Certificates.TestCertificate;
                var idOfElementsThatMustBeSigned = new List<string>
                {
                    "_1", "_2", "_3", "_4"
                };

                if (!string.IsNullOrEmpty(timestampId))
                {
                    idOfElementsThatMustBeSigned.Add(timestampId);
                }

                var newdoc = XmlSignatureUtils.SignDocument(doc, idOfElementsThatMustBeSigned, cert);

                e.SetResponseBodyString(newdoc.ToString());
            }
        }

        private void TestChannel()
        {
            var channel = CreateChannel<IHelloWorld>();

            // Positive cases
            TestChannelMethod(channel.HelloSignAsync, HelloSignedFormat);

            // Negative cases
            channel.Invoking(c => c.HelloSignErrorAsync(_scenario)).Should().ThrowAsync<Exception>().Wait();
        }

        private void TestChannelMethod(Func<string, Task<string>> channelMethod, string formatString)
        {
            var expectedResponse = string.Format(formatString, _scenario);
            var actualResponse = channelMethod.Invoke(_scenario).Result;
            actualResponse.Should().StartWith(expectedResponse);
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
    }
}