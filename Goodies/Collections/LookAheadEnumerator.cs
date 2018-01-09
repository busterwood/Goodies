using System;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Collections
{
    /// <summary>An <see cref="IEnumerator{T}"/> that has a a look ahead of one item</summary>
    /// <typeparam name="T"></typeparam>
    public class LookAheadEnumerator<T> : IEnumerator<T>
    {
        readonly IEnumerator<T> inner;
        bool initialized;
        bool movedNext;

        /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public T Current { get; private set; }

        /// <summary>Gets the current element in the collection.</summary>
        /// <returns>The current element in the collection.</returns>
        object IEnumerator.Current => Current;

        /// <summary>The next item in the sequence, returns default for T when there is no next value</summary>
        public T Next => movedNext ? inner.Current : default(T);

        public LookAheadEnumerator(IEnumerator<T> inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            this.inner = inner;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            inner.Dispose();
        }

        /// <summary>Advances the enumerator to the next element of the collection.</summary>
        /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public bool MoveNext()
        {
            if (!initialized)
            {
                movedNext = inner.MoveNext();
                initialized = true;
            }
            var retVal = movedNext;
            Current = Next;
            movedNext = inner.MoveNext();
            return retVal;
        }

        /// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public void Reset()
        {
            inner.Reset();
            Current = default(T);
            initialized = false;
            movedNext = false;
        }
    }
}