using Digst.OioIdws.SoapCore.Bindings;
using Digst.OioIdws.SoapCore.Tokens;
using Digst.OioIdws.Common.TestUtils;
using Digst.OioIdws.SoapCore.Test;
using System.ServiceModel.Channels;
using Digst.OioIdws.CommonCore;
using System.Xml.Linq;
using System.ServiceModel;
using System.Security.Cryptography.X509Certificates;
using Digst.OioIdws.CommonCore.Constants;
using System.Xml;
using System.Xml.XPath;
using Digst.OioIdws.OioWsTrustCore;

namespace Digst.OioIdws.SoapCore.Test;

[TestClass]
[TestCategory(Constants.UnitTest)]
public class OioIdwsSoapMessageTransformerTest
{
    [TestMethod]
    public void ModifyMessageAccordingToWspNeeds()
    {
        // Arrange
        var clientCert = Certificates.TestCertificate;
        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.StsCertificate);

        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, clientCert);

        var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
            stsTokenServiceConfiguration, "https://test.service");

        var messageXml = XDocument.Load(@"Data\MessageBeforeTransforming.xml");
        var message = Message.CreateMessage(MessageVersion.Soap11, "Test");
        message = messageXml.ToMessage(message);

        // Act
        var sut = new OioIdwsSoapMessageTransformer(tokenParams);
        var result = sut.ModifyMessageAccordingToWspNeeds(message);

        // Assert
        Assert.IsNotNull(result);

        var namespaceManager = CreateXmlNamespaceManager();
        var xDocument = result.ToXml();
        var toElement = xDocument.XPathSelectElement("/s:Envelope/s:Header/a:To", namespaceManager);
        Assert.IsNotNull(toElement);

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xDocument.ToString());
        var signatureNodeList = xmlDocument.GetElementsByTagName("Signature", Namespaces.SignatureNamespace);
        Assert.IsNotNull(signatureNodeList);
        Assert.AreEqual(2, signatureNodeList.Count); // One is in Header's Assertion
    }

    [TestMethod]
    public void ModifyMessageToSkipDefautValidation()
    {
        // Arrange
        var clientCert = Certificates.TestCertificate;
        var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
        var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.StsCertificate);

        var stsTokenServiceConfiguration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, clientCert);

        var tokenParams = new FederatedSecurityTokenParameters(new TestSecurityToken(), MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
            stsTokenServiceConfiguration, "https://test.service");

        var messageXml = XDocument.Load(@"Data\ResponseBeforeTransforming.xml");
        var response = Message.CreateMessage(MessageVersion.Soap11, "Test");
        response = messageXml.ToMessage(response);

        // Act
        var sut = new OioIdwsSoapMessageTransformer(tokenParams);
        response = sut.ModifyMessageToSkipDefautValidation(response);

        // Assert
        Assert.IsNotNull(response);

        var xDocument = response.ToXml();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xDocument.ToString());
        var signatureNodeList = xmlDocument.GetElementsByTagName("Signature", Namespaces.SignatureNamespace);
        Assert.IsNotNull(signatureNodeList);
        Assert.AreEqual(0, signatureNodeList.Count); // Signature is removed to skip the default validation
    }

    private static XmlNamespaceManager CreateXmlNamespaceManager()
    {
        var namespaceManager = new XmlNamespaceManager(new NameTable());
        namespaceManager.AddNamespace("a", Namespaces.WsaNamespace);
        namespaceManager.AddNamespace("s", Namespaces.S11Namespace);
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
