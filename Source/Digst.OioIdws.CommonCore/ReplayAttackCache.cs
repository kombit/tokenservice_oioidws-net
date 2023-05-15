using Microsoft.Extensions.Caching.Distributed;
using System;

namespace Digst.OioIdws.CommonCore
{
    /// <summary>
    ///  Class for holding information about previously received responses.
    ///  Could be optimized to use the configured clock screw instead of being hard coded to the default clock screw of 5 minutes.
    /// </summary>
    public static class ReplayAttackCacheExtensions
    {
        private static readonly byte[] _emptyCacheItem = Array.Empty<byte>();

        /// <summary>
        /// Add an item to the cache.
        /// </summary>
        /// <param name="key">The key should be the signature value of the incoming response.</param>
        /// <param name="absoluteExpiryTime">Items in the cache must be set to the expiry time of the response. This prevents the cache from growing more than necessary.</param>
        public static void Set(this IDistributedCache distributedCache, string key, DateTimeOffset absoluteExpiryTime)
        {
            if (distributedCache == null)
                throw new ArgumentNullException(nameof(distributedCache));

            // Add some clock screw in order for allow servers to be a little out of sync ... ideally it should always be the same as the configured WCF clock screw. Default is 5 minutes and hence 5 minutes are used.
            var dateTimeOffset = absoluteExpiryTime.AddMinutes(5);
            distributedCache.Set(key, _emptyCacheItem, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = dateTimeOffset
            });
        }

        /// <summary>
        /// Used for checking if a response is replayed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DoesKeyExist(this IDistributedCache distributedCache, string key)
        {
            if (distributedCache == null)
                throw new ArgumentNullException(nameof(distributedCache));

            return distributedCache.Get(key) != null;
        }
    }
}
