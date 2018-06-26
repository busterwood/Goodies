﻿using System;

namespace BusterWood.Linq
{
    class WhereBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        private readonly Func<T, bool> predicate;

        public int BatchSize { get; }

        public WhereBatcher(IBatcher<T> source, Func<T, bool> predicate)
        {
            this.source = source;
            this.predicate = predicate;
            BatchSize = source.BatchSize;
        }

        public ArraySegment<T> NextBatch()
        {
            var sb = source.NextBatch();
            if (sb == default(ArraySegment<T>))
                return new ArraySegment<T>();

            T[] batch = new T[BatchSize];
            int count = 0;
            do
            {
                var sarr = sb.Array;
                int start = sb.Offset;
                int end = sb.Count + sb.Offset;
                for (int i = start; i < end; i++)
                {
                    if (predicate(sarr[i]))
                    {
                        batch[count] = sarr[i];
                        count++;
                        if (count == batch.Length)
                            return new ArraySegment<T>(batch);
                    }
                }

                sb = source.NextBatch();
            } while (sb != default(ArraySegment<T>));

            return count == 0 ? new ArraySegment<T>() : new ArraySegment<T>(batch, 0, count);
        }
    }


}
