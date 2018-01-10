using BusterWood.Caching;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class MixedReadWriteOverheadTests
    {
        ValueIsKey<string, string> valueIsKey = new ValueIsKey<string, string> {  };

        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void BitPseudoLru_cache_half(int items)
        {
            string[] keys = CreateKeyStrings(items);
            var pm = new PerformaceMonitor(start: true);

            var cache = new BitPseudoLruMap<string, string>(valueIsKey, items / 2);
            ReadMixKeys(keys, cache);

            pm.Stop();
            Console.WriteLine(pm.Report(items, cache.Count));
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void generational_cache_half(int items)
        {
            string[] keys = CreateKeyStrings(items);
            var pm = new PerformaceMonitor(start: true);

            var cache = valueIsKey.WithGenerationalCache(items / 4, null);
            ReadMixKeys(keys, cache);
            cache.Dispose();
            pm.Stop();
            Console.WriteLine(pm.Report(items, cache.Count));
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void generational_timed(int items)
        {
            string[] keys = CreateKeyStrings(items);
            var pm = new PerformaceMonitor(start: true);

            var cache = valueIsKey.WithGenerationalCache(null, TimeSpan.FromMilliseconds(100));
            ReadMixKeys(keys, cache);
            cache.Dispose();
            pm.Stop();
            Console.WriteLine(pm.Report(items, cache.Count));
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        private void ReadMixKeys(string[] keys, ICache<string, string> cache)
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i < tasks.Length; i++)
            {
                var count = keys.Length / 3;
                var offset = (keys.Length / 4) * i;
                tasks[i] = Task.Run(() => ReadMany(keys, cache, offset, count));
            }
            Task.WaitAll(tasks);
        }

        private void ReadMany(string[] keys, ICache<string, string> cache, int offset, int count)
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

        [TestCase(10000)]
        [TestCase(100000)]
        [TestCase(500000)]
        public void concurrent_dictionary_memory_overhead(int items)
        {
            string[] keys = CreateKeyStrings(items);
            var pm = new PerformaceMonitor(start: true);

            var cache = new ConcurrentDictionary<string, string>();
            ReadMixKeys(keys, cache);
            
            pm.Stop();
            Console.WriteLine(pm.Report(items, cache.Count));
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        private void ReadMixKeys(string[] keys, ConcurrentDictionary<string, string> cache)
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i < tasks.Length; i++)
            {
                var count = keys.Length / 3;
                var offset = (keys.Length / 4) * i;
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
