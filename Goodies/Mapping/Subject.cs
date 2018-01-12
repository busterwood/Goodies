using System;
using System.Collections.Generic;
using System.Threading;

namespace BusterWood.Mapping
{
    /// <summary>
    /// Represents an object that is both an observable sequence as well as an observer.
    /// Each notification is broadcast to all subscribed observers.
    /// </summary>
    /// <remarks>Defined here so that Mapper has no external (third party) dependencies </remarks>
    sealed class Subject<T> : IObservable<T>, IObserver<T>
    {
        readonly object gate = new object();
        bool isDisposed;
        bool isStopped;
        List<IObserver<T>> observers;
        Exception exception;

        public Subject()
        {
            observers = new List<IObserver<T>>();
        }

        public void OnCompleted()
        {
            List<IObserver<T>> os = null;
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    os = observers;
                    observers = null;
                    isStopped = true;
                }
            }

            if (os == null)
                return;
            foreach (var o in os)
                o.OnCompleted();
        }

        public void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            List<IObserver<T>> os = null;
            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    os = observers;
                    observers = null;
                    isStopped = true;
                    exception = error;
                }
            }

            if (os == null)
                return;
            foreach (var o in os)
                o.OnError(error);
        }

        public void OnNext(T value)
        {
            List<IObserver<T>> os = null;
            lock (gate)
            {
                CheckDisposed();
                if (!isStopped)
                    os = observers;
            }

            if (os == null)
                return;
            foreach (var o in os)
                o.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (gate)
            {
                CheckDisposed();

                if (!isStopped)
                {
                    observers = new List<IObserver<T>>(observers);
                    observers.Add(observer);
                    return new Subscription(this, observer);
                }

                if (exception != null)
                    observer.OnError(exception);
                else
                    observer.OnCompleted();
                return Disposable.Empty;
            }
        }

        void Unsubscribe(IObserver<T> observer)
        {
            lock (gate)
            {
                if (observers == null)
                    return;

                observers = new List<IObserver<T>>(observers);
                observers.Remove(observer);
            }
        }

        sealed class Subscription : IDisposable
        {
            Subject<T> subject;
            IObserver<T> observer;

            public Subscription(Subject<T> subject, IObserver<T> observer)
            {
                this.subject = subject;
                this.observer = observer;
            }

            public void Dispose()
            {
                var o = Interlocked.Exchange(ref observer, null);
                if (o != null)
                {
                    subject.Unsubscribe(o);
                    subject = null;
                }
            }
        }

        void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(string.Empty);
        }

        public void Dispose()
        {
            lock (gate)
            {
                isDisposed = true;
                observers = null;
            }
        }
    }

    sealed class Disposable : IDisposable
    {
        public static readonly Disposable Empty = new Disposable();

        public void Dispose()
        {
        }
    }
}
