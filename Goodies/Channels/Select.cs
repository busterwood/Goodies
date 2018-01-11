using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    /// <summary>A Select allows for receiving on one on many channels, in priority order</summary>
    public class Select
    {
        static readonly Task<bool> True = Task.FromResult(true);
        static readonly Task<bool> False = Task.FromResult(false);

        List<ICase> cases = new List<ICase>();

        /// <summary>Adds a action to perform when a channel can be read</summary>
        /// <param name="ch">The channel to try to read from (which must also implement <see cref="ISelectable"/>)</param>
        /// <param name="action">the synchronous action to perform with the value that was read</param>
        public Select OnReceive<T>(IReceiver<T> ch, Action<T> action)
        {
            if (ch == null) throw new ArgumentNullException(nameof(ch));
            if (action == null) throw new ArgumentNullException(nameof(action));
            cases.Add(new ReceiveCase<T>(ch, action));
            return this;
        }

        /// <summary>Adds a asynchronous action to perform when a channel can be read</summary>
        /// <param name="ch">The channel to try to read from (which must also implement <see cref="ISelectable"/>)</param>
        /// <param name="action">the asynchronous action to perform with the value that was read</param>
        public Select OnReceiveAsync<T>(IReceiver<T> ch, Func<T, Task> action)
        {
            if (ch == null) throw new ArgumentNullException(nameof(ch));
            if (action == null) throw new ArgumentNullException(nameof(action));
            cases.Add(new ReceiveAsyncCase<T>(ch, action));
            return this;
        }

        /// <summary>Tries to reads from one (and only one) of the added channels.  Does no action if the <paramref name="timeout"/> has been reached</summary>
        /// <returns>True if an action was performed, False if no action was performed and the timeout was reached</returns>
        public async Task<bool> ExecuteAsync(TimeSpan timeout)
        {
            if (timeout == System.Threading.Timeout.InfiniteTimeSpan || timeout == TimeSpan.MaxValue)
            {
                await ExecuteAsync();
                return false;
            }

            var timedOut = false;
            var receiveTimeout = new ReceiveCase<DateTime>(Timeout.After(timeout), _ => timedOut = true);
            var idx = cases.Count;
            cases.Add(receiveTimeout);
            await ExecuteAsync();
            cases.RemoveAt(idx);
            return timedOut;
        }

        /// <summary>Reads from one (and only one) of the added channels and performs the associated action</summary>
        /// <returns>A task that completes when one channel has been read and the associated action performed</returns>
        public async Task ExecuteAsync()
        {
            if (cases.Count == 0) throw new InvalidOperationException("No cases have been added");
            for (;;)
            {
                // try to execute any case that is ready
                foreach (var c in cases)
                {
                    if (await c.TryExecuteAsync())
                        return;
                }

                // we must wait, no channels are ready
                var waiter = new Waiter();
                foreach (var c in cases)
                    c.AddWaiter(waiter);
                await waiter.Task;
                foreach (var c in cases)
                    c.RemoveWaiter(waiter);
            }
        }

        interface ICase
        {
            Task<bool> TryExecuteAsync();
            void AddWaiter(Waiter tcs);
            void RemoveWaiter(Waiter tcs);
        }


        class ReceiveAsyncCase<T> : ICase
        {
            readonly IReceiver<T> ch;
            readonly Func<T, Task> asyncAction;

            public ReceiveAsyncCase(IReceiver<T> ch, Func<T, Task> asyncAction)
            {
                this.ch = ch;
                this.asyncAction = asyncAction;
                if (!(ch is ISelectable))
                    throw new ArgumentException("receiver must implement " + nameof(ISelectable), nameof(ch));
            }

            public Task<bool> TryExecuteAsync()
            {
                T val;
                return ch.TryReceive(out val) ? AsyncExcuteAction(val) : False;
            }

            async Task<bool> AsyncExcuteAction(T val)
            {
                await asyncAction(val);
                return true;
            }

            public void AddWaiter(Waiter tcs)
            {
                ((ISelectable)ch).AddWaiter(tcs);
            }

            public void RemoveWaiter(Waiter tcs)
            {
                ((ISelectable)ch).RemoveWaiter(tcs);
            }
        }

        class ReceiveCase<T> : ICase
        {
            readonly IReceiver<T> ch;
            readonly Action<T> action;

            public ReceiveCase(IReceiver<T> ch, Action<T> action)
            {
                this.ch = ch;
                this.action = action;
                if (!(ch is ISelectable))
                    throw new ArgumentException("receiver must implement " + nameof(ISelectable), nameof(ch));
            }

            public Task<bool> TryExecuteAsync()
            {
                T val;
                if (ch.TryReceive(out val)) {
                    action(val);
                    return True;
                }
                return False;
            }

            public void AddWaiter(Waiter tcs)
            {
                ((ISelectable)ch).AddWaiter(tcs);
            }

            public void RemoveWaiter(Waiter tcs)
            {
                ((ISelectable)ch).RemoveWaiter(tcs);
            }
        }
    }

    public static class Extensions
    {
        public static async Task<bool> ExecuteAsync(this Select select, TimeSpan? timeout)
        {
            if (timeout != null)
                return await select.ExecuteAsync(timeout.Value);

            await select.ExecuteAsync();
            return false;
        }
    }
}
