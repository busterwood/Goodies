using System.Threading.Tasks;

namespace BusterWood.Caching
{
    /// <summary>A readonly source of data</summary>
    public interface IDataSource<TKey, TValue>
    {
        /// <summary>Gets the value associated with a <paramref name="key"/></summary>
        /// <returns>The value for the key, or the default value (null) when the key is not found</returns>
        TValue this[TKey key] { get; }

        /// <summary>Tries to get a value for a key</summary>
        /// <param name="key">The key to find</param>
        /// <returns>The value for the key if the item was found in the this cache, otherwise returns the default (null for reference types)</returns>
        Task<TValue> GetAsync(TKey key);
    }
}
