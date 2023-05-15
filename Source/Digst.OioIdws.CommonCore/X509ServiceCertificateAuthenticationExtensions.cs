using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;

namespace Digst.OioIdws.CommonCore
{
    public static class X509ServiceCertificateAuthenticationExtensions
    {
        /// <summary>
        /// Creates a new instance of X509ServiceCertificateAuthentication by copying the values of the properties from the source instance.
        /// </summary>
        /// <param name="source">The source instance to copy.</param>
        /// <returns>A new instance of X509ServiceCertificateAuthentication with copied property values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the source instance is null.</exception>
        public static X509ServiceCertificateAuthentication DeepClone(this X509ServiceCertificateAuthentication source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = source.CertificateValidationMode,
                CustomCertificateValidator = source.CustomCertificateValidator,
                RevocationMode = source.RevocationMode,
                TrustedStoreLocation = source.TrustedStoreLocation
            };
        }

        /// <summary>
        /// Copies the values of the properties from the source instance to the destination instance.
        /// </summary>
        /// <param name="destination">The destination instance to copy to.</param>
        /// <param name="source">The source instance to copy from.</param>
        /// <exception cref="ArgumentNullException">Thrown when either the source or destination instances are null.</exception>
        public static void CopyFrom(this X509ServiceCertificateAuthentication destination, X509ServiceCertificateAuthentication source)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            destination.CertificateValidationMode = source.CertificateValidationMode;
            destination.CustomCertificateValidator = source.CustomCertificateValidator;
            destination.RevocationMode = source.RevocationMode;
            destination.TrustedStoreLocation = source.TrustedStoreLocation;
        }

        /// <summary>
        /// Validates an X509Certificate2 certificate using the certificate validator obtained from the X509ServiceCertificateAuthentication instance.
        /// </summary>
        /// <param name="source">The X509ServiceCertificateAuthentication instance to obtain the validator from.</param>
        /// <param name="certificate">The X509Certificate2 certificate to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when either the source instance or certificate is null.</exception>
        public static void Validate(this X509ServiceCertificateAuthentication source, X509Certificate2 certificate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            X509CertificateValidator validator = source.GetCertificateValidator();
            validator.Validate(certificate);
        }

        /// <summary>
        /// Returns an X509CertificateValidator object based on the CertificateValidationMode property of the source instance.
        /// </summary>
        /// <param name="source">The X509ServiceCertificateAuthentication instance to obtain the validator from.</param>
        /// <returns>An X509CertificateValidator object based on the CertificateValidationMode property of the source instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the CertificateValidationMode property is Custom and the CustomCertificateValidator property is null.</exception>
        public static X509CertificateValidator GetCertificateValidator(this X509ServiceCertificateAuthentication source)
        {
            X509CertificateValidator? result;
            if (!source.TryGetCertificateValidator(out result) && source.CustomCertificateValidator == null)
            {
                throw new InvalidOperationException("X509CertificateValidationMode.Custom requires a CustomCertificateValidator. Specify the CustomCertificateValidator property.");
            }
            return result!;
        }

        /// <summary>
        /// Used internally by the GetCertificateValidator method to obtain the X509CertificateValidator object based on the CertificateValidationMode property.
        /// </summary>
        /// <param name="source">The X509ServiceCertificateAuthentication instance to obtain the validator from.</param>
        /// <param name="validator">The obtained X509CertificateValidator object if successful.</param>
        /// <returns>True if the validator object was obtained successfully, and false otherwise.</returns>
        internal static bool TryGetCertificateValidator(this X509ServiceCertificateAuthentication source, out X509CertificateValidator? validator)
        {
            if (source.CertificateValidationMode == X509CertificateValidationMode.None)
            {
                validator = X509CertificateValidatorFactory.None;
            }
            else if (source.CertificateValidationMode == X509CertificateValidationMode.PeerTrust)
            {
                validator = X509CertificateValidatorFactory.PeerTrust;
            }
            else if (source.CertificateValidationMode == X509CertificateValidationMode.Custom)
            {
                validator = source.CustomCertificateValidator;
            }
            else
            {
                X509ChainPolicy chainPolicy = new X509ChainPolicy
                {
                    RevocationMode = source.RevocationMode
                };
                if (source.CertificateValidationMode == X509CertificateValidationMode.ChainTrust)
                {
                    validator = X509CertificateValidatorFactory.CreateChainTrustValidator(chainPolicy);
                }
                else
                {
                    validator = X509CertificateValidatorFactory.CreatePeerOrChainTrustValidator(chainPolicy);
                }
            }
            return validator != null;
        }
    }
}
