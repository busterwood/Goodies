using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    /// <summary>
    /// A cache map that uses generations to cache to minimize the per-key overhead.
    /// A collection releases all items in Gen1 and moves Gen0 -> Gen1.  Reading an item in Gen1 promotes the item back to Gen0.
    /// </summary>
    /// <remarks>This version REMEMBERS cache misses</remarks>
    public class ReadThroughCache<TKey, TValue> : GenerationalBase<TKey>, ICache<TKey, TValue>
    {
        readonly IDataSource<TKey, TValue> _dataSource;      // the underlying source of values
        internal Dictionary<TKey, TValue> _gen0; // internal for test visibility
        internal Dictionary<TKey, TValue> _gen1; // internal for test visibility       
    
        /// <summary>Create a new read-through cache that has a Gen0 size limit and/or a periodic collection time</summary>
        /// <param name="dataSource">The underlying source to load data from</param>
        /// <param name="gen0Limit">(Optional) limit on the number of items allowed in Gen0 before a collection</param>
        /// <param name="timeToLive">(Optional) time period after which a unread item is evicted from the cache</param>
        public ReadThroughCache(IDataSource<TKey, TValue> dataSource, int? gen0Limit, TimeSpan? timeToLive)
            : base(gen0Limit, timeToLive)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            _dataSource = dataSource;
            _gen0 = new Dictionary<TKey, TValue>();
        }
       
        public event EvictionHandler<TKey, TValue> Evicted;

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                int start;
                lock (SyncRoot)
                {
                    start = Version;
                    if (TryGetAnyGen(key, out result))
                        return result;
                }

                // key not found by this point, read-through to the data source *outside* of the lock as this may take some time, i.e. network or file access
                var loaded = _dataSource[key];

                lock (SyncRoot)
                {
                    // another thread may have added the value for our key so double-check
                    if (Version != start && TryGetAnyGen(key, out result))
                        return result;

                    AddToGen0(key, loaded); // store the value (or the fact that the data source does *not* contain the value
                    return loaded;
                }
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
                        AddToGen0(key, value);
                    }
                }
            }
        }

        /// <summary>Gets the <paramref name="value"/> for a <paramref name="key"/> from Gen0 or Gen1</summary>
        protected internal bool TryGetAnyGen(TKey key, out TValue value)
        {
            if (_gen0.TryGetValue(key, out value))
                return true;

            if (_gen1?.TryGetValue(key, out value) == true)
            {
                PromoteGen1ToGen0(key, value);
                return true;
            }
            return false;
        }

        void PromoteGen1ToGen0(TKey key, TValue value)
        {
            _gen1.Remove(key);
            _gen0.Add(key, value);
        }

        protected internal void AddToGen0(TKey key, TValue loaded)
        {
            // about to add, check the limit
            if (_gen0.Count >= Gen0Limit)
                Collect();

            // a new item in the cache
            _gen0.Add(key, loaded);
            unchecked { Version++; }
        }

        protected override int CountCore() => _gen0.Count + (_gen1?.Count).GetValueOrDefault();

        protected override void CollectCore()
        {
            if ((_gen1?.Count).GetValueOrDefault() > 0)
            {
                EvictionCount += _gen1.Count;
                Evicted?.Invoke(this, _gen1);
            }
            _gen1 = _gen0; // Gen1 items are dropped from the cache at this point
            _gen0 = new Dictionary<TKey, TValue>(); // Gen0 is now empty, we choose not to re-use Gen1 dictionary so the memory can be GC'd
        }

        /// <summary>Tries to get a value from this cache, or load it from the underlying cache</summary>
        /// <param name="key">The key to find</param>
        /// <returns>The value found, or default(T) if not found</returns>
        public async Task<TValue> GetAsync(TKey key)
        {
            int start;
            TValue value;
            lock (SyncRoot)
            {
                start = Version;
                if (TryGetAnyGen(key, out value))
                    return value;
            }

            // key not found by this point, read-through to the data source *outside* of the lock as this may take some time, i.e. network or file access
            var loaded = await _dataSource.GetAsync(key);

            lock (SyncRoot)
            {
                if (Version != start && TryGetAnyGen(key, out value))
                    return value;

                AddToGen0(key, loaded);
                value = loaded;
                return value;
            }
        }

        public void Remove(TKey key)
        {
            lock (SyncRoot)
            {
                if (!_gen0.Remove(key))
                    _gen1?.Remove(key);
            }
        }

        /// <summary>Returns a dictionary containing all the keys and values currently in the cache</summary>
        public Dictionary<TKey, TValue> SnapshotValues()
        {
            lock (SyncRoot)
            {
                var result = new Dictionary<TKey, TValue>(CountCore());
                result.AddRange(_gen0);
                if (_gen1 != null)
                    result.AddRange(_gen1);
                return result;
            }
        }

        /// <summary>used to detect other threads modifying the cache</summary>
        protected internal int Version { get; set; }
    }
}