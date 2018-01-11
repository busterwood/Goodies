using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    /// <summary>A data source that supports loading batches of data</summary>
    public interface IBatchDataSource<TKey, TValue> : IDataSource<TKey, TValue>
    {
        /// <summary>Tries to get the values associated with the <paramref name="keys"/></summary>
        /// <param name="keys">The keys to find</param>
        /// <returns>An array the same size as the input <paramref name="keys"/> that contains the value for each key in the corresponding index</returns>
        TValue[] GetBatch(IReadOnlyCollection<TKey> keys);

        /// <summary>Tries to get the values associated with the <paramref name="keys"/> asynchronously</summary>
        /// <param name="keys">The keys to find</param>
        /// <returns>An array the same size as the input <paramref name="keys"/> that contains the value for each key in the corresponding index</returns>
        Task<TValue[]> GetBatchAsync(IReadOnlyCollection<TKey> keys);
    }
}
