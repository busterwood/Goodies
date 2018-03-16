using BusterWood.Testing;
using System.Collections.Generic;

namespace BusterWood.Caching
{
    public class ReadThroughCacheTests
    {
        public static void can_read_item_from_underlying_cache(Test t)
        {
            foreach (var key in new[] { 1,2 })
            {
                var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
                t.Assert(key, cache[key]);
            }
        }

        public static void moves_items_to_gen1_when_gen0_is_full(Test t)
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
            for(int i = 1; i <= 4; i++)
            {
                t.Assert(i, cache[i]);
            }
            t.Assert(3, cache._gen1.Count);
            t.Assert(1, cache._gen0.Count);
        }

        public static void drops_items_in_gen1_when_gen0_is_full(Test t)
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 3, null);
            for(int i = 1; i <= 7; i++)
            {
                t.Assert(i, cache[i]);
            }
            t.Assert(3, cache._gen1.Count);
            t.Assert(1, cache._gen0.Count);
        }

        public static void invalidate_removes_item_from_gen0(Test t)
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 10, null);
            t.Assert(1, cache[1]);
            t.Assert(1, cache.Count);
            cache.Remove(1);
            t.Assert(0, cache.Count);
        }

        public static void invalidate_removes_item_from_gen1(Test t)
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 10, null);
            t.Assert(1, cache[1]);
            t.Assert(1, cache.Count);
            cache.ForceCollect();
            cache.Remove(1);
            t.Assert(0, cache.Count);
        }

        public static void eviction_raises_event_with_evicted_items(Test t)
        {
            var cache = new ReadThroughCache<int, int>(new ValueIsKey<int, int>(), 10, null);
            IReadOnlyDictionary<int, int> evicted = null;
            t.Assert(1, cache[1]);
            cache.Evicted += (sender, key) => evicted = key;
            cache.Clear();
            t.Assert(1, evicted.Count);
            t.Assert(0, cache.Count);
        }

    }

}
