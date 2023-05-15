using Digst.OioIdws.Common.TestUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class X509ServiceCertificateAuthenticationExtensionsTest
    {
        [TestMethod]
        public void DeepClone_Should_Return_Clone_Of_Source_Object()
        {
            var customCertificateValidator = X509CertificateValidatorFactory.PeerOrChainTrust;

            // Arrange
            var source = new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = X509CertificateValidationMode.PeerTrust,
                CustomCertificateValidator = customCertificateValidator,
                RevocationMode = X509RevocationMode.NoCheck,
                TrustedStoreLocation = StoreLocation.LocalMachine
            };

            // Act
            var result = source.DeepClone();

            // Assert
            Assert.AreEqual(source.CertificateValidationMode, result.CertificateValidationMode);
            Assert.AreEqual(source.CustomCertificateValidator, result.CustomCertificateValidator);
            Assert.AreEqual(source.RevocationMode, result.RevocationMode);
            Assert.AreEqual(source.TrustedStoreLocation, result.TrustedStoreLocation);
        }

        [TestMethod]
        public void DeepClone_Should_Throw_ArgumentNullException_If_Source_Is_Null()
        {
            // Arrange
            X509ServiceCertificateAuthentication source = null;

            // Act + Assert
            Assert.ThrowsException<ArgumentNullException>(() => source.DeepClone());
        }

        [TestMethod]
        public void CopyFrom_NullDestination_ThrowsArgumentNullException()
        {
            // Arrange
            X509ServiceCertificateAuthentication destination = null;
            X509ServiceCertificateAuthentication source = new X509ServiceCertificateAuthentication();

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => destination.CopyFrom(source));
        }

        [TestMethod]
        public void CopyFrom_NullSource_ThrowsArgumentNullException()
        {
            // Arrange
            X509ServiceCertificateAuthentication destination = new X509ServiceCertificateAuthentication();
            X509ServiceCertificateAuthentication source = null;

            // Act and Assert
            Assert.ThrowsException<ArgumentNullException>(() => destination.CopyFrom(source));
        }

        [TestMethod]
        public void CopyFrom_CopiesPropertiesCorrectly()
        {
            // Arrange
            var customCertificateValidator = X509CertificateValidatorFactory.PeerOrChainTrust;

            X509ServiceCertificateAuthentication destination = new X509ServiceCertificateAuthentication();
            X509ServiceCertificateAuthentication source = new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = X509CertificateValidationMode.PeerTrust,
                CustomCertificateValidator = customCertificateValidator,
                RevocationMode = X509RevocationMode.Online,
                TrustedStoreLocation = StoreLocation.LocalMachine
            };

            // Act
            destination.CopyFrom(source);

            // Assert
            Assert.AreEqual(source.CertificateValidationMode, destination.CertificateValidationMode);
            Assert.AreEqual(source.CustomCertificateValidator, destination.CustomCertificateValidator);
            Assert.AreEqual(source.RevocationMode, destination.RevocationMode);
            Assert.AreEqual(source.TrustedStoreLocation, destination.TrustedStoreLocation);
        }

        [TestMethod]
        public void Validate_NullSource_ThrowsArgumentNullException()
        {
            // Arrange
            X509ServiceCertificateAuthentication source = null;
            X509Certificate2 certificate = new X509Certificate2();

            // Act + Assert
            Assert.ThrowsException<ArgumentNullException>(() => source.Validate(certificate));
        }

        [TestMethod]
        public void Validate_NullCertificate_ThrowsArgumentNullException()
        {
            // Arrange
            X509ServiceCertificateAuthentication source = new X509ServiceCertificateAuthentication();
            X509Certificate2 certificate = null;

            // Act + Assert
            Assert.ThrowsException<ArgumentNullException>(() => source.Validate(certificate));
        }

        [TestMethod]
        public void Validate_CertificateValidated_Success()
        {
            // Arrange
            X509ServiceCertificateAuthentication source = new X509ServiceCertificateAuthentication();
            X509Certificate2 certificate = Certificates.TestCertificate;

            // Act + Assert
            source.Validate(certificate);
        }

        [TestMethod]
        public void TryGetCertificateValidator_ReturnsNull_ForCertificateValidationModeNone()
        {
            // Arrange
            var source = new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = X509CertificateValidationMode.None
            };

            // Act
            bool result = source.TryGetCertificateValidator(out X509CertificateValidator? validator);

            // Assert
            Assert.IsTrue(result);
            Assert.IsInstanceOfType(validator, X509CertificateValidatorFactory.None.GetType());
        }

        [TestMethod]
        public void TryGetCertificateValidator_ReturnsPeerTrustValidator_ForCertificateValidationModePeerTrust()
        {
            // Arrange
            var source = new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = X509CertificateValidationMode.PeerTrust
            };

            // Act
            bool result = source.TryGetCertificateValidator(out X509CertificateValidator? validator);

            // Assert
            Assert.IsTrue(result);
            Assert.IsInstanceOfType(validator, X509CertificateValidatorFactory.PeerTrust.GetType());
        }

        [TestMethod]
        public void TryGetCertificateValidator_ReturnsCustomValidator_ForCertificateValidationModeCustom()
        {
            // Arrange
            var customValidator = X509CertificateValidatorFactory.PeerTrust;
            var source = new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = X509CertificateValidationMode.Custom,
                CustomCertificateValidator = customValidator
            };

            // Act
            bool result = source.TryGetCertificateValidator(out X509CertificateValidator? validator);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(customValidator, validator);
        }

        [TestMethod]
        public void TryGetCertificateValidator_ThrowsInvalidOperationException_ForCertificateValidationModeCustom_WhenCustomCertificateValidatorIsNull()
        {
            // Arrange
            var source = new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = X509CertificateValidationMode.Custom,
                CustomCertificateValidator = null
            };

            // Act + Assert
            Assert.ThrowsException<InvalidOperationException>(() => source.GetCertificateValidator());
        }

        [TestMethod]
        public void TryGetCertificateValidator_ReturnsChainTrustValidator_ForCertificateValidationModeChainTrust()
        {
            // Arrange
            var source = new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = X509CertificateValidationMode.ChainTrust,
                RevocationMode = X509RevocationMode.Online
            };

            // Act
            bool result = source.TryGetCertificateValidator(out X509CertificateValidator? validator);

            // Assert
            Assert.IsTrue(result);
            Assert.IsInstanceOfType(validator, X509CertificateValidatorFactory.ChainTrust.GetType());
        }

        [TestMethod]
        public void TryGetCertificateValidator_ReturnsPeerOrChainTrustValidator_ForCertificateValidationModePeerOrChainTrust()
        {
            // Arrange
            var source = new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust,
                RevocationMode = X509RevocationMode.Online
            };

            // Act
            bool result = source.TryGetCertificateValidator(out X509CertificateValidator? validator);

            // Assert
            Assert.IsTrue(result);
            Assert.IsInstanceOfType(validator, X509CertificateValidatorFactory.PeerOrChainTrust.GetType());
        }
    }
}
