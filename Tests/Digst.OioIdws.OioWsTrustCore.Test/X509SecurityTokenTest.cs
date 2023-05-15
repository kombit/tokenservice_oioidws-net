using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.Common.TestUtils;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class X509SecurityTokenTest
    {
        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void X509SecurityTokenTests()
        {
            var certificate = Certificates.StsCertificate;
            var x509SecurityToken = new X509SecurityToken(certificate);

            Assert.IsTrue(x509SecurityToken.SecurityKey is X509SecurityKey);
            Assert.AreEqual(certificate.Issuer, x509SecurityToken.Issuer);
            Assert.AreEqual(certificate.Thumbprint, x509SecurityToken.Id);
            Assert.AreEqual(certificate.NotBefore.ToUniversalTime(), x509SecurityToken.ValidFrom);
            Assert.AreEqual(certificate.NotAfter.ToUniversalTime(), x509SecurityToken.ValidTo);
        }
    }
}
