using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Digst.OioIdws.CommonCore
{
    /// <summary>
    /// This class is a port of https://github.com/dotnet/wcf/blob/main/src/System.ServiceModel.Primitives/src/System/IdentityModel/Selectors/X509CertificateValidator.cs
    /// This port is necessary because:
    ///   1. .NET validators don't take revocation status into account
    ///   2. They are not exposed to .NET Core client code, so we cannot use them to validate STS' and WSP's certificates
    ///   
    /// Because it is a port, coding style is kept as close to original source as possible
    /// 
    /// License information from the original source:
    /// 
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    // See the LICENSE file in the project root for more information.
    /// </summary>
    public static class X509CertificateValidatorFactory
    {
        private static X509CertificateValidator? s_peerTrust;
        private static X509CertificateValidator? s_chainTrust;
        private static X509CertificateValidator? s_peerOrChainTrust;
        private static X509CertificateValidator? s_none;

        /// <summary>
        /// This validator does not do any validation
        /// </summary>
        public static X509CertificateValidator None
        {
            get
            {
                if (s_none == null)
                {
                    s_none = new NoneX509CertificateValidator();
                }

                return s_none;
            }
        }

        /// <summary>
        /// A certificate is valid if it is trusted in certificate stores
        /// Important note: PeerTrust does not take certificate revocation check into account
        /// </summary>
        public static X509CertificateValidator PeerTrust
        {
            get
            {
                if (s_peerTrust == null)
                {
                    s_peerTrust = new PeerTrustValidator();
                }

                return s_peerTrust;
            }
        }

        /// <summary>
        /// Validates the whole certificate chain trust without specifying a chain policy
        /// </summary>
        public static X509CertificateValidator ChainTrust
        {
            get
            {
                if (s_chainTrust == null)
                {
                    s_chainTrust = new ChainTrustValidator();
                }

                return s_chainTrust;
            }
        }

        /// <summary>
        /// This built-in validator returns immediately if the PeerTrust check passes, which does not take certificate revocation check into account
        /// </summary>
        public static X509CertificateValidator PeerOrChainTrust
        {
            get
            {
                if (s_peerOrChainTrust == null)
                {
                    s_peerOrChainTrust = new PeerOrChainTrustValidator();
                }

                return s_peerOrChainTrust;
            }
        }

        /// <summary>
        /// Create a new ChainTrustValidator using the input chain policy
        /// </summary>
        /// <param name="chainPolicy">The policy to use</param>
        /// <returns>A ChainTrustValidator object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static X509CertificateValidator CreateChainTrustValidator(X509ChainPolicy chainPolicy)
        {
            if (chainPolicy == null)
            {
                throw new ArgumentNullException(nameof(chainPolicy));
            }

            return new ChainTrustValidator(chainPolicy);
        }

        /// <summary>
        /// Create a new PeerOrChainTrustValidator object using the input chain policy
        /// </summary>
        /// <param name="chainPolicy">The policy to use</param>
        /// <returns>A ChainTrustValidator object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static X509CertificateValidator CreatePeerOrChainTrustValidator(X509ChainPolicy chainPolicy)
        {
            if (chainPolicy == null)
            {
                throw new ArgumentNullException(nameof(chainPolicy));
            }

            return new PeerOrChainTrustValidator(chainPolicy);
        }

        private sealed class NoneX509CertificateValidator : X509CertificateValidator
        {
            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                {
                    throw new ArgumentNullException(nameof(certificate));
                }
            }
        }

        private sealed class PeerTrustValidator : X509CertificateValidator
        {
            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                {
                    throw new ArgumentNullException(nameof(certificate));
                }

                if (!TryValidate(certificate, out Exception? exception))
                {
                    throw exception!;
                }
            }

            private static bool StoreContainsCertificate(StoreName storeName, X509Certificate2 certificate)
            {
                X509Store store = new X509Store(storeName, StoreLocation.CurrentUser);
                X509Certificate2Collection? certificates = null;
                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    certificates = store.Certificates.Find(X509FindType.FindByThumbprint, certificate.Thumbprint, false);
                    return certificates.Count > 0;
                }
                finally
                {
                    SecurityUtils.ResetAllCertificates(certificates);
                    store.Dispose();
                }
            }

            internal bool TryValidate(X509Certificate2 certificate, out Exception? exception)
            {
                DateTime now = DateTime.Now;

                if (now > certificate.NotAfter || now < certificate.NotBefore)
                {
                    exception = new SecurityTokenValidationException($"The X.509 certificate ({SecurityUtils.GetCertificateId(certificate)}) usage time is invalid.  The usage time '{now}' does not fall between NotBefore time '{certificate.NotBefore}' and NotAfter time '{certificate.NotAfter}'.");

                    return false;
                }

                if (!StoreContainsCertificate(StoreName.TrustedPeople, certificate))
                {
                    exception = new SecurityTokenValidationException($"The X.509 certificate {SecurityUtils.GetCertificateId(certificate)} is not in the trusted people store.");
                    return false;
                }

                if (StoreContainsCertificate(StoreName.Disallowed, certificate))
                {
                    exception = new SecurityTokenValidationException($"The {SecurityUtils.GetCertificateId(certificate)} X.509 certificate is in an untrusted certificate store.");

                    return false;
                }

                exception = null;
                return true;
            }


        }

        private sealed class ChainTrustValidator : X509CertificateValidator
        {
            private readonly X509ChainPolicy? _chainPolicy;

            public ChainTrustValidator()
            {
                _chainPolicy = null;
            }

            public ChainTrustValidator(X509ChainPolicy chainPolicy)
            {
                _chainPolicy = chainPolicy;
            }

            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                {
                    throw new ArgumentNullException(nameof(certificate));
                }

                // implies _useMachineContext = false
                // ctor for X509Chain(_useMachineContext, _chainPolicyOID) not present in CoreCLR
                X509Chain chain = new X509Chain();

                if (_chainPolicy != null)
                {
                    _chainPolicy.VerificationTime = DateTime.Now;
                    chain.ChainPolicy = _chainPolicy;
                }

                bool buildChainResult = chain.Build(certificate);
                string chainStatusInformation = GetChainStatusInformation(chain.ChainStatus);

                if (!buildChainResult || !string.IsNullOrEmpty(chainStatusInformation))
                {
                    throw new SecurityTokenValidationException($"Validating certificate {SecurityUtils.GetCertificateId(certificate)} failed. Replace the certificate or change the certificateValidationMode. {GetChainStatusInformation(chain.ChainStatus)}");
                }
            }

            private static string GetChainStatusInformation(X509ChainStatus[] chainStatus)
            {
                if (chainStatus != null)
                {
                    StringBuilder error = new StringBuilder(128);
                    for (int i = 0; i < chainStatus.Length; ++i)
                    {
                        error.Append(chainStatus[i].StatusInformation);
                        error.Append(" ");
                    }
                    return error.ToString();
                }
                return String.Empty;
            }
        }

        private sealed class PeerOrChainTrustValidator : X509CertificateValidator
        {
            private readonly X509CertificateValidator _chain;
            private readonly PeerTrustValidator _peer;

            public PeerOrChainTrustValidator()
            {
                _chain = ChainTrust;
                _peer = (PeerTrustValidator)PeerTrust;
            }

            public PeerOrChainTrustValidator(X509ChainPolicy chainPolicy)
            {
                _chain = CreateChainTrustValidator(chainPolicy);
                _peer = (PeerTrustValidator)PeerTrust;
            }

            public override void Validate(X509Certificate2 certificate)
            {
                if (certificate == null)
                {
                    throw new ArgumentNullException(nameof(certificate));
                }

                if (_peer.TryValidate(certificate, out Exception? exception))
                {
                    return;
                }

                try
                {
                    _chain.Validate(certificate);
                }
                catch (SecurityTokenValidationException ex)
                {
                    throw new SecurityTokenValidationException(exception!.Message + " " + ex.Message, ex);
                }
            }
        }

        private static class SecurityUtils
        {
            internal static string GetCertificateId(X509Certificate2 certificate)
            {
                StringBuilder str = new StringBuilder(256);
                AppendCertificateIdentityName(str, certificate);
                return str.ToString();
            }

            internal static void AppendCertificateIdentityName(StringBuilder str, X509Certificate2 certificate)
            {
                string value = certificate.SubjectName.Name;
                if (String.IsNullOrEmpty(value))
                {
                    value = certificate.GetNameInfo(X509NameType.DnsName, false);
                    if (String.IsNullOrEmpty(value))
                    {
                        value = certificate.GetNameInfo(X509NameType.SimpleName, false);
                        if (String.IsNullOrEmpty(value))
                        {
                            value = certificate.GetNameInfo(X509NameType.EmailName, false);
                            if (String.IsNullOrEmpty(value))
                            {
                                value = certificate.GetNameInfo(X509NameType.UpnName, false);
                            }
                        }
                    }
                }
                // Same format as X509Identity
                str.Append(String.IsNullOrEmpty(value) ? "<x509>" : value);
                str.Append("; ");
                str.Append(certificate.Thumbprint);
            }

            // This is the workaround, Since store.Certificates returns a full collection
            // of certs in store.  These are holding native resources.
            internal static void ResetAllCertificates(X509Certificate2Collection? certificates)
            {
                if (certificates != null)
                {
                    for (int i = 0; i < certificates.Count; ++i)
                    {
                        ResetCertificate(certificates[i]);
                    }
                }
            }

            internal static void ResetCertificate(X509Certificate2 certificate)
            {
                // Check that Dispose() and Reset() do the same thing
                certificate.Dispose();
            }
        }
    }
}
