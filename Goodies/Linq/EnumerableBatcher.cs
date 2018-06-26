using System;
using System.Collections.Generic;

namespace BusterWood.Linq
{
    class EnumerableBatcher<T> : IBatcher<T>
    {
        readonly IEnumerator<T> source;

        public int BatchSize { get; }

        public EnumerableBatcher(IEnumerable<T> source, int batchSize)
        {
            this.source = source.GetEnumerator();
            BatchSize = batchSize;
        }

        public ArraySegment<T> NextBatch()
        {
            var batch = new T[BatchSize];
            for (int i = 0; i < batch.Length; i++)
            {
                if (source.MoveNext())
                    batch[i] = source.Current;
                else
                    return new ArraySegment<T>(batch, 0, i);
            }
            return new ArraySegment<T>(batch);
        }
    }


}
