using System;

namespace BusterWood.Linq
{
    class TakeBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        readonly int take;

        public int BatchSize { get; }
        
        public TakeBatcher(IBatcher<T> source, int count)
        {
            this.source = source;
            take = count;
            BatchSize = Math.Min(source.BatchSize, take);
        }

        public IBatchEnumerator<T> GetBatchEnumerator() => new Enumerator(source.GetBatchEnumerator(), BatchSize, take);

        public class Enumerator : IBatchEnumerator<T>
        {
            readonly IBatchEnumerator<T> source;
            readonly T[] sourceBatch;
            int take;

            public int BatchSize { get; }

            public Enumerator(IBatchEnumerator<T> source, int batchSize, int take)
            {
                this.source = source;
                BatchSize = batchSize;
                sourceBatch = new T[batchSize];
                this.take = take;
            }

            public bool NextBatch(T[] batch, out int count)
            {
                count = 0;
                while (take > 0 && count < batch.Length && source.NextBatch(sourceBatch, out int sbCount))
                {
                    int toCopy = Math.Min(sbCount - count, take);
                    Array.Copy(sourceBatch, 0, batch, count, toCopy);
                    count += toCopy;
                    take -= toCopy;
                }

                return count > 0;
            }
        }
    }
}
