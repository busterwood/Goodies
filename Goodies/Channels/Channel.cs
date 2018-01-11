using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    /// <summary>A channel for communicating between two asynchronous threads.</summary>
    public class Channel<T> : IReceiver<T>, ISender<T>, ISelectable
    {
        readonly object _gate = new object();
        LinkedQueue<Sender<T>> _senders;        // senders waiting for a receiver
        LinkedQueue<Receiver<T>> _receivers;    // receivers waiting for a sender
        LinkedQueue<Waiter> _receiverWaiters;   // selects waiting to receive
        CancellationToken _closed;

        /// <summary>Has <see cref="Close"/> been called to shut down the channel?</summary>
        public bool IsClosed
        {
            get { return _closed.IsCancellationRequested; }
        }

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
                CancelAllWaitingReceivers();
            }
        }

        void CancelAllWaitingReceivers()
        {
            for (var r = _receivers.Head; r != null; r = r.Next)
            {
#if !NET45
                r.TrySetCanceled(_closed);
#else
                Task.Run(() => r.SetCanceled());
#endif
            }
            _receivers = new LinkedQueue<Receiver<T>>();
        }

        /// <summary>Tries to send a value to a waiting receiver.</summary>
        /// <param name="value">the value to send</param>
        /// <returns>TRUE if the value was sent, FALSE if the channel was closed or there was no waiting receivers</returns>
        public bool TrySend(T value)
        {
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                    return false;
                var receiver = Queue.Dequeue(ref _receivers);
                if (receiver == null)
                    return false;
                return receiver.TrySetResult(value);
            }
        }

        /// <summary>Synchronously sends a value to receiver, waiting until a receiver is ready to receive</summary>
        /// <param name="value">the value to send</param>
        /// <exception cref="OperationCanceledException">thrown when the channel <see cref="IsClosed"/></exception>
        public void Send(T value)
        {
            try
            {
                SendAsync(value).Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>Asynchronously sends a value to receiver, waiting until a receiver is ready to receive</summary>
        /// <param name="value">the value to send</param>
        /// <returns>A task that completes when the value has been sent to a receiver.  The returned task may be cancelled if the channel is closed</returns>
        public Task SendAsync(T value)
        {
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                {
                    return Task.FromCanceled(_closed);
#if !NET45
#else
#endif
                }

                // if there is a waiting receiver then exchange now
                var receiver = Queue.Dequeue(ref _receivers);
                if (receiver != null)
                {
#if !NET45
                    receiver.TrySetResult(value);
#else
                    Task.Run(() => receiver.TrySetResult(value));
#endif

                    return Task.CompletedTask;
                }

                // if there is a select waiting then signal a value is ready
                var waiter = Queue.Dequeue(ref _receiverWaiters);
                if (waiter != null)
                {
#if !NET45
                    waiter.TrySetResult(true);
#else
                    Task.Run(() => waiter.TrySetResult(true));
#endif
                }


                // the sender must wait
                var sender = new Sender<T>(value);
                Queue.Enqueue(ref _senders, sender);
                return sender.Task;
            }
        }

        /// <summary>Tries to receive a value from a waiting sender.</summary>
        /// <param name="value">the value that was received, or default(T) when no sender is ready</param>
        /// <returns>TRUE if a sender was ready and <paramref name="value"/> is set, otherwise returns FALSE</returns>
        public bool TryReceive(out T value)
        {
            lock (_gate)
            {
                var sender = Queue.Dequeue(ref _senders);
                if (sender == null)
                {
                    value = default(T);
                    return false;
                }
                value = sender.Value;
                sender.TrySetResult(true);
                return true;
            }
        }

        /// <summary>Synchronously receives a value, waiting for a sender is one is not ready</summary>
        /// <returns>The value that was sent</returns>
        /// <exception cref="OperationCanceledException">thrown when the channel <see cref="IsClosed"/> and there are no waiting senders</exception>
        public T Receive()
        {
            try
            {
                return ReceiveAsync().Result;
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>Asynchronously receives a value, waiting for a sender is one is not ready</summary>
        /// <returns>A task that completes with a result when a sender is ready.  The task may also be cancelled is the channel is closed and there are no waiting senders</returns>
        public Task<T> ReceiveAsync()
        {
            lock (_gate)
            {
                // if there is a waiting sender then exchange values now
                var sender = Queue.Dequeue(ref _senders);
                if (sender != null)
                {
                    var value = sender.Value;
                    sender.TrySetResult(true);
                    return Task.FromResult(value);
                }

                // if the channel has been closed then return a cancelled task
                if (_closed.IsCancellationRequested)
                    return Task.FromCanceled<T>(_closed);

                // the receiver must wait
                var r = new Receiver<T>();
                Queue.Enqueue(ref _receivers, r);
                return r.Task;
            }
        }

        /// <summary>Adds a waiter for a <see cref="Select"/></summary>
        void ISelectable.AddWaiter(Waiter waiter)
        {
            lock (_gate)
            {
                Queue.Enqueue(ref _receiverWaiters, waiter);

                // if there is a waiting sender then signal the select to wake up
                if (_senders.Head != null)
                    waiter.TrySetResult(true);
            }
        }

        /// <summary>Removes a waiter for a <see cref="Select"/></summary>
        void ISelectable.RemoveWaiter(Waiter waiter)
        {
            lock (_gate)
            {
                Queue.Remove(ref _receiverWaiters, waiter);
            }
        }
    }

    class Sender<T> : TaskCompletionSource<bool>, INext<Sender<T>>
    {
        public Sender<T> Next { get; set; } // linked list
        public readonly T Value;

        public Sender(T value)
#if !NET45
            : base(TaskCreationOptions.RunContinuationsAsynchronously)
#endif
        {
            Value = value;
        }
    }

    class Receiver<T> : TaskCompletionSource<T>, INext<Receiver<T>>
    {
        public Receiver<T> Next { get; set; } // linked list

        public Receiver()
#if !NET45
             : base(TaskCreationOptions.RunContinuationsAsynchronously)
#endif
        {
        }
    }

    public class Waiter : TaskCompletionSource<bool>, INext<Waiter>
    {
        public Waiter Next { get; set; } // linked list

        public Waiter()
#if !NET45
            : base(TaskCreationOptions.RunContinuationsAsynchronously)
#endif
        {
        }
    }
}
