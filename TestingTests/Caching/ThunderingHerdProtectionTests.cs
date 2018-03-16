using BusterWood.Testing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    public class ThunderingHerdProtectionTests
    {
        public static void get_reads_though(Test t)
        {
            var source = new HerdValueIsKey<int, int>();
            var thp = source.WithThunderingHerdProtection();
            var c = new ReadThroughCache<int, int>(thp, 10, null);
            t.Assert(1, c[1]);
        }
    }

    class HerdValueIsKey<TKey, TValue> : IDataSource<TKey, TValue>
        where TValue : TKey
    {
        public int SpinWaitCount;
        public TimeSpan SleepFor;
        private int ConcurrentGets;

        public int HitCount { get; set; }

        public TValue this[TKey key]
        {
            get
            {
                var gets = Interlocked.Increment(ref ConcurrentGets);
                if (gets > 1)
                    throw new Exception("Concurrency of more than one");
                if (SpinWaitCount > 0)
                    Thread.SpinWait(SpinWaitCount);
                if (SleepFor > TimeSpan.Zero)
                    Thread.Sleep(SleepFor + TimeSpan.FromMilliseconds(gets - 1));
                HitCount++;
                Interlocked.Decrement(ref ConcurrentGets);
                return (TValue)key;
            }
        }

        public Task<TValue> GetAsync(TKey key) => Task.FromResult((TValue)key);

    }

}
