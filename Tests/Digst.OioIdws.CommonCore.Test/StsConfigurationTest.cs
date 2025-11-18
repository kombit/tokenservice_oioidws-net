using Digst.OioIdws.Common.TestUtils;

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
        }

        [TestMethod]
        public void Constructor_WithOboCertificate_SetsPropertyCorrectly()
        {
            // Arrange
            string stsEndpointAddress = "https://stsEndpointAddress";
            string stsEntityIdentifier = "https://stsEntityIdentifier";
            string cvr = "12345678";
            X509Certificate2 stsCertificate = Certificates.StsCertificate;

            // Act
            var sut = new StsConfiguration(stsEndpointAddress, stsEntityIdentifier, cvr, stsCertificate);
            sut.OboCertificate = stsCertificate;

            // Assert
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

            var other = new StsConfiguration(stsEndpointAddress, stsEntityIdentifier, cvr, stsCertificate)
            {
                OboCertificate = Certificates.TestCertificate
            };

            // Act
            var sut = new StsConfiguration(other);

            // Assert
            Assert.AreEqual(other.EndpointAddress, sut.EndpointAddress);
            Assert.AreEqual(other.EntityIdentifier, sut.EntityIdentifier);
            Assert.AreEqual(other.Cvr, sut.Cvr);
            Assert.AreSame(other.Certificate, sut.Certificate);
            Assert.AreSame(other.OboCertificate, sut.OboCertificate);
        }

        [TestMethod]
        [DataRow("", "https://stsEntityIdentifier", "12345678", DisplayName = "Empty endpoint address")]
        [DataRow("https://stsEndpointAddress", "", "12345678", DisplayName = "Empty entity identifier")]
        [DataRow("https://stsEndpointAddress", "https://stsEntityIdentifier", "", DisplayName = "Empty CVR")]
        [DataRow(null, "https://stsEntityIdentifier", "12345678", DisplayName = "Null endpoint address")]
        [DataRow("https://stsEndpointAddress", null, "12345678", DisplayName = "Null entity identifier")]
        [DataRow("https://stsEndpointAddress", "https://stsEntityIdentifier", null, DisplayName = "Null CVR")]
        public void Constructor_ThrowWhenInputParameterIsNullOrEmpty(string? stsEndpointAddress, string? stsEntityIdentifier, string? cvr)
        {
            // Arrange
            X509Certificate2 stsCertificate = Certificates.StsCertificate;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new StsConfiguration(stsEndpointAddress!, stsEntityIdentifier!, cvr!, stsCertificate));
        }

        [TestMethod]
        public void Constructor_ThrowWhenStsCertificateIsNull()
        {
            // Arrange
            string stsEndpointAddress = "https://stsEndpointAddress";
            string stsEntityIdentifier = "https://stsEntityIdentifier";
            string cvr = "12345678";

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new StsConfiguration(stsEndpointAddress, stsEntityIdentifier, cvr, null!));
        }

        [TestMethod]
        public void Constructor_CopyConstructor_ThrowsWhenOtherIsNull()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new StsConfiguration(null!));
        }
    }
}
