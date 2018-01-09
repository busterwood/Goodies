using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusterWood.Collections;

namespace BusterWood.Caching
{
    /// <summary>
    /// Used to prevent multiple threads calling an underlying database, or remote service, to load the value for the *same* key.
    /// Different keys are handled concurrently, but indiviual keys are read by only one thread.
    /// </summary>
    /// <remarks>This could be useful on a client, or on the server side</remarks>
    public class BatchThunderingHerdProtection<TKey, TValue> : ThunderingHerdProtection<TKey, TValue>, IBatchDataSource<TKey, TValue>
    {
        readonly IBatchDataSource<TKey, TValue> _batchDataSource;

        public BatchThunderingHerdProtection(IBatchDataSource<TKey, TValue> dataSource) : base(dataSource)
        {
            _batchDataSource = dataSource;
        }

        public TValue[] GetBatch(IReadOnlyCollection<TKey> keys)
        {
            var batch = WorkOutWhatNeedsToBeLoaded(keys);

            if (batch.IsMixedLoad(keys.Count))
                return TryGetBatchMixed(keys, batch);

            // we are loading all the keys
            TValue[] loaded = _batchDataSource.GetBatch(keys);

            SetAllCompletionSources(batch.TaskCompletionSources, loaded);
            RemoveAllKeys(keys);
            return loaded;
        }

        BatchLoad WorkOutWhatNeedsToBeLoaded(IReadOnlyCollection<TKey> keys)
        {
            var tcs = new TaskCompletionSource<TValue>[keys.Count];
            var alreadyLoading = new bool[keys.Count];
            int toLoadCount = 0;
            lock (_inProgress)
            {
                int i = 0;
                foreach (var k in keys)
                {
                    TaskCompletionSource<TValue> t;
                    alreadyLoading[i] = _inProgress.TryGetOrAdd(k, NewTcs, out t);
                    if (alreadyLoading[i])
                        toLoadCount++;
                    tcs[i] = t;
                    i++;
                }
            }
            return new BatchLoad(tcs, alreadyLoading, toLoadCount);
        }

        void RemoveAllKeys(IReadOnlyCollection<TKey> keys)
        {
            lock (_inProgress)
            {
                _inProgress.RemoveRange(keys);
            }
        }

        static void SetAllCompletionSources(TaskCompletionSource<TValue>[] taskCompletionSources, TValue[] loaded)
        {
            int j = 0;
            foreach (var tcs in taskCompletionSources)
            {
                var value = loaded[j];
                var ignored = Task.Run(() => tcs.SetResult(value));
                j++;
            }
        }

        private TValue[] TryGetBatchMixed(IReadOnlyCollection<TKey> keys, BatchLoad batch)
        {
            // construct a list of keys to load
            var keysToLoad = new List<TKey>(batch.ToLoadCount);
            var originalIndexes = new List<int>(batch.ToLoadCount);
            int i = 0;
            foreach (var key in keys)
            {
                if (!batch.AlreadyLoading[i])
                {
                    keysToLoad.Add(key);
                    originalIndexes.Add(i);
                }
                i++;
            }

            // the following line may block
            TValue[] loaded = keysToLoad.Count > 0 ? _batchDataSource.GetBatch(keysToLoad) : new TValue[0];

            // set the results of the all the TCS we added
            i = 0;
            foreach (var l in loaded)
            {
                int idx = originalIndexes[i];
                Task.Run(() => batch.TaskCompletionSources[idx].TrySetResult(l));
                i++;
            }

            // remove all keys that we loaded
            RemoveAllKeys(keysToLoad);

            // construct results from TCS results
            return ResultsFromBatchTCS(keys, batch);
        }

        static TValue[] ResultsFromBatchTCS(IReadOnlyCollection<TKey> keys, BatchLoad batch)
        {
            var results = new TValue[keys.Count];
            int i = 0;
            foreach (var tcs in batch.TaskCompletionSources)
            {
                results[i] = tcs.Task.Result;
                i++;
            }
            return results;
        }

        public Task<TValue[]> GetBatchAsync(IReadOnlyCollection<TKey> keys) => Task.FromResult(GetBatch(keys)); // TODO: async version

        struct BatchLoad
        {
            public readonly TaskCompletionSource<TValue>[] TaskCompletionSources;
            public readonly bool[] AlreadyLoading;
            public readonly int ToLoadCount;

            public BatchLoad(TaskCompletionSource<TValue>[] tcs, bool[] alreadyLoading, int toLoadCount)
            {
                this.TaskCompletionSources = tcs;
                this.AlreadyLoading = alreadyLoading;
                this.ToLoadCount = toLoadCount;
            }

            public bool IsMixedLoad(int keys) => ToLoadCount < keys;
        }
    }
}