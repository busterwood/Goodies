using System;

namespace BusterWood.Linq
{
    class SelectBatcher<T, TResult> : IBatcher<TResult>
    {
        readonly IBatcher<T> source;
        readonly Func<T, TResult> selector;

        public int BatchSize => source.BatchSize;

        public SelectBatcher(IBatcher<T> source, Func<T, TResult> selector)
        {
            this.source = source;
            this.selector = selector;
        }

        public IBatchEnumerator<TResult> GetBatchEnumerator() => new Enumerator(source.GetBatchEnumerator(), selector);

        public class Enumerator : IBatchEnumerator<TResult>
        {
            readonly IBatchEnumerator<T> source;
            readonly Func<T, TResult> selector;
            readonly T[] sourceBatch;

            public int BatchSize => source.BatchSize;

            public Enumerator(IBatchEnumerator<T> source, Func<T, TResult> selector)
            {
                this.source = source;
                this.selector = selector;
                sourceBatch = new T[BatchSize];
            }

            public bool NextBatch(TResult[] batch, out int count)
            {
                if (!source.NextBatch(sourceBatch, out count))
                    return false;

                for (int i = 0; i < count; i++)
                {
                    batch[i] = selector(sourceBatch[i]);
                }
                return true;
            }
        }
    }
}
