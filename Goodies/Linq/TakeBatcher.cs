using System;

namespace BusterWood.Linq
{
    class TakeBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        readonly int take;

        public int BatchSize => source.BatchSize;

        public TakeBatcher(IBatcher<T> source, int count)
        {
            this.source = source;
            take = count;
        }

        public IBatchEnumerator<T> GetBatchEnumerator() => new Enumerator(source.GetBatchEnumerator(), take);

        public class Enumerator : IBatchEnumerator<T>
        {
            readonly IBatchEnumerator<T> source;
            readonly T[] sourceBatch;
            int take;

            public int BatchSize => source.BatchSize;

            public Enumerator(IBatchEnumerator<T> source, int take)
            {
                this.source = source;
                this.take = take;
                sourceBatch = new T[BatchSize];
            }

            public bool NextBatch(T[] batch, out int count)
            {
                count = 0;
                while (source.NextBatch(sourceBatch, out int sbCount))
                {
                    int toCopy = batch.Length - count;
                    Array.Copy(sourceBatch, 0, batch, count, toCopy);
                    count += toCopy;

                    if (count == batch.Length)
                        break;
                }

                return count > 0;
            }
        }
    }
}
