using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.OioWsTrustCore.Utils;
using Digst.OioIdws.Common.TestUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class TrustSignatureTests
    {
        /// <summary>
        /// Tests that the XML signature can be verified.
        /// </summary>
        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void SignatureTest()
        {
            // Arrange
            var rtsSoapMessageNotSigned = XDocument.Load(@"Resources\RST_Not_Signed.xml");
            var idOfElementsThatMustBeSigned = new List<string>
            {
                IdValue.ActionIdValue,
                IdValue.MessageIdIdValue,
                IdValue.ToIdValue,
                IdValue.RelatesToIdValue,
                IdValue.TimeStampIdValue,
                IdValue.BinarySecurityTokenIdValue,
                IdValue.BodyIdValue
            };
            var cert = Certificates.TestCertificate;

            // Act
            var rtsSoapMessageSigned = XmlSignatureUtils.SignDocument(rtsSoapMessageNotSigned, idOfElementsThatMustBeSigned, cert);

            // Assert
            Assert.IsTrue(XmlSignatureUtils.VerifySignature(rtsSoapMessageSigned, cert));
        }

        /// <summary>
        /// Tests that our <see cref="XmlSignatureUtils.VerifySignature"/> works with the RSTR comming from the STS. Response has been taken by using Fiddler.
        /// </summary>
        [TestMethod]
        [TestCategory(Constants.UnitTest)]
        public void SignatureResponseTest()
        {
            // Arrange
            var rtsrSoapMessageSigned = XDocument.Load(@"Resources\Response25022015.xml");

            var cert = new X509Certificate2(Convert.FromBase64String("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"));

            // Act
            var verified = XmlSignatureUtils.VerifySignature(rtsrSoapMessageSigned, cert);

            // Assert
            Assert.IsTrue(verified);
        }
    }
}
