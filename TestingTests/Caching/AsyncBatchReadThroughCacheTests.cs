using BusterWood.Testing;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BusterWood.Caching
{
    public class AsyncBatchReadThroughCacheTests
    {
        public static async Task batch_load_reads_from_underlying_datasource_when_key_not_in_cache(Test t)
        {
            var cache = new BatchReadThroughCache<int, int>(new BatchValueIsKey<int, int>(), 10, null);
            var results = await cache.GetBatchAsync(new int[] { 2 });
            t.Assert(() => results != null);
            t.Assert(() => 1 == results.Length);
            t.Assert(() => 2 == results[0]);
        }

        public static async Task batch_load_reads_from_cache(Test t)
        {
            var cache = new BatchReadThroughCache<int, int>(new BatchValueIsKey<int, int>(), 10, null);
            t.Assert(() => 2 == cache[2]);
            t.Assert(() => 1 == cache.Count);
            var results = await cache.GetBatchAsync(new int[] { 2 });
            t.Assert(() => 1 == cache.Count);
            t.Assert(() => results != null);
            t.Assert(() => 1 == results.Length);
            t.Assert(() => 2 == results[0]);
        }

        public static async Task batch_load_reads_from_cache_and_underlying_datasource(Test t)
        {
            var cache = new BatchReadThroughCache<int, int>(new BatchValueIsKey<int, int>(), 10, null);
            t.Assert(() => 2 == cache[2]);
            t.Assert(() => 1 == cache.Count);
            var results = await cache.GetBatchAsync(new int[] { 2, 3 });
            t.Assert(() => 2 == cache.Count);
            t.Assert(() => results != null);
            t.Assert(() => 2 == results.Length);
            t.Assert(() => 2 == results[0]);
            t.Assert(() => 3 == results[1]);
        }
    }

    class BatchValueIsKey<TKey, TValue> : ValueIsKey<TKey, TValue>, IBatchDataSource<TKey, TValue>
        where TValue : TKey
    {
        public TValue[] GetBatch(IReadOnlyCollection<TKey> keys)
        {
            var results = new TValue[keys.Count];
            int i = 0;
            foreach (var k in keys)
            {
                results[i++] = (TValue)k;
            }
            return results;
        }

        public Task<TValue[]> GetBatchAsync(IReadOnlyCollection<TKey> keys)
        {
            return Task.FromResult(GetBatch(keys));
        }

    }
}
