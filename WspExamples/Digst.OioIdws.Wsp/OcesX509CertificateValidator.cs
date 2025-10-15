using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
namespace Digst.OioIdws.Wsp
{
    public class OcesX509CertificateValidator : X509CertificateValidator
    {
        public override void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }
            if (certificate.IssuerName.Name.Contains("Den Danske Stat OCES"))
                return;
            X509CertificateValidator.ChainTrust.Validate(certificate);
        }
    }
}