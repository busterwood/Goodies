using BusterWood.Collections;
using BusterWood.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Batching
{
    /// <summary>Batches calls to the <see cref="QueryAsync(TKey)"/> method, which returns multiple items per key</summary>
    public class AsyncFuncManyBatcher<TKey, TValue>
    {
        readonly object _gate = new object();
        readonly Func<IReadOnlyCollection<TKey>, Task<ILookup<TKey, TValue>>> _queryMany;
        readonly Timer _timer;
        readonly Func<TKey, TaskCompletionSource<IEnumerable<TValue>>> _tcsFactory;
        Dictionary<TKey, TaskCompletionSource<IEnumerable<TValue>>> _completionSources;

        /// <summary>The configured batching delay</summary>
        /// <remarks>Set in the constructor</remarks>
        public TimeSpan Delay { get; } = TimeSpan.FromMilliseconds(100);

        /// <summary>Creates a batcher</summary>
        /// <param name="queryMany">The function to call to get the results for a set of keys</param>
        /// <param name="delay">Optional batching delay, defaults to 100ms</param>
        public AsyncFuncManyBatcher(Func<IReadOnlyCollection<TKey>, Task<ILookup<TKey, TValue>>> queryMany, TimeSpan? delay = null)
        {
            if (delay.HasValue && delay.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            _queryMany = queryMany ?? throw new ArgumentNullException(nameof(queryMany));
            _timer = new Timer(TimerCallback, null, Timeout.Never, Timeout.Never);
            _completionSources = new Dictionary<TKey, TaskCompletionSource<IEnumerable<TValue>>>();
            _tcsFactory = _ => new TaskCompletionSource<IEnumerable<TValue>>(
#if !NET452
                TaskCreationOptions.RunContinuationsAsynchronously
#endif
            );
            if (delay.HasValue)
                Delay = delay.Value;
        }

        /// <summary>Gets the results for a <paramref name="key"/></summary>
        /// <returns>Zero or mores results</returns>
        /// <remarks>Calling this method will delay execution by up to <see cref="Delay"/></remarks>
        public Task<IEnumerable<TValue>> QueryAsync(TKey key)
        {
            lock (_gate)
            {
                var tcs = _completionSources.GetOrAdd(key, _tcsFactory);
                if (_completionSources.Count == 1)
                    _timer.Change(Delay, Timeout.Never);
                return tcs.Task;
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void TimerCallback(object state)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            // capture the dictionary at this point in time, replace it with an empty one
            Dictionary<TKey, TaskCompletionSource<IEnumerable<TValue>>> completionSources;
            lock (_gate)
            {
                _timer.Change(Timeout.Never, Timeout.Never);  // reset the timer, the Query method will start it again if needed
                completionSources = _completionSources;
                _completionSources = new Dictionary<TKey, TaskCompletionSource<IEnumerable<TValue>>>();
            }

            // try to query many at once
            ILookup<TKey, TValue> results;
            try
            {
#if NET452
                results = await _queryMany(completionSources.Keys.ToList());
#else
                results = await _queryMany(completionSources.Keys);
#endif
            }
            catch (Exception ex)
            {
                // it failed, pass the exception to all completion sources
                foreach (var tcs in completionSources.Values)
                {
#if NET452
                    Task.Run(() => tcs.TrySetException(ex));
#else
                    tcs.TrySetException(ex);
#endif
                }
                return;
            }

            // set the results
            foreach (var pair in completionSources)
            {
                var values = results[pair.Key];  // lookups return an empty IEnumerable when the key is not present
#if NET452
                Task.Run(() => pair.Value.TrySetResult(values));
#else
                pair.Value.TrySetResult(values);
#endif
            }
        }
    }
}
