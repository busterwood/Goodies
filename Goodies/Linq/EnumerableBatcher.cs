using System.Collections.Generic;

namespace BusterWood.Linq
{
    class EnumerableBatcher<T> : IBatcher<T>
    {
        readonly IEnumerable<T> source;

        public int BatchSize { get; }

        public EnumerableBatcher(IEnumerable<T> source, int batchSize)
        {
            this.source = source;
            BatchSize = batchSize;
        }

        public IBatchEnumerator<T> GetBatchEnumerator() => new Enumerator(source.GetEnumerator(), BatchSize);

        public class Enumerator : IBatchEnumerator<T>
        {
            readonly IEnumerator<T> source;
            int sourceIndex;

            public int BatchSize { get; }

            public Enumerator(IEnumerator<T> source, int batchSize)
            {
                this.source = source;
                BatchSize = batchSize;
            }

            public bool NextBatch(T[] batch, out int count)
            {
                for (count = 0; count < batch.Length; count++)
                {
                    if (source.MoveNext())
                        batch[count] = source.Current;
                    else
                        break;
                }
                return count > 0;
            }
        }
    }
}
