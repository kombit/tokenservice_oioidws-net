using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Digst.OioIdws.Common.TestUtils;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class ReplayAttackCacheExtensionsTests
    {
        private IDistributedCache _cache = null!;

        [TestInitialize]
        public void Setup()
        {
            _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        }

        [TestMethod]
        public void DoesKeyExist_Returns_True_When_Key_Exists_In_DistributedCache()
        {
            // Arrange
            var key = "test_key";
            _cache.Set(key, new byte[] { 0x01, 0x02 });

            // Act
            var result = _cache.DoesKeyExist(key);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DoesKeyExist_Returns_False_When_Key_Does_Not_Exist_In_DistributedCache()
        {
            // Arrange
            var key = "nonexistent_key";

            // Act
            var result = _cache.DoesKeyExist(key);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DoesKeyExist_Returns_False_When_Key_Is_Empty()
        {
            // Act
            var result = _cache.DoesKeyExist(string.Empty);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Set_WithAbsoluteExpiry_StoresValueCorrectly()
        {
            // Arrange
            var key = "expiry_test_key";
            var absoluteExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5);

            // Act
            _cache.Set(key, absoluteExpiryTime);

            // Assert
            Assert.IsTrue(_cache.DoesKeyExist(key));
        }

        [TestMethod]
        public void Set_Throws_Exception_When_DistributedCache_Is_Null()
        {
            // Arrange
            IDistributedCache? distributedCache = null;
            var key = "test_key";
            var absoluteExpiryTime = DateTimeOffset.UtcNow;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => distributedCache!.Set(key, absoluteExpiryTime));
        }

        [TestMethod]
        public void DoesKeyExist_Throws_Exception_When_DistributedCache_Is_Null()
        {
            // Arrange
            IDistributedCache? distributedCache = null;
            var key = "test_key";

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => distributedCache!.DoesKeyExist(key));
        }

        [TestMethod]
        public void DoesKeyExist_Throws_Exception_When_Key_Is_Null()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => _cache.DoesKeyExist(null!));
        }

        [TestMethod]
        public void Set_Throws_Exception_When_Key_Is_Null()
        {
            // Arrange
            var absoluteExpiryTime = DateTimeOffset.UtcNow;

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => _cache.Set(null!, absoluteExpiryTime));
        }
    }
}
