using System;
using System.Linq;

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
                    if (predicate(sarr[i]))
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
