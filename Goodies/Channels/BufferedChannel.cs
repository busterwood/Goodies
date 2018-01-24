using BusterWood.Collections;
using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    public class BufferedChannel<T> : ISender<T>, IReceiver<T>, ISelectable
    {
        readonly CircularQueue<T> _items;
        LinkedQueue<Sender<T>> _senders;        // senders waiting for a receiver
        LinkedQueue<Receiver<T>> _receivers;    // receivers waiting for a sender
        LinkedQueue<Waiter> _selects;   // selects waiting to receive
        CancellationToken _closed;

        public BufferedChannel(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "must be one or more");
            _items = new CircularQueue<T>(capacity);
        }

        /// <summary>Has <see cref="Close"/> been called to shut down the channel?</summary>
        public bool IsClosed
        {
            get { return _closed.IsCancellationRequested; }
        }

        /// <summary>Closing a channel prevents any further values being sent and will cancel the tasks of any waiting receivers, <see cref="ReceiveAsync"/></summary>
        public void Close()
        {
            lock (_items)
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
            lock (_items)
            {
                if (_items.IsEmpty)
                {
                    // if the channel has been closed then return a cancelled task
                    if (_closed.IsCancellationRequested)
                        return Task.FromCanceled<T>(_closed);

                    // the receiver must wait
                    var r = new Receiver<T>();
                    Queue.Enqueue(ref _receivers, r);
                    return r.Task;
                }
                else
                {
                    // there are queued items, just grad the first one
                    T value = _items.Dequeue();

                    if (_senders.Head != null)
                        ReleaseWaitingSenderByEnqueuingItsValue();

                    return Task.FromResult(value);
                }
            }
        }

        private void ReleaseWaitingSenderByEnqueuingItsValue()
        {
            Contract.Requires(!_items.IsFull);

            var sender = Queue.Dequeue(ref _senders);
            if (sender != null)
            {
                _items.Enqueue(sender.Value);
                sender.TrySetResult(true);
            }
        }

        public bool TryReceive(out T value)
        {
            lock (_items)
            {
                if (_items.IsEmpty)
                {
                    value = default(T);
                    return false;
                }
                else
                {
                    // there are queued items, just grad the first one
                    value = _items.Dequeue();
                    if (_senders.Head != null)
                        ReleaseWaitingSenderByEnqueuingItsValue();
                    return true;
                }
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
            lock (_items)
            {
                if (_closed.IsCancellationRequested)
                    return Task.FromCanceled(_closed);

                if (_items.IsFull)
                {
                    // the queue is full, the sender must wait
                    var sender = new Sender<T>(value);
                    Queue.Enqueue(ref _senders, sender);
                    return sender.Task;
                }

                if (!_items.IsEmpty)
                {
                    // just add the value to the queue so items are received in sending order
                    _items.Enqueue(value);
                    return Task.CompletedTask;
                }

                // At this point queue is must be empty 
                Contract.Assert(_items.IsEmpty);

                // if there is a waiting receiver then exchange now
                var receiver = Queue.Dequeue(ref _receivers);
                if (receiver != null)
                {
                    receiver.TrySetResult(value);
                    return Task.CompletedTask;
                }

                // no waiting receivers, add the item to the queue
                _items.Enqueue(value);

                // if there is a select waiting then signal a value is ready
                var waiter = Queue.Dequeue(ref _selects);
                if (waiter != null)
                    waiter.TrySetResult(true);

                return Task.CompletedTask;
            }
        }

        public bool TrySend(T value)
        {
            lock (_items)
            {
                if (_closed.IsCancellationRequested)
                    return false;

                if (_items.IsFull)
                    return false;

                if (!_items.IsEmpty)
                {
                    // just add the value to the queue so items are received in sending order
                    _items.Enqueue(value);
                    return true;
                }

                // At this point queue is must be empty 
                Contract.Assert(_items.IsEmpty);

                // if there is a waiting receiver then exchange now
                var receiver = Queue.Dequeue(ref _receivers);
                if (receiver != null)
                {
                    receiver.TrySetResult(value);
                    return true;
                }

                // add the item to the queue
                _items.Enqueue(value);

                // if there is a select waiting then signal a value is ready
                var waiter = Queue.Dequeue(ref _selects);
                if (waiter != null)
                    waiter.TrySetResult(true);

                return true;
            }
        }

        /// <summary>Adds a waiter for a <see cref="Select"/></summary>
        void ISelectable.AddWaiter(Waiter waiter)
        {
            lock (_items)
            {
                Queue.Enqueue(ref _selects, waiter);

                // if there values waiting to be received then signal the queue to wake up
                if (!_items.IsEmpty)
                    waiter.TrySetResult(true);
            }
        }

        /// <summary>Removes a waiter for a <see cref="Select"/></summary>
        void ISelectable.RemoveWaiter(Waiter waiter)
        {
            lock (_items)
            {
                Queue.Remove(ref _selects, waiter);
            }
        }

    }
}
