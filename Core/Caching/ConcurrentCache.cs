using System;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    /// <summary>A concurrent version of the <see cref="Cache{TKey, TValue}"/> that uses a number of smaller caches to provide concurrent read and modification</summary>
    public class ConcurrentCache<TKey, TValue> : ICache<TKey, TValue>
    {
        readonly Cache<TKey, TValue>[] _partitions;
        readonly int _partitionCount;

        public event EvictionHandler<TKey, TValue> Evicted;

        /// <summary>Create a new cache that has a Gen0 size limit and/or a periodic collection time</summary>
        /// <param name="gen0Limit">(Optional) limit on the number of items allowed in Gen0 before a collection</param>
        /// <param name="timeToLive">(Optional) time period after which a unread item is evicted from the cache</param>
        /// <param name="partitions">The number of paritions to split the cache into, defaults to <see cref="Environment.ProcessorCount"/></param>
        public ConcurrentCache(int? gen0Limit, TimeSpan? timeToLive, int partitions = 0)
        {
            if (partitions == 0)
                partitions = Environment.ProcessorCount;
            else if (partitions < 1)
                throw new ArgumentOutOfRangeException(nameof(partitions), partitions, "Must be one or more");

            _partitionCount = partitions;
            _partitions = new Cache<TKey, TValue>[partitions];
            for (int i = 0; i < partitions; i++)
            {
                _partitions[i] = new Cache<TKey, TValue>(gen0Limit / partitions, timeToLive);
                _partitions[i].Evicted += (sender, args) => Evicted(sender, args);
            }
        }

        public object SyncRoot => throw new NotImplementedException();

        internal void ForceCollect()
        {
            foreach (var p in _partitions)
            {
                p.ForceCollect();
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (var p in _partitions)
                {
                    count += p.Count;
                }
                return count;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int idx = PartitionIndex(key);
                return _partitions[idx][key];
            }
            set
            {
                int idx = PartitionIndex(key);
                _partitions[idx][key] = value;
            }
        }

        public void Clear()
        {
            foreach (var part in _partitions)
            {
                part.Clear();
            }
        }

        public Task<TValue> GetAsync(TKey key) => Task.FromResult(this[key]);

        /// <summary>Removes a <param name="key" /> (and value) from the cache, if it exists.</summary>
        public void Remove(TKey key)
        {
            int idx = PartitionIndex(key);
            _partitions[idx].Remove(key);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        int PartitionIndex(TKey key)
        {
            int positiveHashCode = key.GetHashCode() & ~int.MinValue;
            // the following optimizes the modulus (IDIV) instruction for the common cases, as IDIV takes 60-80 clock cycles
            if (_partitionCount == 1)
                return 0;
            if (_partitionCount == 2)
                return positiveHashCode & 3;
            if (_partitionCount == 4)
                return positiveHashCode & 7;
            if (_partitionCount == 8)
                return positiveHashCode & 15;
            if (_partitionCount == 16)
                return positiveHashCode & 31;
            if (_partitionCount == 32)
                return positiveHashCode & 63;
            else
                return positiveHashCode % _partitionCount;            
        }

    }
}
