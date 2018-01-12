using BusterWood.Caching;
using NUnit.Framework;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class AsyncReadThroughCacheTests
    {
        [TestCase(1)]
        [TestCase(2)]
        public async Task can_read_item_from_underlying_cache(int key)
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
            Assert.AreEqual(key, await cache.GetAsync(key));
        }

        [Test]
        public async Task moves_items_to_gen1_when_gen0_is_full()
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
            for (int i = 1; i <= 4; i++)
            {
                Assert.AreEqual(i, await cache.GetAsync(i));
            }
            Assert.AreEqual(3, cache._gen1.Count, "gen1.Count");
            Assert.AreEqual(1, cache._gen0.Count, "gen0.Count");
        }

        [Test]
        public async Task drops_items_in_gen1_when_gen0_is_full()
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
            for (int i = 1; i <= 7; i++)
            {
                Assert.AreEqual(i, await cache.GetAsync(i));
            }
            Assert.AreEqual(3, cache._gen1.Count, "gen1.Count");
            Assert.AreEqual(1, cache._gen0.Count, "gen0.Count");
        }

    }

}
