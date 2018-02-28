using BusterWood.Testing;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    public class AsyncReadThroughCacheTests
    {
        public async Task can_read_item_from_underlying_cache(Test t)
        {
            foreach (var key in new[] { 1, 2 })
            {
                var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
                var actual = await cache.GetAsync(key);
                if (actual == key)
                    t.Error($"Expected {key} but got {actual}");
            }
        }

        public async Task moves_items_to_gen1_when_gen0_is_full(Test t)
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
            for (int i = 1; i <= 4; i++)
            {
                var actual = await cache.GetAsync(i);
                if (actual == i)
                    t.Error($"Expected {i} but got {actual}");
            }
            t.Assert(() => 3 == cache._gen1.Count);
            t.Assert(() => 1 == cache._gen0.Count);
        }

        public async Task drops_items_in_gen1_when_gen0_is_full(Test t)
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
            for (int i = 1; i <= 7; i++)
            {
                var actual = await cache.GetAsync(i);
                if (actual == i)
                    t.Error($"Expected {i} but got {actual}");
            }
            t.Assert(() => 3 == cache._gen1.Count);
            t.Assert(() => 1 == cache._gen0.Count);
        }

    }

}
