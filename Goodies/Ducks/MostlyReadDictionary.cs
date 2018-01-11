using System;
using System.Collections.Generic;
using System.Threading;

namespace BusterWood.Ducks
{
    /// <summary>A dictionary that is mostly read from, hardly ever written too</summary>
    class MostlyReadDictionary<TKey, TValue>
    {
        readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> addFunc)
        {
            if (addFunc == null)
                throw new ArgumentNullException(nameof(addFunc));

            // simple case of we already have a value for the key
            _rwLock.EnterReadLock();
            try
            {
                if (_map.TryGetValue(key, out var value)) return value;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            // no value for the key
            _rwLock.EnterUpgradeableReadLock();
            try
            {
                // some other thread may have just added it
                if (_map.TryGetValue(key, out var value)) return value;

                // the function may take "some time" so evaluate it outside of the write lock so other threads can still read the dictionary
                value = addFunc(key);

                _rwLock.EnterWriteLock();
                try
                {
                    _map[key] = value;
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }
                return value;
            }
            finally
            {
                _rwLock.ExitUpgradeableReadLock();
            }
        }
    }
}