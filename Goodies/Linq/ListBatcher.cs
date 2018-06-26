using System;
using System.Collections.Generic;

namespace BusterWood.Linq
{
    class ListBatcher<T> : IBatcher<T>
    {
        readonly List<T> source;
        int sourceIndex;

        public int BatchSize { get; }

        public ListBatcher(List<T> source, int batchSize)
        {
            this.source = source;
            BatchSize = Math.Min(source.Count, batchSize); // no point having a larger batch size if source is small
        }

        public ArraySegment<T> NextBatch()
        {
            if (sourceIndex >= source.Count)
                return default(ArraySegment<T>);

            int remaining = source.Count - sourceIndex;
            var batch = new T[BatchSize < remaining ? BatchSize : remaining];
            source.CopyTo(sourceIndex, batch, 0, batch.Length);
            sourceIndex += batch.Length;
            return new ArraySegment<T>(batch);
        }
    }


}
