using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Digst.OioIdws.OioWsTrustCore
{
    /// <summary>
    /// Ported from .NET Framework's WCF code to handle requests/responses that use X509SecurityToken
    /// </summary>
    public sealed class X509SecurityToken : SecurityToken, IDisposable
    {
        private bool disposed = false;
        private readonly X509Certificate2 certificate;
        private SecurityKey? securityKey;

        public X509SecurityToken(X509Certificate2 certificate)
        {
            this.certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        }

        public X509Certificate2 Certificate
        {
            get
            {
                ThrowIfDisposed();
                return certificate;
            }
        }
        public override string Id
        {
            get
            {
                ThrowIfDisposed();
                return certificate.Thumbprint;
            }
        }

        public override string Issuer
        {
            get
            {
                ThrowIfDisposed();
                return certificate.Issuer;
            }
        }

        public override SecurityKey SecurityKey
        {
            get
            {
                ThrowIfDisposed();
                if (this.securityKey == null)
                {
                    this.securityKey = new X509SecurityKey(this.certificate);
                }
                return this.securityKey;
            }
        }
#nullable disable //This security token is used as an proxy OBO token, so SigningKey is not used 
        public override SecurityKey SigningKey { get; set; }
#nullable enable

        public override DateTime ValidFrom
        {
            get
            {
                ThrowIfDisposed();
                return Certificate.NotBefore.ToUniversalTime();
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                ThrowIfDisposed();
                return Certificate.NotAfter.ToUniversalTime();
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.Certificate.Reset();
            }
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("X509SecurityToken");
            }
        }
    }
}
