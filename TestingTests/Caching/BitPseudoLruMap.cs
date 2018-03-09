using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BusterWood.Caching
{
    public class BitPseudoLruMap<TKey, TValue> : ICache<TKey, TValue>
    {
        readonly IDataSource<TKey, TValue> _nextLevel;
        readonly object _lock;
        readonly int _capacity;
        readonly IEqualityComparer<TKey> comparer;
        readonly IEqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;
        int[] buckets;
        Entry[] entries;
        BitArray recent;
        int count;
        int freeList;
        int freeCount;

        private struct Entry
        {
            public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            public int next;        // Index of next entry, -1 if last
            public TKey key;           // Key of entry
            public TValue value;         // Value of entry
        }
    
        public BitPseudoLruMap(IDataSource<TKey, TValue> nextLevel, int capacity)
        {
            if (nextLevel == null)
                throw new ArgumentNullException(nameof(nextLevel));
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Value must be one or more");
            _nextLevel = nextLevel;
            _capacity = capacity;
            _lock = new object();
            comparer = EqualityComparer<TKey>.Default;
        }

        public int Count => count;

        public object SyncRoot { get { throw new NotImplementedException(); } }

        /// <summary>Tries to get a value from this cache, or load it from the underlying cache</summary>
        /// <param name="key">Teh key to find</param>
        /// <returns>The value found, or default(T) if not found</returns>
        public TValue this[TKey key]
        {
            get
            {
                lock (_lock)
                {
                    int i = FindEntry(key);
                    if (i >= 0)
                    {
                        recent[i] = true;
                        return entries[i].value;
                    }
                }

                // key not found by this point
                var loaded = _nextLevel[key]; // NOTE: possible blocking
                if (valueComparer.Equals(loaded, default(TValue))) 
                    return loaded;

                lock (_lock)
                {
                    // about to add, check the limit
                    if (count >= _capacity)
                    {
                        TKey lru = FindLeastRecentlyUsed();
                        Remove(lru);
                    }

                    // a new item in the cache
                    Insert(key, loaded, false);
                    return loaded;
                }
            }
            set
            {
                lock(_lock)
                {
                    // about to add, check the limit
                    if (count >= _capacity)
                    {
                        TKey lru = FindLeastRecentlyUsed();
                        Remove(lru);
                    }
                    Insert(key, value, false);
                }
            }
        }

        private TKey FindLeastRecentlyUsed()
        {
            int first = -1;
            int lru = -1;

            
            for (int i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    if (first < 0)
                        first = i;
                    if (lru < 0 && recent[i] == false)
                    {
                        lru = i;
                        break;
                    }
                }
            }
            recent.SetAll(false);
            if (lru >= 0)
                return entries[lru].key;

            // no entry marked, just use the first one we find
            return entries[first].key;
        }

        private int FindEntry(TKey key)
        {
            if (key == null)
                ThrowArgumentNullException(nameof(key));

            if (buckets != null)
            {
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
                {
                    if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                        return i;
                }
            }
            return -1;
        }

        static void ThrowArgumentNullException(string name)
        {
            throw new ArgumentNullException(name);
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null)
            {
                ThrowArgumentNullException(nameof(key));
            }

            if (buckets == null) Initialize(0);
            int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;

            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].next)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    if (add)
                        ThrowArgumentException("Cannot add as key already exists");
                    entries[i].value = value;
                    recent[i] = true;
                    return;
                }

            }
            int index;
            if (freeCount > 0)
            {
                index = freeList;
                freeList = entries[index].next;
                freeCount--;
            }
            else
            {
                if (count == entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % buckets.Length;
                }
                index = count;
                count++;
            }

            entries[index].hashCode = hashCode;
            entries[index].next = buckets[targetBucket];
            entries[index].key = key;
            entries[index].value = value;
            recent[index] = true;
            buckets[targetBucket] = index;
        }

        private void Initialize(int capacity)
        {
            int size = GetPrime(capacity);
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++)
                buckets[i] = -1;
            entries = new Entry[size];
            recent = new BitArray(size);
            freeList = -1;
        }

        static void ThrowArgumentException(string text)
        {
            throw new ArgumentException(text);
        }

        private void Resize()
        {
            Resize(ExpandPrime(count), false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            Debug.Assert(newSize >= entries.Length);

            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            Entry[] newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);
            if (forceNewHashCodes)
            {
                for (int i = 0; i < count; i++)
                {
                    if (newEntries[i].hashCode != -1)
                    {
                        newEntries[i].hashCode = (comparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF);
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                if (newEntries[i].hashCode >= 0)
                {
                    int bucket = newEntries[i].hashCode % newSize;
                    newEntries[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            buckets = newBuckets;
            entries = newEntries;

            // resize the recent bits
            var ints = ((newSize - 1) / 32) + 1;
            var temp = new int[ints];
            recent.CopyTo(temp, 0);
            recent = new BitArray(temp);
        }

        public void Remove(TKey key)
        {
            if (key == null)
            {
                ThrowArgumentNullException(nameof(key));
            }

            if (buckets == null)
                return;

            int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int bucket = hashCode % buckets.Length;
            int last = -1;
            for (int i = buckets[bucket]; i >= 0; last = i, i = entries[i].next)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    if (last < 0)
                    {
                        buckets[bucket] = entries[i].next;
                    }
                    else
                    {
                        entries[last].next = entries[i].next;
                    }
                    entries[i].hashCode = -1;
                    entries[i].next = freeList;
                    entries[i].key = default(TKey);
                    entries[i].value = default(TValue);
                    recent[i] = false;
                    freeList = i;
                    freeCount++;
                    return;
                }
            }
            return;
        }

        public static readonly int[] primes = {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};

        public event EvictionHandler<TKey, TValue> Evicted;

        public static int GetPrime(int min)
        {
            if (min < 0)
                ThrowArgumentException("Overflow");

            for (int i = 0; i < primes.Length; i++)
            {
                int prime = primes[i];
                if (prime >= min) return prime;
            }

            //outside of our predefined table. 
            //compute the hard way. 
            for (int i = (min | 1); i < Int32.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % 101 != 0))
                    return i;
            }
            return min;
        }

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                        return false;
                }
                return true;
            }
            return (candidate == 2);
        }

        public static int GetMinPrime()
        {
            return primes[0];
        }

        // Returns size of hashtable to grow to.
        public static int ExpandPrime(int oldSize)
        {
            const int MaxPrimeArrayLength = 0x7FEFFFFD;
            int newSize = 2 * oldSize;

            // Allow the hashtables to grow to maximum possible size (~2G elements) before encoutering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
            {
                return MaxPrimeArrayLength;
            }

            return GetPrime(newSize);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Task<TValue> GetAsync(TKey key)
        {
            throw new NotImplementedException();
        }
    }


}