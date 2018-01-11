using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    /// <summary>
    /// A cache-aside generational cache that will evict batches of least recently used items after some time and/or some size limit has been reached.
    /// </summary>
    public class Cache<TKey, TValue> : GenerationalBase<TKey>, ICache<TKey, TValue>
    {
        internal Dictionary<TKey, TValue> _gen0; // internal for test visibility
        internal Dictionary<TKey, TValue> _gen1; // internal for test visibility

        /// <summary>Create a new cache that has a Gen0 size limit and/or a periodic collection time</summary>
        /// <param name="gen0Limit">(Optional) limit on the number of items allowed in Gen0 before a collection</param>
        /// <param name="timeToLive">(Optional) time period after which a unread item is evicted from the cache</param>
        public Cache(int? gen0Limit, TimeSpan? timeToLive) : base(gen0Limit, timeToLive)
        {
            _gen0 = new Dictionary<TKey, TValue>();
        }

        /// <summary>Optional callback during the eviction process.  Maybe useful if the evicted items need disposing</summary>
        public event EvictionHandler<TKey, TValue> Evicted;

        /// <summary>Gets or sets a value for a key</summary>
        /// <returns>The returned value can be default (e.g. null) when the cache does not contain a value for the <paramref name="key"/></returns>
        /// <remarks>Setting a value may cause eviction if the size limit is reached</remarks>
        public TValue this[TKey key]
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_gen0.TryGetValue(key, out var value))
                        return value;

                    if (_gen1?.TryGetValue(key, out value) == true)
                    {
                        PromoteGen1ToGen0(key, value);
                        return value;
                    }
                }
                return default(TValue);
            }
            set
            {
                lock (SyncRoot)
                {
                    if (_gen0.ContainsKey(key))
                    {
                        // replace existing value in Gen0
                        _gen0[key] = value;
                    }
                    else if (_gen1?.ContainsKey(key) == true)
                    {
                        // Remove key from Gen1, replace with new value in Gen0
                        _gen1.Remove(key);
                        _gen0[key] = value;
                    }
                    else
                    {
                        // this may cause a collection and eviction
                        AddToGen0(key, value);
                    }
                }
            }
        }
        
        void PromoteGen1ToGen0(TKey key, TValue value)
        {
            _gen1.Remove(key);
            _gen0.Add(key, value);
        }

        void AddToGen0(TKey key, TValue loaded)
        {
            // about to add, check the limit
            if (_gen0.Count >= Gen0Limit)
                Collect();

            // a new item in the cache
            _gen0.Add(key, loaded);
        }

        protected override sealed void CollectCore()
        {
            if ((_gen1?.Count).GetValueOrDefault() > 0)
            {
                EvictionCount += _gen1.Count;
                Evicted?.Invoke(this, _gen1);
            }
            _gen1 = _gen0; // Gen1 items are dropped from the cache at this point
            _gen0 = new Dictionary<TKey, TValue>(); // Gen0 is now empty, we choose not to re-use Gen1 dictionary so the memory can be GC'd
        }

        protected override sealed int CountCore() => _gen0.Count + (_gen1?.Count).GetValueOrDefault();

        /// <summary>Tries to get a value for a key</summary>
        /// <param name="key">The key to find</param>
        /// <returns>The value for the key if the item was found in the this cache, otherwise returns the default (null for reference types)</returns>
        /// <remarks>Runs synchronously in this implementation</remarks>
        public Task<TValue> GetAsync(TKey key) => Task.FromResult(this[key]);

        /// <summary>Removes the key and associated value from the cache</summary>
        public void Remove(TKey key)
        {
            lock(SyncRoot)
            {
                if (!_gen0.Remove(key))
                    _gen1?.Remove(key);
            }
        }
        
    }
}