using System;

namespace BusterWood.Linq
{
    class ArrayBatcher<T> : IBatcher<T>
    {
        readonly T[] source;
        int sourceIndex;

        public int BatchSize { get; }

        public ArrayBatcher(T[] source, int batchSize)
        {
            this.source = source;
            BatchSize = Math.Min(source.Length, batchSize); // no point having a larger batch size if source is small
        }

        public IBatchEnumerator<T> GetBatchEnumerator()
        {
            return new Enumerator(source, BatchSize);
        }

        public class Enumerator : IBatchEnumerator<T>
        {
            readonly T[] source;
            int sourceIndex;

            public int BatchSize { get; }

            public Enumerator(T[] source, int batchSize)
            {
                this.source = source;
                BatchSize = batchSize;
            }

            public bool NextBatch(T[] batch, out int count)
            {
                if (sourceIndex >= source.Length)
                {
                    count = 0;
                    return false;
                }

                int remaining = source.Length - sourceIndex;
                count = BatchSize < remaining ? BatchSize : remaining;
                Array.Copy(source, sourceIndex, batch, 0, count);
                sourceIndex += count;
                return true;
            }
        }
    }

}
