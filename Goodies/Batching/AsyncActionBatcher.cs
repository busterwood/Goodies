using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Batching
{
    /// <summary>Batches calls to the <see cref="DoAsync(T)"/> method, delegating to method that takes many values at once</summary>
    public class AsyncActionBatcher<T>
    {
        readonly object _gate = new object();
        readonly Func<IReadOnlyCollection<T>, Task> _doMany;
        readonly Timer _timer;
        TaskCompletionSource<bool> _completionSource;
        List<T> _inputs;

        /// <summary>The configured batching delay</summary>
        /// <remarks>Set in the constructor</remarks>
        public TimeSpan Delay { get; } = TimeSpan.FromMilliseconds(100);

        /// <summary>Creates a batcher</summary>
        /// <param name="doMany">The function to call to get the results for a set of keys</param>
        /// <param name="delay">Optional batching delay, defaults to 100ms</param>
        public AsyncActionBatcher(Func<IReadOnlyCollection<T>, Task> doMany, TimeSpan? delay = null)
        {
            if (delay.HasValue && delay.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            _doMany = doMany ?? throw new ArgumentNullException(nameof(doMany));
            _timer = new Timer(TimerCallback, null, Timeout.Never, Timeout.Never);
            _completionSource = new TaskCompletionSource<bool>();
            if (delay.HasValue)
                Delay = delay.Value;
        }

        /// <summary>Performs an async action to the <paramref name="input"/></summary>
        /// <returns>A <see cref="Task"/> that becomes Complete or Faulted</returns>
        /// <remarks>Calling this method will delay execution by up to <see cref="Delay"/></remarks>
        public Task DoAsync(T input)
        {
            lock (_gate)
            {
                if (_inputs == null)
                {
                    _inputs = new List<T> { input };
                    _completionSource = new TaskCompletionSource<bool>(
#if !NET452
                        TaskCreationOptions.RunContinuationsAsynchronously
#endif
                    );
                    _timer.Change(Delay, Timeout.Never);
                }
                return _completionSource.Task;
            }
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void TimerCallback(object state)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            // capture the inputs and completion source at this point in time
            TaskCompletionSource<bool> tcs;
            List<T> inputs;
            lock (_gate)
            {
                _timer.Change(Timeout.Never, Timeout.Never);  // reset the timer, the DoAsync method will start it again if needed
                tcs = _completionSource;
                inputs = _inputs;
                _completionSource = null;
                _inputs = null;
            }

            // do the action on many inputs in one go
            try
            {
                await _doMany(inputs);
            }
            catch (Exception ex)
            {
                // it failed, pass the exception to all waiters
                tcs.TrySetException(ex);
                return;
            }

            // tell the waiters that the task has now been done
            tcs.TrySetResult(true);
        }
    }
}