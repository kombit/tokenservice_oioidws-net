using Digst.OioIdws.Common.TestUtils;
using Digst.OioIdws.CommonCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class StsTokenServiceConfigurationTest
    {
        [TestMethod]
        public void StsTokenServiceConfiguration_Constructor_WithNonNullArguments()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
            var clientCertificate = Certificates.TestCertificate;

            var expectedCacheClockSkew = TimeSpan.FromSeconds(60);

            // Act
            var configuration = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, clientCertificate);

            // Assert
            Assert.AreSame(stsConfiguration, configuration.StsConfiguration);
            Assert.AreSame(wspConfiguration, configuration.WspConfiguration);
            Assert.AreSame(clientCertificate, configuration.ClientCertificate);
            Assert.AreEqual(expectedCacheClockSkew, configuration.CacheClockSkew);
        }

        [TestMethod]
        public void StsTokenServiceConfiguration_Constructor_WithNullStsConfiguration_ShouldThrowArgumentNullException()
        {
            // Arrange
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
            var clientCertificate = Certificates.TestCertificate;

            StsConfiguration? stsConfiguration = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new StsTokenServiceConfiguration(stsConfiguration!, wspConfiguration, clientCertificate));
        }

        [TestMethod]
        public void StsTokenServiceConfiguration_Constructor_WithNullWspConfiguration_ShouldThrowArgumentNullException()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var clientCertificate = Certificates.TestCertificate;
            WspConfiguration? wspConfiguration = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration!, clientCertificate));
        }

        [TestMethod]
        public void StsTokenServiceConfiguration_Constructor_WithNullClientCertificate_ShouldThrowArgumentNullException()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
            X509Certificate2? clientCertificate = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, clientCertificate!));
        }

        [TestMethod]
        public void StsTokenServiceConfiguration_CopyConstructor_CopiesValuesCorrectly()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
            var clientCertificate = Certificates.TestCertificate;

            var original = new StsTokenServiceConfiguration(stsConfiguration, wspConfiguration, clientCertificate)
            {
                TokenLifeTimeInMinutes = 60,
                MaxReceivedMessageSize = 1000000,
                SendTimeout = TimeSpan.FromSeconds(30),
                CacheClockSkew = TimeSpan.FromSeconds(120),
                IncludeLibertyHeader = true,
                ReplayAttackCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))
            };

            // Act
            var copy = new StsTokenServiceConfiguration(original);

            // Assert
            Assert.AreNotSame(original, copy);
            Assert.AreNotSame(original.StsConfiguration, copy.StsConfiguration);
            Assert.AreNotSame(original.WspConfiguration, copy.WspConfiguration);
            Assert.AreSame(original.ClientCertificate, copy.ClientCertificate);
            Assert.AreEqual(original.TokenLifeTimeInMinutes, copy.TokenLifeTimeInMinutes);
            Assert.AreEqual(original.MaxReceivedMessageSize, copy.MaxReceivedMessageSize);
            Assert.AreEqual(original.SendTimeout, copy.SendTimeout);
            Assert.AreEqual(original.CacheClockSkew, copy.CacheClockSkew);
            Assert.AreEqual(original.IncludeLibertyHeader, copy.IncludeLibertyHeader);
            Assert.AreSame(original.ReplayAttackCache, copy.ReplayAttackCache);
            Assert.IsNotNull(copy.StsCertificateAuthentication);
            Assert.IsNotNull(copy.WspCertificateAuthentication);
            Assert.IsNotNull(copy.SslCertificateAuthentication);
            Assert.AreNotSame(original.StsCertificateAuthentication, copy.StsCertificateAuthentication);
            Assert.AreNotSame(original.WspCertificateAuthentication, copy.WspCertificateAuthentication);
            Assert.AreNotSame(original.SslCertificateAuthentication, copy.SslCertificateAuthentication);

            var source = original.StsCertificateAuthentication;
            var result = copy.StsCertificateAuthentication;
            Assert.AreEqual(source.CertificateValidationMode, result.CertificateValidationMode);
            Assert.AreEqual(source.CustomCertificateValidator, result.CustomCertificateValidator);
            Assert.AreEqual(source.RevocationMode, result.RevocationMode);
            Assert.AreEqual(source.TrustedStoreLocation, result.TrustedStoreLocation);

            source = original.WspCertificateAuthentication;
            result = copy.WspCertificateAuthentication;
            Assert.AreEqual(source.CertificateValidationMode, result.CertificateValidationMode);
            Assert.AreEqual(source.CustomCertificateValidator, result.CustomCertificateValidator);
            Assert.AreEqual(source.RevocationMode, result.RevocationMode);
            Assert.AreEqual(source.TrustedStoreLocation, result.TrustedStoreLocation);

            source = original.SslCertificateAuthentication;
            result = copy.SslCertificateAuthentication;
            Assert.AreEqual(source.CertificateValidationMode, result.CertificateValidationMode);
            Assert.AreEqual(source.CustomCertificateValidator, result.CustomCertificateValidator);
            Assert.AreEqual(source.RevocationMode, result.RevocationMode);
            Assert.AreEqual(source.TrustedStoreLocation, result.TrustedStoreLocation);
        }

        [TestMethod]
        public void StsTokenServiceConfiguration_CopyConstructor_ThrowsArgumentNullExceptionWhenOtherIsNull()
        {
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new StsTokenServiceConfiguration(null!));
        }

        [TestMethod]
        public void ReplayAttackCache_Should_Not_Be_Null_By_Default()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
            var clientCertificate = Certificates.TestCertificate;

            var configuration = new StsTokenServiceConfiguration(
                stsConfiguration,
                wspConfiguration,
                clientCertificate);

            // Assert
            Assert.IsNotNull(configuration.ReplayAttackCache);
        }

        [TestMethod]
        public void Setting_Null_To_ReplayAttackCache_Should_Throw_Exception()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
            var clientCertificate = Certificates.TestCertificate;

            var configuration = new StsTokenServiceConfiguration(
                stsConfiguration,
                wspConfiguration,
                clientCertificate);

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => configuration.ReplayAttackCache = null!);
        }

        [TestMethod]
        public void Setting_Non_Null_Value_To_ReplayAttackCache_Should_Set_The_Value()
        {
            // Arrange
            var stsConfiguration = new StsConfiguration("testendpoint", "teststsid", "12345678", Certificates.StsCertificate);
            var wspConfiguration = new WspConfiguration("testwspendpoint", "testwspendpointid", EnvelopeVersion.Soap11, Certificates.TestCertificate);
            var clientCertificate = Certificates.TestCertificate;

            var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

            var configuration = new StsTokenServiceConfiguration(
                stsConfiguration,
                wspConfiguration,
                clientCertificate);

            // Act
            configuration.ReplayAttackCache = cache;

            // Assert
            Assert.AreSame(cache, configuration.ReplayAttackCache);
        }
    }
}
