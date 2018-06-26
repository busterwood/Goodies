using System;

namespace BusterWood.Linq
{
    class TakeBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        int _take;

        public int BatchSize { get; }

        public TakeBatcher(IBatcher<T> source, int count)
        {
            this.source = source;
            BatchSize = source.BatchSize;
            _take = count;
        }

        public ArraySegment<T> NextBatch()
        {
            var sb = source.NextBatch();
            if (sb == default(ArraySegment<T>))
                return new ArraySegment<T>();

            T[] batch = new T[Math.Min(BatchSize, _take)];
            int count = 0;
            do
            {
                int toCopy = batch.Length - count;
                Array.Copy(sb.Array, sb.Offset, batch, count, toCopy);
                count += toCopy;

                if (count == batch.Length)
                    break;

                sb = source.NextBatch();
            } while (sb != default(ArraySegment<T>));
            _take -= count;
            return count == 0 ? default(ArraySegment<T>) : new ArraySegment<T>(batch, 0, count);
        }
    }


}
