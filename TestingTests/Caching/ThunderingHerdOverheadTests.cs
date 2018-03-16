using BusterWood.Testing;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    public class ThunderingHerdOverheadTests
    {
        ValueIsKey<string, string> valueIsKey;
        string[] keys = CreateKeyStrings(100);
        PerformaceMonitor pm;

        public ThunderingHerdOverheadTests(Test t)
        {
            if (Tests.Short)
                t.Skip();
            valueIsKey = new ValueIsKey<string, string> { SleepFor=TimeSpan.FromMilliseconds(10) };
            pm = new PerformaceMonitor(start: true);
        }

        public void BitPseudoLru_cache_half(Test t)
        {
            var cache = new BitPseudoLruMap<string, string>(valueIsKey.WithThunderingHerdProtection(), keys.Length / 2);
            ReadMixKeys(keys, cache, t);

            Console.WriteLine($"BitPseudoLru half {valueIsKey.HitCount} hits to underlying data source, {pm.Stop()}");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        public void generational_cache_half(Test t)
        {
            var cache = valueIsKey.WithThunderingHerdProtection().WithGenerationalCache(keys.Length / 4, null);
            ReadMixKeys(keys, cache, t);
            cache.Dispose();
            Console.WriteLine($"Generational half {valueIsKey.HitCount} hits to underlying data source, {pm.Stop()}");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        public void generational_timed(Test t)
        {
            var cache = valueIsKey.WithThunderingHerdProtection().WithGenerationalCache(null, TimeSpan.FromSeconds(5));
            ReadMixKeys(keys, cache, t);
            cache.Dispose();
            Console.WriteLine($"Generational timed {valueIsKey.HitCount} hits to underlying data source, {pm.Stop()}");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        static void ReadMixKeys(string[] keys, ICache<string, string> cache, Test t)
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i < tasks.Length; i++)
            {
                var count = keys.Length;
                var offset = 0; // (keys.Length / 4) * i;
                tasks[i] = Task.Run(() => ReadMany(keys, cache, offset, count, t));
            }
            Task.WaitAll(tasks);
        }

        static void ReadMany(string[] keys, ICache<string, string> cache, int offset, int count, Test t)
        {
            for (int i = 0; i < count; i++)
            {
                var index = (offset + i) % keys.Length;
                var key = keys[index];
                var read = cache[key];
                if (read != key)
                    t.Assert(key, read);
            }
        }
        
        public void concurrent_dictionary_memory_overhead(Test t)
        {
            var cache = new ConcurrentDictionary<string, string>();
            ReadMixKeys(keys, cache, t);
            
            pm.Stop();
            Console.WriteLine($"concurrent dictionary {valueIsKey.HitCount} hits to underlying data source, {pm.Stop()}");
            GC.KeepAlive(cache);
            GC.KeepAlive(keys);
        }

        private void ReadMixKeys(string[] keys, ConcurrentDictionary<string, string> cache, Test t)
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i < tasks.Length; i++)
            {
                var count = keys.Length;
                var offset = 0; //  (keys.Length / 4) * i;
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
                    t.Assert(key, read);
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
