using BusterWood.Caching;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    class ValueIsKey<TKey, TValue> : IDataSource<TKey, TValue>
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