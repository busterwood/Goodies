using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusterWood.Collections;
using BusterWood.Tasks;

namespace BusterWood.Batching
{
    /// <summary>Batches calls to the <see cref="QueryAsync(TKey)"/> method, which a single item per key</summary>
    public class SingleResultAsyncBatcher<TKey, TValue>
    {
        readonly TimeSpan _delay = TimeSpan.FromMilliseconds(100);
        readonly object _gate = new object();
        readonly Func<IReadOnlyCollection<TKey>, Task<Dictionary<TKey, TValue>>> _queryMany;
        readonly Timer _timer;
        readonly Func<TKey, TaskCompletionSource<TValue>> _tcsFactory;
        Dictionary<TKey, TaskCompletionSource<TValue>> _completionSources;

        /// <summary>The configured batching delay</summary>
        /// <remarks>Set in the constructor</remarks>
        public TimeSpan Delay => _delay;

        /// <summary>Creates a batcher</summary>
        /// <param name="queryMany">The function to call to get the results for a set of keys</param>
        /// <param name="delay">Optional batching delay, defaults to 100ms</param>
        public SingleResultAsyncBatcher(Func<IReadOnlyCollection<TKey>, Task<Dictionary<TKey, TValue>>> queryMany, TimeSpan? delay = null)
        {
            if (queryMany == null) throw new ArgumentNullException(nameof(queryMany));
            if (delay.HasValue && delay.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            _queryMany = queryMany;
            _timer = new Timer(TimerCallback, null, Timeout.Never, Timeout.Never);
            _completionSources = new Dictionary<TKey, TaskCompletionSource<TValue>>();
            _tcsFactory = _ => new TaskCompletionSource<TValue>();
            if (delay.HasValue)
                _delay = delay.Value;
        }

        /// <summary>Gets the results for a <paramref name="key"/></summary>
        /// <returns>The result for the key, or default(TValue) if the batch query returned no data for the key</returns>
        /// <remarks>Calling this method will delay execution by up to <see cref="Delay"/></remarks>
        public Task<TValue> QueryAsync(TKey key)
        {
            lock (_gate)
            {
                var tcs = _completionSources.GetOrAdd(key, _tcsFactory);
                if (_completionSources.Count == 1)
                    _timer.Change(_delay, Timeout.Never);
                return tcs.Task;
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void TimerCallback(object state)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            // capture the dictionary at this point in time, replace it with an empty one
            Dictionary<TKey, TaskCompletionSource<TValue>> completionSources;
            lock (_gate)
            {
                _timer.Change(Timeout.Never, Timeout.Never);  // reset the timer, the Query method will start it again if needed
                completionSources = _completionSources;
                _completionSources = new Dictionary<TKey, TaskCompletionSource<TValue>>();
            }

            // try to query many at once
            Dictionary<TKey, TValue> results;
            try
            {
                results = await _queryMany(completionSources.Keys.ToList());  // on .NET 4.6 we don't need ".ToList()" as Keys implments IReadOnlyCollection
            }
            catch (Exception ex)
            {
                // it failed, pass the exception to all completion sources
                Action<object> trySetException = s => ((TaskCompletionSource<TValue>)s).TrySetException(ex);
                foreach (var tcs in completionSources.Values)
                    Task.Factory.StartNew(trySetException, tcs).DontWait();
                return;
            }

            // set the results
            foreach (var pair in completionSources)
            {
                TValue value;
                results.TryGetValue(pair.Key, out value);  // use default(TValue) if key not in dictionary
                var tcs = pair.Value;
                Task.Run(() => tcs.TrySetResult(value)).DontWait(); // make sure continuations run asynchronously
            }
        }
    }
}