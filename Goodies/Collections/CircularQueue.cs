using BusterWood.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Collections
{
    /// <summary>A fixed size queue implemented using a circular array.</summary>
    public sealed class CircularQueue<T> : IEnumerable<T>
    {
        private readonly T[] arr;
        private int head;       // First valid element in the queue
        private int tail;       // Last valid element in the queue

        public CircularQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();

            arr = new T[capacity + 1]; // extra free slot to detect if the buffer is full
            head = 0;
            tail = 0;
        }

        public bool IsEmpty => head == tail;

        public bool IsFull => tail == head - 1 || (head == 0 && tail == arr.Length - 1);

        public int Capacity => arr.Length - 1;

        public void Clear()
        {
            Array.Clear(arr, 0, arr.Length);
            head = 0;
            tail = 0;
        }

        public void Enqueue(T item)
        {
            Contract.Requires(!IsFull);
            arr[tail] = item;
            tail = (tail + 1) % arr.Length;
        }

        public T Dequeue()
        {
            Contract.Requires(!IsEmpty);
            T removed = arr[head];
            arr[head] = default(T);
            head = (head + 1) % arr.Length;
            return removed;
        }

        public T Peek()
        {
            Contract.Requires(!IsEmpty);
            return arr[head];
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = head; i != tail; i = (i + 1) % arr.Length)
            {
                yield return arr[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}