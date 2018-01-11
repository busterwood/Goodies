using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    /// <summary>
    /// A cache map that uses generations to cache to minimize the per-key overhead.
    /// A collection releases all items in Gen1 and moves Gen0 -> Gen1.  Reading an item in Gen1 promotes the item back to Gen0.
    /// </summary>
    /// <remarks>This version REMEMBERS cache misses</remarks>
    public class BatchReadThroughCache<TKey, TValue> : ReadThroughCache<TKey, TValue>, IBatchCache<TKey, TValue>
    {
        static readonly IEqualityComparer<TValue> ValueEquality = EqualityComparer<TValue>.Default;
        readonly IBatchDataSource<TKey, TValue> _dataSource;      // the underlying source of values

        /// <summary>Create a new read-through cache that has a Gen0 size limit and/or a periodic collection time</summary>
        /// <param name="dataSource">The underlying source to load data from</param>
        /// <param name="gen0Limit">(Optional) limit on the number of items allowed in Gen0 before a collection</param>
        /// <param name="timeToLive">(Optional) time period after which a unread item is evicted from the cache</param>
        public BatchReadThroughCache(IBatchDataSource<TKey, TValue> dataSource, int? gen0Limit, TimeSpan? timeToLive)
            : base(dataSource, gen0Limit, timeToLive)
        {
            _dataSource = dataSource;
        }
       
        /// <summary>Tries to get the values associated with the <paramref name="keys" /></summary>
        /// <param name="keys">The keys to find</param>
        /// <returns>An array the same size as the input <paramref name="keys" /> that contains a value or default(T) for each key in the corresponding index</returns>
        public TValue[] GetBatch(IReadOnlyCollection<TKey> keys)
        {
            BatchLoad batch;
            lock (SyncRoot)
            {
                batch = TryGetBatchFromCache(keys);
            }

            // we got all the results from the cache
            if (batch.MissedKeys.Count == 0)
                return batch.Results;

            // key not found by this point, read-through to the data source *outside* of the lock as this may take some time, i.e. network or file access
            var dsLoaded = _dataSource.GetBatch(batch.MissedKeys);

            return UpdateCacheAndResults(batch, dsLoaded);
        }

        /// <summary>Tries to get the values associated with the <paramref name="keys" /></summary>
        /// <param name="keys">The keys to find</param>
        /// <returns>An array the same size as the input <paramref name="keys" /> that contains a value or default(T) for each key in the corresponding index</returns>
        public async Task<TValue[]> GetBatchAsync(IReadOnlyCollection<TKey> keys)
        {
            BatchLoad batch;
            lock (SyncRoot)
            {
                batch = TryGetBatchFromCache(keys);
            }

            // we got all the results from the cache
            if (batch.MissedKeys.Count == 0)
                return batch.Results;

            // key not found by this point, read-through to the data source *outside* of the lock as this may take some time, i.e. network or file access
            var loaded = await _dataSource.GetBatchAsync(batch.MissedKeys);

            return UpdateCacheAndResults(batch, loaded);
        }

        private BatchLoad TryGetBatchFromCache(IReadOnlyCollection<TKey> keys)
        {
            var batch = new BatchLoad(keys.Count, Version);
            int i = 0;
            foreach (var key in keys)
            {
                TValue value;
                if (TryGetAnyGen(key, out value))
                {
                    batch.Results[i] = value;
                }
                else
                {
                    batch.MissedKeys.Add(key);
                    batch.MissedKeyIdx.Add(i);
                }
                i++;
            }
            return batch;
        }

        private TValue[] UpdateCacheAndResults(BatchLoad batch, TValue[] fromDataSource)
        {
            lock (SyncRoot)
            {
                int keyIdx = 0;
                foreach (var loaded in fromDataSource)
                {
                    if (!ValueEquality.Equals(loaded, default(TValue)))
                    {
                        var idx = batch.MissedKeyIdx[keyIdx];
                        TValue cached;
                        if (batch.Version != Version && TryGetAnyGen(batch.MissedKeys[keyIdx], out cached))
                        {
                            // another thread loaded our value
                            batch.Results[idx] = cached;
                            unchecked { Version++; }
                        }
                        else
                        {
                            // we loaded the value, store it in the cache
                            AddToGen0(batch.MissedKeys[keyIdx], loaded);
                            batch.Results[idx] = loaded;
                        }
                    }
                    keyIdx++;
                }
            }
            return batch.Results;
        }

        struct BatchLoad
        {
            public readonly TValue[] Results;
            public readonly List<TKey> MissedKeys;
            public readonly List<int> MissedKeyIdx;
            public readonly int Version;

            public BatchLoad(int keys, int version)
            {
                Version = version;
                Results = new TValue[keys];
                MissedKeys = new List<TKey>();
                MissedKeyIdx = new List<int>();
            }
        }
    }
}