using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;
using Digst.OioIdws.Common.TestUtils;
using System.ServiceModel;
using System.Net;

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

            // Act
            var other = new WspConfiguration(wspEndpoint, wspEndpointId, envelopeVersion, serviceCertificate);
            var sut = new WspConfiguration(other);

            // Assert
            Assert.AreEqual(other.EndpointAddress, sut.EndpointAddress);
            Assert.AreEqual(other.EndpointId, sut.EndpointId);
            Assert.AreEqual(other.SoapVersion, sut.SoapVersion);
            Assert.AreSame(other.ServiceCertificate, sut.ServiceCertificate);
        }

        /// <summary>
        /// Invalid input parameter cases
        /// Test to ensure that an StsConfiguration is always in a valid state
        /// </summary>
        [TestMethod]
        [DataRow("", "https://wspEndpointId")]
        [DataRow("https://stsEndpointAddress", "")]
        public void Constructor_ThrowWhenInputParameterIsEmpty(string wspEndpoint, string wspEndpointId)
        {
            // Arrange
            EnvelopeVersion envelopeVersion = EnvelopeVersion.Soap11;
            X509Certificate2 serviceCertificate = Certificates.TestCertificate;

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new WspConfiguration(wspEndpoint, wspEndpointId, envelopeVersion, serviceCertificate));

            // Assert
        }

        [TestMethod]
        public void Constructor_ThrowWhenServerCertificateIsNull()
        {
            // Arrange
            string wspEndpoint = "https://wspEndpoint";
            string wspEndpointId = "https://wspEndpointId";
            EnvelopeVersion envelopeVersion = EnvelopeVersion.Soap11;
            X509Certificate2? serviceCertificate = null;

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new WspConfiguration(wspEndpoint, wspEndpointId, envelopeVersion, serviceCertificate));

            // Assert
        }

        [TestMethod]
        public void Constructor_ThrowWhenEnvelopVersionIsNull()
        {
            // Arrange
            string wspEndpoint = "https://wspEndpoint";
            string wspEndpointId = "https://wspEndpointId";
            EnvelopeVersion? envelopeVersion = null;
            X509Certificate2 serviceCertificate = Certificates.TestCertificate;

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new WspConfiguration(wspEndpoint, wspEndpointId, envelopeVersion, serviceCertificate));

            // Assert
        }

        [TestMethod]
        public void Constructor_CopyConstructor_ThrowsWhenOtherIsNull()
        {
            // Arrange
            WspConfiguration? other = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new WspConfiguration(other));
        }
    }
}
