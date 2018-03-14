using BusterWood.Testing;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    public class MixedReadWriteOverheadTests
    {
        ValueIsKey<string, string> valueIsKey = new ValueIsKey<string, string> {  };
        readonly int[] testSizes;

        public MixedReadWriteOverheadTests(Test t)
        {
            testSizes = (Tests.Short) ? new[] { 10000 } : new[] { 10000, 100000, 500000 };
            t.Verbose = true;
        }

        public void BitPseudoLru_cache_half(Test t)
        {
            foreach (var items in testSizes)
            {
                string[] keys = CreateKeyStrings(items);
                var pm = new PerformaceMonitor(start: true);

                var cache = new BitPseudoLruMap<string, string>(valueIsKey, items / 2);
                ReadMixKeys(keys, cache, t);

                pm.Stop();
                t.Log($"BitPseudoLru_cache_half of {items} items {pm.Stop()}");
                GC.KeepAlive(cache);
                GC.KeepAlive(keys);

            }
        }

        public void generational_cache_half(Test t)
        {
            foreach (var items in testSizes)
            {
                string[] keys = CreateKeyStrings(items);
                var pm = new PerformaceMonitor(start: true);

                var cache = valueIsKey.WithGenerationalCache(items / 4, null);
                ReadMixKeys(keys, cache, t);
                cache.Dispose();
                pm.Stop();
                t.Log($"generational_cache_half of {items} items {pm.Stop()}");
                GC.KeepAlive(cache);
                GC.KeepAlive(keys);
            }
        }

        public void generational_timed(Test t)
        {
            foreach (var items in testSizes)
            {
                string[] keys = CreateKeyStrings(items);
                var pm = new PerformaceMonitor(start: true);

                var cache = valueIsKey.WithGenerationalCache(null, TimeSpan.FromMilliseconds(100));
                ReadMixKeys(keys, cache, t);
                cache.Dispose();
                pm.Stop();
                t.Log($"generational_timed of {items} items {pm.Stop()}");
                GC.KeepAlive(cache);
                GC.KeepAlive(keys);
            }
        }

        private void ReadMixKeys(string[] keys, ICache<string, string> cache, Test t)
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i < tasks.Length; i++)
            {
                var count = keys.Length / 3;
                var offset = (keys.Length / 4) * i;
                tasks[i] = Task.Run(() => ReadMany(keys, cache, offset, count, t));
            }
            Task.WaitAll(tasks);
        }

        private void ReadMany(string[] keys, ICache<string, string> cache, int offset, int count, Test t)
        {
            for (int i = 0; i < count; i++)
            {
                var index = (offset + i) % keys.Length;
                var key = keys[index];
                var read = cache[key];
                if (read != key)
                    t.Assert(() => key == read);
            }
        }

        public void concurrent_dictionary_memory_overhead(Test t)
        {
            foreach (var items in testSizes)
            {
                string[] keys = CreateKeyStrings(items);
                var pm = new PerformaceMonitor(start: true);

                var cache = new ConcurrentDictionary<string, string>();
                ReadMixKeys(keys, cache, t);

                pm.Stop();
                t.Log($"concurrent_dictionary_memory_overhead of {items} items {pm.Stop()}");
                GC.KeepAlive(cache);
                GC.KeepAlive(keys);
            }
        }

        private void ReadMixKeys(string[] keys, ConcurrentDictionary<string, string> cache, Test t)
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i < tasks.Length; i++)
            {
                var count = keys.Length / 3;
                var offset = (keys.Length / 4) * i;
                tasks[i] = Task.Run(() => ReadMany(keys, cache, offset, count, t));
            }
            Task.WaitAll(tasks);
        }

        private void ReadMany(string[] keys, ConcurrentDictionary<string, string> cache, int offset, int count, Test t)
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
                    t.Assert(() => key == read);
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
