using BusterWood.Collections;
using System;
using System.Collections.Generic;

namespace BusterWood.Linq
{
    /// <summary>Returns batches of unique elements</summary>
    class DistinctBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        readonly UniqueList<T> _unique; // use unique list so we can set initial capacity

        public int BatchSize { get; }

        public DistinctBatcher(IBatcher<T> source, EqualityComparer<T> equality)
        {
            this.source = source;
            BatchSize = source.BatchSize;
            _unique = new UniqueList<T>(BatchSize, equality);
        }

        public ArraySegment<T> NextBatch()
        {
            T[] batch = new T[BatchSize];
            int count = 0;
            while (count < batch.Length)
            {
                var sb = source.NextBatch();
                if (sb == default(ArraySegment<T>))
                    return sb;

                var sarr = sb.Array;
                int start = sb.Offset;
                int end = sb.Count + sb.Offset;
                for (int i = start; i < end; i++)
                {
                    if (_unique.Add(sarr[i]))
                    {
                        batch[count] = sarr[i];
                        count++;
                        if (count == batch.Length)
                            return new ArraySegment<T>(batch);
                    }
                }
            }

            return new ArraySegment<T>(batch, 0, count);
        }
    }


}
