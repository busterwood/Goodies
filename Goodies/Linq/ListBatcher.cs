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
            if (source.Count < batchSize)
                BatchSize = source.Count;   // no point having a larger batch size if source is small
            //else if (source.Count < batchSize * 3)
            //    BatchSize = (source.Count / 2) + 1; // avoid memory overhead when list size is small
            else
                BatchSize = batchSize; 
        }

        public IBatchEnumerator<T> GetBatchEnumerator() => new Enumerator(source, BatchSize);

        public class Enumerator : IBatchEnumerator<T>
        {
            readonly List<T> source;
            int sourceIndex;

            public int BatchSize { get; }

            public Enumerator(List<T> source, int batchSize)
            {
                this.source = source;
                BatchSize = batchSize;
            }

            public bool NextBatch(T[] batch, out int count)
            {
                if (sourceIndex >= source.Count)
                {
                    count = 0;
                    return false;
                }

                int remaining = source.Count - sourceIndex;
                count = BatchSize < remaining ? BatchSize : remaining;
                source.CopyTo(sourceIndex, batch, 0, count);
                sourceIndex += count;
                return true;
            }
        }
    }

}
