using BusterWood.Collections;
using System.Collections.Generic;

namespace BusterWood.Linq
{
    /// <summary>Returns batches of unique elements</summary>
    class DistinctBatcher<T> : IBatcher<T>
    {
        readonly IBatcher<T> source;
        readonly EqualityComparer<T> _equality;

        public int BatchSize { get; }

        public DistinctBatcher(IBatcher<T> source, EqualityComparer<T> equality)
        {
            this.source = source;
            BatchSize = source.BatchSize;
            _equality = equality;
        }

        public IBatchEnumerator<T> GetBatchEnumerator() => new Enumerator(source.GetBatchEnumerator(), _equality);

        public class Enumerator : IBatchEnumerator<T>
        {
            readonly IBatchEnumerator<T> source;
            readonly UniqueList<T> unique; // use unique list so we can set initial capacity
            readonly T[] sourceBatch;

            public int BatchSize => source.BatchSize;

            public Enumerator(IBatchEnumerator<T> source, EqualityComparer<T> equality)
            {
                this.source = source;
                unique = new UniqueList<T>(BatchSize, equality);
                sourceBatch = new T[BatchSize];
            }

            public bool NextBatch(T[] batch, out int count)
            {
                count = 0;
                while (source.NextBatch(sourceBatch, out int sbCount) && count < batch.Length)
                {
                    for (int i = 0; i < sbCount; i++)
                    {
                        if (unique.Add(sourceBatch[i]))
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
