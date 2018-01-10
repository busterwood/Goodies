using BusterWood.Caching;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class ThunderingHerdOverheadTests
    {
        ValueIsKey<string, string> valueIsKey;
        string[] keys = CreateKeyStrings(100);

        [SetUp]
        public void Setup()
        {
            valueIsKey = new ValueIsKey<string, string> { SleepFor=TimeSpan.FromMilliseconds(10) };
        }

        [TestCase(-1)]
        public void BitPseudoLru_cache_half(int x)
        {
            var pm = new PerformaceMonitor(start: true);

            var cache = new BitPseudoLruMap<string, string>(valueIsKey.WithThunderingHerdProtection(), keys.Length / 2);
            ReadMixKeys(keys, cache);

            pm.Stop();
            Console.WriteLine(pm.Report(keys.Length, cache.Count) + $", {valueIsKey.HitCount} hits to underlying data source");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(-1)]
        public void generational_cache_half(int x)
        {
            var pm = new PerformaceMonitor(start: true);

            var cache = valueIsKey.WithThunderingHerdProtection().WithGenerationalCache(keys.Length / 4, null);
            ReadMixKeys(keys, cache);
            cache.Dispose();
            pm.Stop();
            Console.WriteLine(pm.Report(keys.Length, cache.Count) + $", {valueIsKey.HitCount} hits to underlying data source");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(-1)]
        public void generational_timed(int x)
        {
            var pm = new PerformaceMonitor(start: true);

            var cache = valueIsKey.WithThunderingHerdProtection().WithGenerationalCache(null, TimeSpan.FromSeconds(5));
            ReadMixKeys(keys, cache);
            cache.Dispose();
            pm.Stop();
            Console.WriteLine(pm.Report(keys.Length, cache.Count) + $", {valueIsKey.HitCount} hits to underlying data source");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        static void ReadMixKeys(string[] keys, ICache<string, string> cache)
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i < tasks.Length; i++)
            {
                var count = keys.Length;
                var offset = 0; // (keys.Length / 4) * i;
                tasks[i] = Task.Run(() => ReadMany(keys, cache, offset, count));
            }
            Task.WaitAll(tasks);
        }

        static void ReadMany(string[] keys, ICache<string, string> cache, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var index = (offset + i) % keys.Length;
                var key = keys[index];
                var read = cache[key];
                if (read != key)
                    Assert.AreEqual(key, read);
            }
        }

        [TestCase(-1)]
        public void concurrent_dictionary_memory_overhead(int x)
        {
            var pm = new PerformaceMonitor(start: true);

            var cache = new ConcurrentDictionary<string, string>();
            ReadMixKeys(keys, cache);
            
            pm.Stop();
            Console.WriteLine(pm.Report(keys.Length, cache.Count) + $", {valueIsKey.HitCount} hits to underlying data source");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        private void ReadMixKeys(string[] keys, ConcurrentDictionary<string, string> cache)
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i < tasks.Length; i++)
            {
                var count = keys.Length;
                var offset = 0; //  (keys.Length / 4) * i;
                tasks[i] = Task.Run(() => ReadMany(keys, cache, offset, count));
            }
            Task.WaitAll(tasks);
        }

        private void ReadMany(string[] keys, ConcurrentDictionary<string, string> cache, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var index = (offset + i) % keys.Length;
                var key = keys[index];

                string read;
                if (!cache.TryGetValue(key, out read))
                {
                    read = valueIsKey[key];
                    cache.TryAdd(key, key);
                }

                if (read != key)
                    Assert.AreEqual(key, read);
            }
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
