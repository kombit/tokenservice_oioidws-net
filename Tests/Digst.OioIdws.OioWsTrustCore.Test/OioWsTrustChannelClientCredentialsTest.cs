using Digst.OioIdws.OioWsTrustCore;
using System.ServiceModel.Description;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    [TestClass]
    public class OioWsTrustChannelClientCredentialsTest
    {
        [TestMethod]
        public void CreateSecurityTokenManager_Without_ClientCredentials()
        {
            // Setup

            var sut = new OioWsTrustChannelClientCredentials();

            // Act

            var tokenManager = sut.CreateSecurityTokenManager();

            // Assert
            Assert.IsNotNull(tokenManager);
        }

        [TestMethod]
        public void CreateSecurityTokenManager_With_ClientCredentials()
        {
            // Setup
            var clientCredentials = new ClientCredentials();
            var sut = new OioWsTrustChannelClientCredentials(clientCredentials);

            // Act

            var tokenManager = sut.CreateSecurityTokenManager() as OioWsTrustChannelSecurityTokenManager;

            // Assert
            Assert.IsNotNull(tokenManager);
            Assert.IsNotNull(tokenManager.ClientCredentials);
        }
    }
}