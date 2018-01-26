using BusterWood.Collections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    /// <summary>
    /// Publishes values to multiple consumer channels.
    /// Add one or more subscribers via the <see cref="Subscribe(ISender{T})"/> method.
    /// Typically you would add a <see cref="BufferedChannel{T}"/> with a large buffer as the subscriber 
    /// because a value will not be sent to a subscriber that cannot immediately take the value.
    /// </summary>
    public class MulticastChannel<T> : ISender<T>
    {
        readonly object _gate = new object();
        readonly UniqueList<ISender<T>> _subscriptions = new UniqueList<ISender<T>>();
        CancellationToken _closed;

        /// <summary>Has <see cref="Close"/> been called to shut down the channel?</summary>
        public bool IsClosed => _closed.IsCancellationRequested;

        /// <summary>Closing a channel prevents any further values being sent and will cancel the tasks of any waiting receivers, <see cref="ReceiveAsync"/></summary>
        public void Close()
        {
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                    return;
                var source = new CancellationTokenSource();
                _closed = source.Token;
                source.Cancel();
            }
        }

        /// <summary>
        /// Adds a subscriber to this multicast channel.  Typically you would add a <see cref="BufferedChannel{T}"/> as the subscriber.
        /// </summary>
        public void Subscribe(ISender<T> channel)
        {
            lock(_gate)
            {
                _subscriptions.Add(channel);
            }
        }

        /// <summary>Removes a subscriber from this multicast channel.</summary>
        public void Unsubscribe(ISender<T> channel)
        {
            lock(_gate)
            {
                _subscriptions.Remove(channel);
            }
        }

        /// <summary>
        /// Sends a value to all subscribers.  
        /// If a subscriber cannot take the value immediately then the value is not sent for that subscriber.
        /// </summary>
        public void Send(T value)
        {
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                    throw new OperationCanceledException();

                foreach (var s in _subscriptions)
                {
                    s.TrySend(value);  // don't block if a receiver is not ready
                }
            }
        }

        /// <summary>
        /// Sends a value to all subscribers.  
        /// If a subscriber cannot take the value immediately then the value is not sent for that subscriber.
        /// </summary>
        public Task SendAsync(T value)
        {
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                    return Task.FromCanceled(_closed);

                foreach (var s in _subscriptions)
                {
                    s.TrySend(value);  // don't block if a receiver is not ready
                }
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Tries to send a value to all subscribers.  
        /// If a subscriber cannot take the value immediately then the value is not sent for that subscriber.
        /// </summary>
        /// <returns>Returns TRUE if the value was sent to a least one subscriber</returns>
        public bool TrySend(T value)
        {
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                    return false;

                var result = true;
                foreach (var s in _subscriptions)
                {
                    result &= s.TrySend(value);
                }
                return result;
            }
        }

    }
}
