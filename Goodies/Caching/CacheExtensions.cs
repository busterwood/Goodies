using System;
using System.Collections.Generic;

namespace BusterWood.Caching
{
    public static class CacheExtensions
    {
         /// <summary>Create a new read-through cache that has a Gen0 size limit and/or a periodic collection time</summary>
        /// <param name="cache">The underlying cache to load data from</param>
        /// <param name="gen0Limit">(Optional) limit on the number of items allowed in Gen0 before a collection</param>
        /// <param name="timeToLive">(Optional) time period after which a unread item is evicted from the cache</param>
        public static ReadThroughCache<TKey, TValue> WithGenerationalCache<TKey, TValue>(this IDataSource<TKey, TValue> cache, int? gen0Limit, TimeSpan? timeToLive)
        {
            return new ReadThroughCache<TKey, TValue>(cache, gen0Limit, timeToLive);
        }

        /// <summary>
        /// Adds <see cref="ThunderingHerdProtection{TKey, TValue}"/> to a cache which prevents 
        /// calling the data source concurrently *for the same key*.
        /// </summary>
        /// <param name="cache">The underlying cache to load data from</param>
        public static ThunderingHerdProtection<TKey, TValue> WithThunderingHerdProtection<TKey, TValue>(this IDataSource<TKey, TValue> cache)
        {
            return new ThunderingHerdProtection<TKey, TValue>(cache);
        }

        public static void AddOrUpdateRange<TKey, TValue>(this ICache<TKey, TValue> cache, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            lock (cache.SyncRoot)
            {
                foreach (var p in pairs)
                {
                    cache[p.Key] = p.Value;
                }
            }
        }

        public static void RemoveRange<TKey, TValue>(this ICache<TKey, TValue> cache, IEnumerable<TKey> keys)
        {
            lock(cache.SyncRoot)
            {
                foreach (var k in keys)
                {
                    cache.Remove(k);
                }
            }
        }
    }
}