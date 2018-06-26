using System;

namespace BusterWood.Linq
{
    class SkipBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        int _skip;

        public int BatchSize { get; }

        public SkipBatcher(IBatcher<T> source, int skip)
        {
            this.source = source;
            BatchSize = source.BatchSize;
            _skip = skip;
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
                if (_skip > 0)
                {
                    int toSkip = Math.Min(_skip, sb.Count);
                    start += toSkip;
                    _skip -= toSkip;
                }
                for (int i = start; i < end; i++)
                {
                    batch[count] = sarr[i];
                    count++;
                    if (count == batch.Length)
                        return new ArraySegment<T>(batch);
                }
                sb = source.NextBatch();
            } while (sb != default(ArraySegment<T>));

            return count == 0 ? new ArraySegment<T>() : new ArraySegment<T>(batch, 0, count);
        }
    }


}
