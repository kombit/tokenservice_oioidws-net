using Digst.OioIdws.CommonCore;
using Digst.OioIdws.Common.TestUtils;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class CertificateValidatorTest
    {
        [TestMethod]
        public void ValidateAExpiredCertificate_ChainTrust_WillThrowException()
        {
            // Arrange
            var certificate = Certificates.ExpiredCertificate;
            var sut = X509CertificateValidatorFactory.ChainTrust;

            // Act
            var exception = Assert.ThrowsException<SecurityTokenValidationException>(() => sut.Validate(certificate));

            // Assert
            StringAssert.Contains(exception.Message, "Validating certificate SERIALNUMBER=CVR:30808460-UID:25351738 + CN=NETS DANID A/S - TU VOCES gyldig, O=NETS DANID A/S // CVR:30808460, C=DK; E85AC4E2A5ED9CE4CCFEF42F3F9DE1E4A521CDDD failed. Replace the certificate or change the certificateValidationMode. A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.");
        }

        [TestMethod]
        public void ValidateAExpiredCertificate_CustomPolicyValidator_WillThrowException()
        {
            // Arrange
            var certificate = Certificates.ExpiredCertificate;
            var sut = X509CertificateValidatorFactory.CreateChainTrustValidator(new X509ChainPolicy
            {
                RevocationFlag = X509RevocationFlag.EntireChain,
                RevocationMode = X509RevocationMode.NoCheck
            });

            // Act
            var exception = Assert.ThrowsException<SecurityTokenValidationException>(() => sut.Validate(certificate));

            // Assert
            StringAssert.Contains(exception.Message, "Validating certificate SERIALNUMBER=CVR:30808460-UID:25351738 + CN=NETS DANID A/S - TU VOCES gyldig, O=NETS DANID A/S // CVR:30808460, C=DK; E85AC4E2A5ED9CE4CCFEF42F3F9DE1E4A521CDDD failed. Replace the certificate or change the certificateValidationMode. A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.");
        }

        [TestMethod]
        public void NoneX509CertificateValidationMode_ExpiredCertificate_WillNotValidateTheCertificate()
        {
            // Arrange
            var certificate = Certificates.ExpiredCertificate;
            var sut = X509CertificateValidatorFactory.None;

            // Act
            sut.Validate(certificate);

            // Assert
            // Implicitly means no exception. Otherwise, this test will fail
        }

        [TestMethod]
        public void NoneX509CertificateValidationMode_RevokedCertificate_WillNotValidateTheCertificate()
        {
            // Arrange
            var certificate = Certificates.RevokeCertificate;
            var sut = X509CertificateValidatorFactory.None;

            // Act
            sut.Validate(certificate);

            // Assert
            // Implicitly means no exception. Otherwise, this test will fail
        }

        [TestMethod]
        public void ValidateARevokedCertificate_ChainTrust_WillThrowException()
        {
            // Arrange
            var certificate = Certificates.RevokeCertificate;
            var sut = X509CertificateValidatorFactory.ChainTrust;

            // Act
            var exception = Assert.ThrowsException<SecurityTokenValidationException>(() => sut.Validate(certificate));

            // Assert
            StringAssert.Contains(exception.Message, "Validating certificate SERIALNUMBER=CVR:30808460-FID:87102701 + CN=TU GENEREL FOCES spærret (funktionscertifikat), O=NETS DANID A/S // CVR:30808460, C=DK; A93DF35F7A5E7AF9E2F2737DB2163C5D3716819F failed. Replace the certificate or change the certificateValidationMode. The certificate is revoked.");
        }

        [TestMethod]
        public void ValidateARevokedCertificate_PeerTrust_WillNotThrow()
        {
            // Arrange
            var certificate = Certificates.RevokeCertificate;
            var sut = X509CertificateValidatorFactory.PeerTrust;

            // Act
            sut.Validate(certificate);

            // Assert
        }

        [TestMethod]
        public void ValidateARevokedCertificate_PeerOrChainTrust_WillNotThrow()
        {
            // Arrange
            var certificate = Certificates.RevokeCertificate;
            var sut = X509CertificateValidatorFactory.PeerOrChainTrust;

            // Act
            sut.Validate(certificate);

            // Assert
        }

        [TestMethod]
        public void ValidateAValidCertificate_ChainTrust_WillNotThrow()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;
            var sut = X509CertificateValidatorFactory.ChainTrust;

            // Act
            sut.Validate(certificate);

            // Assert
            // Implicitly means no exception. Otherwise, this test will fail
        }

        [TestMethod]
        public void ValidateAValidCertificate_PeerTrust_WillNotThrow()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;
            var sut = X509CertificateValidatorFactory.PeerTrust;

            // Act
            sut.Validate(certificate);

            // Assert
            // Implicitly means no exception. Otherwise, this test will fail
        }

        [TestMethod]
        public void ValidateAValidCertificate_PeerOrChainTrust_WillNotThrow()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;
            var sut = X509CertificateValidatorFactory.PeerOrChainTrust;

            // Act
            sut.Validate(certificate);

            // Assert
            // Implicitly means no exception. Otherwise, this test will fail
        }

        [TestMethod]
        public void ValidateARevokedCertificate_CustomPolicyValidator_WillThrowException()
        {
            // Arrange
            var certificate = Certificates.RevokeCertificate;
            var sut = X509CertificateValidatorFactory.CreateChainTrustValidator(new X509ChainPolicy
            {
                RevocationFlag = X509RevocationFlag.EndCertificateOnly,
                RevocationMode = X509RevocationMode.Online
            });

            // Act
            var exception = Assert.ThrowsException<SecurityTokenValidationException>(() => sut.Validate(certificate));

            // Assert
            StringAssert.Contains(exception.Message, "Validating certificate SERIALNUMBER=CVR:30808460-FID:87102701 + CN=TU GENEREL FOCES spærret (funktionscertifikat), O=NETS DANID A/S // CVR:30808460, C=DK; A93DF35F7A5E7AF9E2F2737DB2163C5D3716819F failed. Replace the certificate or change the certificateValidationMode. The certificate is revoked.");
        }

        [TestMethod]
        public void ValidateAValidCertificate_CustomPolicyValidator_WillNotThrow()
        {
            // Arrange
            var certificate = Certificates.TestCertificate;
            var sut = X509CertificateValidatorFactory.CreateChainTrustValidator(new X509ChainPolicy
            {
                RevocationFlag = X509RevocationFlag.EndCertificateOnly,
                RevocationMode = X509RevocationMode.Online
            });

            // Act
            sut.Validate(certificate);

            // Assert
            // Implicitly means no exception. Otherwise, this test will fail
        }

        [TestMethod]
        public void NoneX509CertificateValidator_Validate_ThrowsArgumentNullException_WhenCertificateIsNull()
        {
            // Arrange
            var validator = X509CertificateValidatorFactory.None;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => validator.Validate(null));
        }

        [TestMethod]
        public void PeerTrustValidator_Validate_ThrowsArgumentNullException_WhenCertificateIsNull()
        {
            // Arrange
            var validator = X509CertificateValidatorFactory.PeerTrust;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => validator.Validate(null));
        }

        [TestMethod]
        public void ChainTrustValidator_Validate_ThrowsArgumentNullException_WhenCertificateIsNull()
        {
            // Arrange
            var validator = X509CertificateValidatorFactory.ChainTrust;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => validator.Validate(null));
        }

        [TestMethod]
        public void PeerOrChainTrustValidator_Validate_ThrowsArgumentNullException_WhenCertificateIsNull()
        {
            // Arrange
            var validator = X509CertificateValidatorFactory.PeerOrChainTrust;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => validator.Validate(null));
        }
    }
}
