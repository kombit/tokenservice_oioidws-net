using Digst.OioIdws.Common.TestUtils;
using Microsoft.IdentityModel.Tokens;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class CertificateValidatorTest
    {
        [TestMethod]
        // Note that this test may contains other errors related to revocation checking OR root CA not being trusted (if the Ps script is not run before running the test)   
        // This test is only to verify that the exception contains the expected text about the certificate being expired
        public void ValidateAExpiredCertificate_ChainTrust_WillThrowException()
        {
            // Arrange
            var certificate = Certificates.ExpiredCertificate;
            var sut = X509CertificateValidatorFactory.ChainTrust;

            // Act
            var exception = Assert.ThrowsExactly<Microsoft.IdentityModel.Tokens.SecurityTokenValidationException>(() => sut.Validate(certificate));

            // Assert
            Assert.Contains("Validating certificate C=DK, OID.2.5.4.97=NTRDK-90892939, O=Testorganisation nr. 90892939, SERIALNUMBER=UI:DK-O:G:f1eed326-bca5-4133-88a6-a639b66aaa54, CN=hhp-test-ch-demo-17may; 11DD0FC0BC0B0CEE24DCB02AFE3D985124AB14B4 failed.", exception.Message);
            Assert.Contains("A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.", exception.Message);
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
            var exception = Assert.ThrowsExactly<Microsoft.IdentityModel.Tokens.SecurityTokenValidationException>(() => sut.Validate(certificate));

            // Assert
            Assert.Contains("Validating certificate C=DK, OID.2.5.4.97=NTRDK-90892939, O=Testorganisation nr. 90892939, SERIALNUMBER=UI:DK-O:G:f1eed326-bca5-4133-88a6-a639b66aaa54, CN=hhp-test-ch-demo-17may; 11DD0FC0BC0B0CEE24DCB02AFE3D985124AB14B4 failed.", exception.Message);
            Assert.Contains("A required certificate is not within its validity period when verifying against the current system clock or the timestamp in the signed file.", exception.Message);
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
            var exception = Assert.ThrowsExactly<Microsoft.IdentityModel.Tokens.SecurityTokenValidationException>(() => sut.Validate(certificate));

            // Assert
            Assert.Contains("Validating certificate C=DK, OID.2.5.4.97=NTRDK-92273980, O=Testorganisation nr. 92273980, SERIALNUMBER=UI:DK-O:G:e77d87a5-9f75-4aee-b139-e15eafed0894, CN=usercert-02-revoked; 64305D1145DC3572CD88B54084C06DE0E1A638D3 failed.", exception.Message);
            Assert.Contains("The certificate is revoked.", exception.Message);
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
            var exception = Assert.ThrowsExactly<Microsoft.IdentityModel.Tokens.SecurityTokenValidationException>(() => sut.Validate(certificate));

            // Assert
            Assert.Contains("Validating certificate C=DK, OID.2.5.4.97=NTRDK-92273980, O=Testorganisation nr. 92273980, SERIALNUMBER=UI:DK-O:G:e77d87a5-9f75-4aee-b139-e15eafed0894, CN=usercert-02-revoked; 64305D1145DC3572CD88B54084C06DE0E1A638D3 failed.", exception.Message);
            Assert.Contains("The certificate is revoked.", exception.Message);
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
            Assert.ThrowsExactly<ArgumentNullException>(() => validator.Validate(null));
        }

        [TestMethod]
        public void PeerTrustValidator_Validate_ThrowsArgumentNullException_WhenCertificateIsNull()
        {
            // Arrange
            var validator = X509CertificateValidatorFactory.PeerTrust;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => validator.Validate(null));
        }

        [TestMethod]
        public void ChainTrustValidator_Validate_ThrowsArgumentNullException_WhenCertificateIsNull()
        {
            // Arrange
            var validator = X509CertificateValidatorFactory.ChainTrust;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => validator.Validate(null));
        }

        [TestMethod]
        public void PeerOrChainTrustValidator_Validate_ThrowsArgumentNullException_WhenCertificateIsNull()
        {
            // Arrange
            var validator = X509CertificateValidatorFactory.PeerOrChainTrust;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => validator.Validate(null));
        }
    }
}
