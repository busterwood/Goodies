using BusterWood.Caching;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class AsyncBatchReadThroughCacheTests
    {
        [Test]
        public async Task batch_load_reads_from_underlying_datasource_when_key_not_in_cache()
        {
            var cache = new BatchReadThroughCache<int, int>(new BatchValueIsKey<int, int>(), 10, null);
            var results = await cache.GetBatchAsync(new int[] { 2 });
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length, "number of results returned");
            Assert.AreEqual(2, results[0], "results[0]");
        }

        [Test]
        public async Task batch_load_reads_from_cache()
        {
            var cache = new BatchReadThroughCache<int, int>(new BatchValueIsKey<int, int>(), 10, null);
            Assert.AreEqual(2, cache[2]);
            Assert.AreEqual(1, cache.Count, "Count");
            var results = await cache.GetBatchAsync(new int[] { 2 });
            Assert.AreEqual(1, cache.Count, "no extra items added");
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length, "number of results returned");
            Assert.AreEqual(2, results[0], "results[0]");
        }

        [Test]
        public async Task batch_load_reads_from_cache_and_underlying_datasource()
        {
            var cache = new BatchReadThroughCache<int, int>(new BatchValueIsKey<int, int>(), 10, null);
            Assert.AreEqual(2, cache[2]);
            Assert.AreEqual(1, cache.Count, "Count");
            var results = await cache.GetBatchAsync(new int[] { 2, 3 });
            Assert.AreEqual(2, cache.Count, "no extra items added");
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Length, "number of results returned");
            Assert.AreEqual(2, results[0], "results[0]");
            Assert.AreEqual(3, results[1], "results[1]");
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
