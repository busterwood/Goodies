using System;
using BusterWood.Testing;

namespace BusterWood.Caching
{
    public class ConcurrentCacheTests
    {        
        public static void empty_cache_has_count_of_zero(Test t)
        {
            var c = NewCache();
            t.Assert(0, c.Count);
        }
        
        public static void can_add_to_cache_and_read_back(Test t)
        {
            var c = NewCache();
            c[2] = "hello";
            t.Assert("hello", c[2]);
            t.Assert(1, c.Count);
        }
        
        public static void can_replace_existing_value(Test t)
        {
            var c = NewCache();
            c[2] = "hello";
            c[2] = "world";
            t.Assert("world", c[2]);
            t.Assert(1, c.Count);
        }
        
        public static void can_invalidate(Test t)
        {
            var c = NewCache();
            c[2] = "hello";
            t.Assert(1, c.Count);
            c.Remove(2);
            t.Assert(0, c.Count);
            t.Assert(null, c[2]);
        }
        
        public static void can_add_and_readback_after_first_collection(Test t)
        {
            var c = NewCache();
            c[2] = "hello";
            c.ForceCollect();
            t.Assert("hello", c[2]);
            t.Assert(1, c.Count);
        }
        
        public static void evicted_after_two_collections(Test t)
        {
            var c = NewCache();
            c[2] = "hello";
            c.ForceCollect();
            c.ForceCollect();
            t.Assert(null, c[2]);
            t.Assert(0, c.Count);
        }

        static ConcurrentCache<int, string> NewCache(int gen0 = 10, TimeSpan? ttl = null) => new ConcurrentCache<int, string>(gen0, ttl);

    }
}
