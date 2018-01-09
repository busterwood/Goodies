using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusterWood.Collections;

namespace BusterWood.Caching
{
    /// <summary>
    /// Used to prevent multiple threads calling an underlying database, or remote service, to load the value for the *same* key.
    /// Different keys are handled concurrently, but individual keys are read by only one thread.
    /// </summary>
    /// <remarks>This could be useful on a client, or on the server side</remarks>
    public class ThunderingHerdProtection<TKey, TValue> : IDataSource<TKey, TValue>
    {
        protected readonly Dictionary<TKey, TaskCompletionSource<TValue>> _inProgress; // only populated keys currently being read
        protected readonly IDataSource<TKey, TValue> _dataSource;
        readonly EvictionHandler<TKey, TValue> _dataSourceEvicted;

        public ThunderingHerdProtection(IDataSource<TKey, TValue> dataSource)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            _dataSource = dataSource;
            _inProgress = new Dictionary<TKey, TaskCompletionSource<TValue>>();
        }

        public TValue this[TKey key]
        {
            get
            {
                TaskCompletionSource<TValue> tcs;
                bool alreadyLoading;
                lock (_inProgress)
                {
                    alreadyLoading = _inProgress.TryGetOrAdd(key, NewTcs, out tcs);
                }

                if (alreadyLoading)
                {
                    // wait for another thread to load the value, then return
                    return tcs.Task.Result;   // blocks
                }

                // call the data source to load it
                var value = _dataSource[key]; // blocks

                var ignored = Task.Run(() => tcs.SetResult(value));
                lock (_inProgress)
                {
                    _inProgress.Remove(key);
                }
                return value;
            }
        }

        public async Task<TValue> GetAsync(TKey key)
        {
            TaskCompletionSource<TValue> tcs;
            bool alreadyLoading;
            lock (_inProgress)
            {
                alreadyLoading = _inProgress.TryGetOrAdd(key, NewTcs, out tcs);
            }

            if (alreadyLoading)
            {
                return await tcs.Task;
            }

            // call the data source to load it
            var value = await _dataSource.GetAsync(key);

            var ignored = Task.Run(() => tcs.SetResult(value));
            lock (_inProgress)
            {
                _inProgress.Remove(key);
            }
            return value;
        }

        protected TaskCompletionSource<TValue> NewTcs(TKey key) => new TaskCompletionSource<TValue>();
    }
}