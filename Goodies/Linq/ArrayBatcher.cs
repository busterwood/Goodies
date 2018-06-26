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

        public ArraySegment<T> NextBatch()
        {
            if (sourceIndex >= source.Length)
                return default(ArraySegment<T>);

            int remaining = source.Length - sourceIndex;
            var batch = new T[BatchSize < remaining ? BatchSize : remaining];
            Array.Copy(source, sourceIndex, batch, 0, batch.Length);
            sourceIndex += batch.Length;
            return new ArraySegment<T>(batch);
        }
    }


}
