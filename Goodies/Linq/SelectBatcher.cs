using System;

namespace BusterWood.Linq
{
    class SelectBatcher<T, TResult> : IBatcher<TResult>
    {
        readonly IBatcher<T> source;
        private readonly Func<T, TResult> map;

        public int BatchSize { get; }

        public SelectBatcher(IBatcher<T> source, Func<T, TResult> map)
        {
            this.source = source;
            this.map = map;
            BatchSize = source.BatchSize;
        }

        public ArraySegment<TResult> NextBatch()
        {
            var sb = source.NextBatch();
            if (sb == default(ArraySegment<T>))
                return new ArraySegment<TResult>();

            TResult[] batch = new TResult[sb.Count];
            var sarr = sb.Array;
            for (int i = 0; i < batch.Length; i++)
            {
                batch[i] = map(sarr[i + sb.Offset]);
            }
            return new ArraySegment<TResult>(batch);
        }
    }


}
