using System;
using System.Data.Common;
using BusterWood.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Data
{
    public class DbDataReaderAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly DbDataReader _source;
        private readonly Func<DbDataReader, T> _transform;

        public DbDataReaderAsyncEnumerator(DbDataReader source, Func<DbDataReader, T> transform)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        }

        public T Current { get; private set; }

        object IAsyncEnumerator.Current => Current;

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            bool hasNext = await _source.ReadAsync(cancellationToken);
            Current = hasNext ? _transform(_source) : default(T); 
            return hasNext;
        }

        public void Dispose()
        {
            // do we really want to dispose of the reader?  what if there is another result set?
            //_source.Dispose();
        }
    }
}
