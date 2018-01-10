using BusterWood.Caching;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class WriteOnlyOverheadTests
    {
        ValueIsKey<string, string> valueIsKey = new ValueIsKey<string, string>();

        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void BitPseudoLru_cache_half(int items)
        {
            var sw = new Stopwatch();
            string[] keys = CreateKeyStrings(items);
            var starting = GC.GetTotalMemory(true);
            sw.Start();
            var cache = new BitPseudoLruMap<string, string>(valueIsKey, items / 2);
            foreach (var key in keys)
            {
                Assert.AreEqual(key, cache[key]);
            }
            sw.Stop();
            var allocated = GC.GetTotalMemory(false) - starting;
            var held = GC.GetTotalMemory(true) - starting;
            Console.WriteLine($"Took {sw.ElapsedMilliseconds:N0} ms, {items} added to cache, {cache.Count} item in the cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)items:N2} bytes");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void generational_cache_half(int items)
        {
            var sw = new Stopwatch();
            string[] keys = CreateKeyStrings(items);
            var starting = GC.GetTotalMemory(true);
            sw.Start();
            var cache = new ReadThroughCache<string, string>(valueIsKey, items / 4, null);
            foreach (var key in keys)
            {
                Assert.AreEqual(key, cache[key]);
            }
            sw.Stop();
            var allocated = GC.GetTotalMemory(false) - starting;
            var held = GC.GetTotalMemory(true) - starting;
            Console.WriteLine($"Took {sw.ElapsedMilliseconds:N0} ms, {items} added to cache, {cache._gen0.Count + cache._gen1.Count} item in the cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)items:N2} bytes");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }


        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void BitPseudoLru_cache_memory_overhead(int items)
        {
            var sw = new Stopwatch();
            string[] keys = CreateKeyStrings(items);
            var starting = GC.GetTotalMemory(true);
            sw.Start();
            var cache = new BitPseudoLruMap<string, string>(valueIsKey, items);
            foreach (var key in keys)
            {
                Assert.AreEqual(key, cache[key]);
            }
            sw.Stop();
            var allocated = GC.GetTotalMemory(false) - starting;
            var held = GC.GetTotalMemory(true) - starting;
            Console.WriteLine($"Took {sw.ElapsedMilliseconds:N0} ms, {items} added to cache, {cache.Count} item in the cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)items:N2} bytes");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void generational_cache_memory_overhead(int items)
        {
            var sw = new Stopwatch();
            string[] keys = CreateKeyStrings(items);
            var starting = GC.GetTotalMemory(true);
            sw.Start();
            var cache = new ReadThroughCache<string, string>(valueIsKey, items / 2, null);
            foreach (var key in keys)
            {
                Assert.AreEqual(key, cache[key]);
            }
            sw.Stop();
            var allocated = GC.GetTotalMemory(false) - starting;
            var held = GC.GetTotalMemory(true) - starting;
            Console.WriteLine($"Took {sw.ElapsedMilliseconds:N0} ms, {items} added to cache, {cache._gen0.Count + cache._gen1.Count} item in the cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)items:N2} bytes");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void time_generational_cache_memory_overhead(int items)
        {
            var sw = new Stopwatch();
            string[] keys = CreateKeyStrings(items);
            var starting = GC.GetTotalMemory(true);
            sw.Start();
            var cache = new ReadThroughCache<string, string>(valueIsKey, null, TimeSpan.FromMilliseconds(200));
            foreach (var key in keys)
            {
                Assert.AreEqual(key, cache[key]);
            }
            sw.Stop();
            var allocated = GC.GetTotalMemory(false) - starting;
            var held = GC.GetTotalMemory(true) - starting;
            Console.WriteLine($"Took {sw.ElapsedMilliseconds:N0} ms, {items} added to cache, {cache._gen0.Count + (cache._gen1?.Count).GetValueOrDefault()} item in the cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)items:N2} bytes");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
            cache.Dispose(); // stop the underlying task
        }

        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void system_caching_memory_overhead(int items)
        {
            var sw = new Stopwatch();
            string[] keys = CreateKeyStrings(items);
            var starting = GC.GetTotalMemory(true);
            sw.Start();

            var cache = new MemoryCache("test");
            foreach (var key in keys)
            {
                var got = cache.Get(key);
                if (got == null)
                {
                    got = valueIsKey[key];
                    cache.Add(key, got, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(10) });
                }
            }

            sw.Stop();
            var allocated = GC.GetTotalMemory(false) - starting;
            var held = GC.GetTotalMemory(true) - starting;
            Console.WriteLine($"Took {sw.ElapsedMilliseconds:N0} ms, {items} added to cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)items:N2} bytes");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void simple_dictionary_memory_overhead(int items)
        {
            var sw = new Stopwatch();
            string[] keys = CreateKeyStrings(items);
            var starting = GC.GetTotalMemory(true);
            sw.Start();

            var cache = new Dictionary<string, string>();
            foreach (var key in keys)
            {
                lock(cache)
                {
                    string got;
                    if (!cache.TryGetValue(key, out got))
                    {
                        got = valueIsKey[key];
                        cache.Add(key, key);
                    }
                }
            }

            sw.Stop();
            var allocated = GC.GetTotalMemory(false) - starting;
            var held = GC.GetTotalMemory(true) - starting;
            Console.WriteLine($"Took {sw.ElapsedMilliseconds:N0} ms, {items} added to cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)items:N2} bytes");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void concurrent_dictionary_memory_overhead(int items)
        {
            var sw = new Stopwatch();
            string[] keys = CreateKeyStrings(items);
            var starting = GC.GetTotalMemory(true);
            sw.Start();

            var cache = new ConcurrentDictionary<string, string>();
            foreach (var key in keys)
            {
                string got;
                if (!cache.TryGetValue(key, out got))
                {
                    got = valueIsKey[key];
                    cache.TryAdd(key, key);
                }
            }

            sw.Stop();
            var allocated = GC.GetTotalMemory(false) - starting;
            var held = GC.GetTotalMemory(true) - starting;
            Console.WriteLine($"Took {sw.ElapsedMilliseconds:N0} ms, {items} added to cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)items:N2} bytes");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        private static string[] CreateKeyStrings(int items)
        {
            string[] keys = new string[items];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = (i + 1).ToString();
            }
            return keys;
        }

    }

}
