using Digst.OioIdws.Common.TestUtils;
using Digst.OioIdws.CommonCore;
using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.SoapCore.Tokens;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security.Tokens;

namespace Digst.OioIdws.SoapCore.Test
{
    [TestClass]
    public class FederatedSecurityTokenParametersTest
    {
        [TestMethod]
        public void Constructor_WithNullStsTokenServiceConfiguration_ThrowsArgumentNullException()
        {
            // Arrange
            GenericXmlSecurityToken securityToken = new TestSecurityToken();
            MessageVersion messageVersion = MessageVersion.Default;
            IStsTokenServiceConfiguration stsTokenServiceConfiguration = null;
            string wspEndpoint = "https://testwspendpoint";

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                new FederatedSecurityTokenParameters(securityToken, messageVersion, stsTokenServiceConfiguration, wspEndpoint);
            }, "The argument 'stsTokenServiceConfiguration' cannot be null.");
        }

        [TestMethod]
        public void Constructor_WithNullSecurityToken_ThrowsArgumentNullException()
        {
            // Arrange
            GenericXmlSecurityToken securityToken = null;
            MessageVersion messageVersion = MessageVersion.Default;
            IStsTokenServiceConfiguration stsTokenServiceConfiguration = CreateStsTokenServiceConfiguration();
            string wspEndpoint = "https://testwspendpoint";

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                new FederatedSecurityTokenParameters(securityToken, messageVersion, stsTokenServiceConfiguration, wspEndpoint);
            }, "The argument 'securityToken' cannot be null.");
        }

        [TestMethod]
        public void Constructor_WithNullMessageVersion_ThrowsArgumentNullException()
        {
            // Arrange
            GenericXmlSecurityToken securityToken = new TestSecurityToken();
            MessageVersion messageVersion = null;
            IStsTokenServiceConfiguration stsTokenServiceConfiguration = CreateStsTokenServiceConfiguration();
            string wspEndpoint = "https://testwspendpoint";

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                new FederatedSecurityTokenParameters(securityToken, messageVersion, stsTokenServiceConfiguration, wspEndpoint);
            }, "The argument 'messageVersion' cannot be null.");
        }

        [TestMethod]
        public void Constructor_WithNullOrEmptyWspEndpoint_ThrowsArgumentNullException()
        {
            // Arrange
            GenericXmlSecurityToken securityToken = new TestSecurityToken();
            MessageVersion messageVersion = MessageVersion.Default;
            IStsTokenServiceConfiguration stsTokenServiceConfiguration = CreateStsTokenServiceConfiguration();
            string wspEndpoint = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                new FederatedSecurityTokenParameters(securityToken, messageVersion, stsTokenServiceConfiguration, wspEndpoint);
            }, "The argument 'wspEndpoint' cannot be null or empty.");
        }

        [TestMethod]
        public void Constructor_WithValidArguments_SetsPropertiesCorrectly()
        {
            // Arrange
            GenericXmlSecurityToken securityToken = new TestSecurityToken();
            MessageVersion messageVersion = MessageVersion.Default;
            IStsTokenServiceConfiguration stsTokenServiceConfiguration = CreateStsTokenServiceConfiguration();
            string wspEndpoint = "https://localhost:1234";

            // Act
            FederatedSecurityTokenParameters tokenParameters = new FederatedSecurityTokenParameters(securityToken, messageVersion, stsTokenServiceConfiguration, wspEndpoint);

            // Assert
            Assert.AreSame(securityToken, tokenParameters.SecurityToken);
            Assert.AreEqual(messageVersion, tokenParameters.MessageVersion);
            Assert.AreSame(stsTokenServiceConfiguration, tokenParameters.StsTokenServiceConfiguration);
            Assert.AreSame(stsTokenServiceConfiguration.ClientCertificate, tokenParameters.ClientCertificate);
            Assert.AreSame(stsTokenServiceConfiguration.WspConfiguration.ServiceCertificate, tokenParameters.ServerCertificate);
            Assert.AreEqual(wspEndpoint, tokenParameters.WspEndpoint);
            Assert.IsNull(tokenParameters.MaxReceivedMessageSize);
            Assert.IsFalse(tokenParameters.IncludeLibertyHeader);
        }


        [TestMethod]
        public void CopyConstructor_WithValidArguments_CopiesPropertiesCorrectly()
        {
            // Arrange
            GenericXmlSecurityToken securityToken = new TestSecurityToken();
            MessageVersion messageVersion = MessageVersion.Default;
            IStsTokenServiceConfiguration stsTokenServiceConfiguration = CreateStsTokenServiceConfiguration();
            string wspEndpoint = "https://localhost:1234";
            var originalTokenParameters = new FederatedSecurityTokenParametersStub(securityToken, messageVersion, stsTokenServiceConfiguration, wspEndpoint);

            // Act
            var copiedTokenParameters = originalTokenParameters.StubCloneCore() as FederatedSecurityTokenParameters;

            // Assert
            Assert.AreSame(originalTokenParameters.SecurityToken, copiedTokenParameters.SecurityToken);
            Assert.AreEqual(originalTokenParameters.MessageVersion, copiedTokenParameters.MessageVersion);
            Assert.AreSame(originalTokenParameters.StsTokenServiceConfiguration, copiedTokenParameters.StsTokenServiceConfiguration);
            Assert.AreSame(originalTokenParameters.ClientCertificate, copiedTokenParameters.ClientCertificate);
            Assert.AreSame(originalTokenParameters.ServerCertificate, copiedTokenParameters.ServerCertificate);
            Assert.AreEqual(originalTokenParameters.WspEndpoint, copiedTokenParameters.WspEndpoint);
            Assert.AreEqual(originalTokenParameters.MaxReceivedMessageSize, copiedTokenParameters.MaxReceivedMessageSize);
            Assert.AreEqual(originalTokenParameters.IncludeLibertyHeader, copiedTokenParameters.IncludeLibertyHeader);
        }

        private class FederatedSecurityTokenParametersStub : FederatedSecurityTokenParameters
        {
            public FederatedSecurityTokenParametersStub(GenericXmlSecurityToken securityToken, MessageVersion messageVersion, IStsTokenServiceConfiguration stsTokenServiceConfiguration, string wspEndpoint) : base(securityToken, messageVersion, stsTokenServiceConfiguration, wspEndpoint)
            {
            }

            public SecurityTokenParameters StubCloneCore()
            {
                return CloneCore();
            }
        }

        private static StsTokenServiceConfiguration CreateStsTokenServiceConfiguration()
        {
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
            var clientCertificate = Certificates.TestCertificate;

            return new StsTokenServiceConfiguration(
                stsConfiguration,
                wspConfiguration,
                clientCertificate);
        }
    }
}
