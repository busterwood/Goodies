using System.Collections.Generic;

namespace BusterWood.Caching
{
    /// <summary>A cache you add items to</summary>
    public interface ICache<TKey, TValue> : IDataSource<TKey, TValue>
    {
        /// <summary>The underlying object used for locking.  Allows the caller to add or remove more that one key/value at a time</summary>
        object SyncRoot { get; }

        /// <summary>The number of items currently in the cache</summary>
        int Count { get; }

        /// <summary>Gets or sets the value associated with a <paramref name="key"/></summary>
        /// <returns>The value for the key, or the default value (null) when the key is not found</returns>
        new TValue this[TKey key] { get; set; }

        /// <summary>Removes all keys and values from the cache</summary>
        void Clear();

        /// <summary>Removes a <param name="key"/> (and value) from the cache, if it exists.</summary>
        void Remove(TKey key);

        /// <summary>Optional callback during the eviction process.  Maybe useful if the evicted items need disposing</summary>
        event EvictionHandler<TKey, TValue> Evicted;
    }

    public delegate void EvictionHandler<TKey, TValue>(object sender, IReadOnlyDictionary<TKey, TValue> evicted);

    /// <summary>A <see cref="Cache{TKey, TValue}"/> that supports reading batches of data</summary>
    public interface IBatchCache<TKey, TValue> : ICache<TKey, TValue>, IBatchDataSource<TKey, TValue>
    {
    }
}
