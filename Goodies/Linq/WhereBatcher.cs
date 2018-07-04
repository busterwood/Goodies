using System;

namespace BusterWood.Linq
{
    class WhereBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        readonly Func<T, bool> predicate;

        public int BatchSize { get; }

        public WhereBatcher(IBatcher<T> source, Func<T, bool> predicate)
        {
            this.source = source;
            this.predicate = predicate;
            BatchSize = source.BatchSize;
        }

        public IBatchEnumerator<T> GetBatchEnumerator() => new Enumerator(source.GetBatchEnumerator(), predicate);

        public class Enumerator : IBatchEnumerator<T>
        {
            readonly IBatchEnumerator<T> source;
            readonly Func<T, bool> predicate;
            readonly T[] sourceBatch;

            public int BatchSize => source.BatchSize;

            public Enumerator(IBatchEnumerator<T> source, Func<T, bool> predicate)
            {
                this.source = source;
                this.predicate = predicate;
                sourceBatch = new T[BatchSize];
            }

            public bool NextBatch(T[] batch, out int count)
            {
                count = 0;
                while (source.NextBatch(sourceBatch, out int sbCount))
                {
                    for (int i = 0; i < sbCount; i++)
                    {
                        if (predicate(sourceBatch[i]))
                        {
                            batch[count] = sourceBatch[i];
                            count++;
                            if (count == batch.Length)
                                return true;
                        }
                    }
                }
                return count > 0;
            }
        }
    }
}
