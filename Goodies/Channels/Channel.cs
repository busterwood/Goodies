using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    /// <summary>A mechanism for communicating between two asynchronous threads with CSP semantics.</summary>
    public class Channel<T> : IReceiver<T>, ISender<T>, ISelectable
    {
        readonly object _gate = new object();
        LinkedQueue<Sender<T>> _senders;        // senders waiting for a receiver
        LinkedQueue<Receiver<T>> _receivers;    // receivers waiting for a sender
        LinkedQueue<Waiter> _selects;   // selects waiting to receive
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
                CancelAllWaitingReceivers();
            }
        }

        void CancelAllWaitingReceivers()
        {
            for (var r = _receivers.Head; r != null; r = r.Next)
            {
                r.TrySetCanceled(_closed);
            }
            _receivers = new LinkedQueue<Receiver<T>>();
        }

        /// <summary>Tries to send a value to a waiting receiver.</summary>
        /// <param name="value">the value to send</param>
        /// <returns>TRUE if the value was sent, FALSE if the channel was closed or there was no waiting receivers</returns>
        public bool TrySend(T value)
        {
            Receiver<T> receiver;
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                    return false;
                receiver = Queue.Dequeue(ref _receivers);
            }
            return receiver != null && receiver.TrySetResult(value);
        }

        /// <summary>Synchronously sends a value to receiver, waiting until a receiver is ready to receive</summary>
        /// <param name="value">the value to send</param>
        /// <exception cref="OperationCanceledException">thrown when the channel <see cref="IsClosed"/></exception>
        public void Send(T value)
        {
            Sender<T> sender;
            Waiter waiter;
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                    throw new OperationCanceledException();

                // if there is a waiting receiver then exchange now
                var receiver = Queue.Dequeue(ref _receivers);
                if (receiver != null)
                {
                    receiver.TrySetResult(value);
                    return;
                }

                // the sender must wait
                sender = new Sender<T>(value);
                Queue.Enqueue(ref _senders, sender);

                // if there is a select waiting then signal a value is ready
                waiter = Queue.Dequeue(ref _selects);
            }
            waiter?.TrySetResult(true);
            sender.Task.Wait();
        }

        /// <summary>Asynchronously sends a value to receiver, waiting until a receiver is ready to receive</summary>
        /// <param name="value">the value to send</param>
        /// <returns>A task that completes when the value has been sent to a receiver.  The returned task may be cancelled if the channel is closed</returns>
        public Task SendAsync(T value)
        {
            Sender<T> sender;
            Waiter waiter;
            lock (_gate)
            {
                if (_closed.IsCancellationRequested)
                    return Tasks.FromCanceled(_closed);

                // if there is a waiting receiver then exchange now
                var receiver = Queue.Dequeue(ref _receivers);
                if (receiver != null)
                {
                    receiver.TrySetResult(value);
                    return Tasks.CompletedTask;
                }

                // the sender must wait
                sender = new Sender<T>(value);
                Queue.Enqueue(ref _senders, sender);

                // if there is a select waiting then signal a value is ready
                waiter = Queue.Dequeue(ref _selects);
            }
            waiter?.TrySetResult(true);
            return sender.Task;
        }

        /// <summary>Tries to receive a value from a waiting sender.</summary>
        /// <param name="value">the value that was received, or default(T) when no sender is ready</param>
        /// <returns>TRUE if a sender was ready and <paramref name="value"/> is set, otherwise returns FALSE</returns>
        public bool TryReceive(out T value)
        {
            Sender<T> sender;
            lock (_gate)
            {
                sender = Queue.Dequeue(ref _senders);
            }

            if (sender == null)
            {
                value = default(T);
                return false;
            }
            else
            {
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
            Receiver<T> receiver;
            lock (_gate)
            {
                // if there is a waiting sender then exchange values now
                var sender = Queue.Dequeue(ref _senders);
                if (sender != null)
                {
                    var value = sender.Value;
                    sender.TrySetResult(true);
                    return value;
                }

                // if the channel has been closed then return a cancelled task
                if (_closed.IsCancellationRequested)
                    throw new OperationCanceledException();

                // the receiver must wait
                receiver = new Receiver<T>();
                Queue.Enqueue(ref _receivers, receiver);
            }
            return receiver.Task.Result;
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
                    return Tasks.FromCanceled<T>(_closed);

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
                Queue.Enqueue(ref _selects, waiter);

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
                Queue.Remove(ref _selects, waiter);
            }
        }
    }
}
