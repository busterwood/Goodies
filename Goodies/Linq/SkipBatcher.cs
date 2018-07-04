using System;

namespace BusterWood.Linq
{
    class SkipBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        readonly int skip;

        public int BatchSize { get; }

        public SkipBatcher(IBatcher<T> source, int skip)
        {
            this.source = source;
            BatchSize = source.BatchSize;
            this.skip = skip;
        }

        public IBatchEnumerator<T> GetBatchEnumerator() => new Enumerator(source.GetBatchEnumerator(), skip);

        public class Enumerator : IBatchEnumerator<T>
        {
            readonly IBatchEnumerator<T> source;
            readonly T[] sourceBatch;
            int skip;

            public int BatchSize => source.BatchSize;

            public Enumerator(IBatchEnumerator<T> source, int skip)
            {
                this.source = source;
                this.skip = skip;
                sourceBatch = new T[BatchSize];
            }

            public bool NextBatch(T[] batch, out int count)
            {
                count = 0;
                while (source.NextBatch(sourceBatch, out int sbCount))
                {
                    int start = 0;
                    if (skip > 0)
                    {
                        int toSkip = Math.Min(skip, sbCount);
                        start += toSkip;
                        skip -= toSkip;
                    }
                    for (int i = start; i < sbCount; i++)
                    {
                        batch[count] = sourceBatch[i];
                        count++;
                        if (count == batch.Length)
                            return true;
                    }                    
                }

                return count > 0;
            }
        }
    }
}