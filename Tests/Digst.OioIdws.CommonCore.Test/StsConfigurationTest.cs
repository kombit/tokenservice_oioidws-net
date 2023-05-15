using Digst.OioIdws.Common.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class StsConfigurationTest
    {
        [TestMethod]
        public void Constructor_CanSetPropertiesCorrectly()
        {
            // Arrange
            string stsEndpointAddress = "https://stsEndpointAddress";
            string stsEntityIdentifier = "https://stsEntityIdentifier";
            string cvr = "12345678";
            X509Certificate2 stsCertificate = Certificates.StsCertificate;

            // Act
            var sut = new StsConfiguration(stsEndpointAddress, stsEntityIdentifier, cvr, stsCertificate);

            // Assert
            Assert.AreEqual(stsEndpointAddress, sut.EndpointAddress);
            Assert.AreEqual(stsEntityIdentifier, sut.EntityIdentifier);
            Assert.AreEqual(cvr, sut.Cvr);
            Assert.AreSame(stsCertificate, sut.Certificate);
            Assert.IsNull(sut.OboCertificate);

            // Another Act and Assertion
            sut.OboCertificate = stsCertificate;
            Assert.AreSame(stsCertificate, sut.OboCertificate);
        }

        [TestMethod]
        public void Constructor_CopyConstructor_CanSetPropertiesCorrectly()
        {
            // Arrange
            string stsEndpointAddress = "https://stsEndpointAddress";
            string stsEntityIdentifier = "https://stsEntityIdentifier";
            string cvr = "12345678";
            X509Certificate2 stsCertificate = Certificates.StsCertificate;

            // Act
            var other = new StsConfiguration(stsEndpointAddress, stsEntityIdentifier, cvr, stsCertificate)
            {
                OboCertificate = Certificates.TestCertificate
            };

            var sut = new StsConfiguration(other);

            // Assert
            Assert.AreEqual(other.EndpointAddress, sut.EndpointAddress);
            Assert.AreEqual(other.EntityIdentifier, sut.EntityIdentifier);
            Assert.AreEqual(other.Cvr, sut.Cvr);
            Assert.AreSame(other.Certificate, sut.Certificate);
            Assert.AreSame(other.OboCertificate, sut.OboCertificate);
        }

        /// <summary>
        /// Invalid input parameter cases
        /// Test to ensure that an StsConfiguration is always in a valid state
        /// </summary>
        [TestMethod]
        [DataRow("", "https://stsEntityIdentifier", "12345678")]
        [DataRow("https://stsEndpointAddress", "", "12345678")]
        [DataRow("https://stsEndpointAddress", "https://stsEntityIdentifier", "")]
        public void Constructor_ThrowWhenInputParameterIsEmpty(string stsEndpointAddress, string stsEntityIdentifier, string cvr)
        {
            // Arrange
            X509Certificate2 stsCertificate = Certificates.StsCertificate;

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new StsConfiguration(stsEndpointAddress, stsEntityIdentifier, cvr, stsCertificate));

            // Assert
        }

        [TestMethod]
        public void Constructor_ThrowWhenStsCertificateIsNull()
        {
            // Arrange
            string stsEndpointAddress = "https://stsEndpointAddress";
            string stsEntityIdentifier = "https://stsEntityIdentifier";
            string cvr = "12345678";
            X509Certificate2? stsCertificate = null;

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new StsConfiguration(stsEndpointAddress, stsEntityIdentifier, cvr, stsCertificate));

            // Assert
        }

        [TestMethod]
        public void Constructor_CopyConstructor_ThrowsWhenOtherIsNull()
        {
            // Arrange
            StsConfiguration? other = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new StsConfiguration(other));
        }
    }
}
