using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class ReplayAttackCacheExtensionsTests
    {
        [TestMethod]
        public void DoesKeyExist_Returns_True_When_Key_Exists_In_DistributedCache()
        {
            // Arrange
            var key = "test_key";
            var replayAttackCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            replayAttackCache.Set(key, new byte[] { 0x01, 0x02 });

            // Act
            var result = replayAttackCache.DoesKeyExist(key);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DoesKeyExist_Returns_False_When_Key_Does_Not_Exist_In_DistributedCache()
        {
            // Arrange
            var key = "test_key";
            var replayAttackCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

            // Act
            var result = replayAttackCache.DoesKeyExist(key);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Set_Throws_Exception_When_DistributedCache_Is_Null()
        {
            // Arrange
            IDistributedCache? distributedCache = null;
            var key = "test_key";
            var absoluteExpiryTime = DateTimeOffset.UtcNow;

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => distributedCache.Set(key, absoluteExpiryTime));

            // Assert (expected exception)
        }

        [TestMethod]
        public void DoesKeyExist_Throws_Exception_When_DistributedCache_Is_Null()
        {
            // Arrange
            IDistributedCache? distributedCache = null;
            var key = "test_key";

            // Act
            Assert.ThrowsException<ArgumentNullException>(() => distributedCache.DoesKeyExist(key));

            // Assert (expected exception)
        }
    }
}
