using Digst.OioIdws.Common.TestUtils;
using System.ServiceModel;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class WspConfigurationTest
    {
        [TestMethod]
        public void Constructor_CanSetPropertiesCorrectly()
        {
            // Arrange
            string wspEndpoint = "https://wspEndpoint";
            string wspEndpointId = "https://wspEndpointId";
            EnvelopeVersion envelopeVersion = EnvelopeVersion.Soap11;
            X509Certificate2 serviceCertificate = Certificates.TestCertificate;

            // Act
            var sut = new WspConfiguration(wspEndpoint, wspEndpointId, envelopeVersion, serviceCertificate);

            // Assert
            Assert.AreEqual(wspEndpoint, sut.EndpointAddress);
            Assert.AreEqual(wspEndpointId, sut.EndpointId);
            Assert.AreEqual(envelopeVersion, sut.SoapVersion);
            Assert.AreSame(serviceCertificate, sut.ServiceCertificate);
        }

        [TestMethod]
        public void Constructor_CopyConstructor_CanSetPropertiesCorrectly()
        {
            // Arrange
            string wspEndpoint = "https://wspEndpoint";
            string wspEndpointId = "https://wspEndpointId";
            EnvelopeVersion envelopeVersion = EnvelopeVersion.Soap11;
            X509Certificate2 serviceCertificate = Certificates.TestCertificate;

            var other = new WspConfiguration(wspEndpoint, wspEndpointId, envelopeVersion, serviceCertificate);

            // Act
            var sut = new WspConfiguration(other);

            // Assert
            Assert.AreEqual(other.EndpointAddress, sut.EndpointAddress);
            Assert.AreEqual(other.EndpointId, sut.EndpointId);
            Assert.AreEqual(other.SoapVersion, sut.SoapVersion);
            Assert.AreSame(other.ServiceCertificate, sut.ServiceCertificate);
        }

        [TestMethod]
        [DataRow("", "https://wspEndpointId", DisplayName = "Empty endpoint address")]
        [DataRow("https://wspEndpointAddress", "", DisplayName = "Empty endpoint ID")]
        [DataRow(null, "https://wspEndpointId", DisplayName = "Null endpoint address")]
        [DataRow("https://wspEndpointAddress", null, DisplayName = "Null endpoint ID")]
        public void Constructor_ThrowWhenInputParameterIsNullOrEmpty(string? wspEndpoint, string? wspEndpointId)
        {
            // Arrange
            EnvelopeVersion envelopeVersion = EnvelopeVersion.Soap11;
            X509Certificate2 serviceCertificate = Certificates.TestCertificate;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new WspConfiguration(wspEndpoint!, wspEndpointId!, envelopeVersion, serviceCertificate));
        }

        [TestMethod]
        public void Constructor_ThrowWhenServerCertificateIsNull()
        {
            // Arrange
            string wspEndpoint = "https://wspEndpoint";
            string wspEndpointId = "https://wspEndpointId";
            EnvelopeVersion envelopeVersion = EnvelopeVersion.Soap11;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new WspConfiguration(wspEndpoint, wspEndpointId, envelopeVersion, null!));
        }

        [TestMethod]
        public void Constructor_ThrowWhenEnvelopVersionIsNull()
        {
            // Arrange
            string wspEndpoint = "https://wspEndpoint";
            string wspEndpointId = "https://wspEndpointId";
            X509Certificate2 serviceCertificate = Certificates.TestCertificate;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new WspConfiguration(wspEndpoint, wspEndpointId, null!, serviceCertificate));
        }

        [TestMethod]
        public void Constructor_CopyConstructor_ThrowsWhenOtherIsNull()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new WspConfiguration(null!));
        }

        [TestMethod]
        public void Constructor_WithSoap12_SetsVersionCorrectly()
        {
            // Arrange
            string wspEndpoint = "https://wspEndpoint";
            string wspEndpointId = "https://wspEndpointId";
            EnvelopeVersion envelopeVersion = EnvelopeVersion.Soap12;
            X509Certificate2 serviceCertificate = Certificates.TestCertificate;

            // Act
            var sut = new WspConfiguration(wspEndpoint, wspEndpointId, envelopeVersion, serviceCertificate);

            // Assert
            Assert.AreEqual(EnvelopeVersion.Soap12, sut.SoapVersion);
        }
    }
}
