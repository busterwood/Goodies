using System;
using System.Collections.Generic;

namespace BusterWood.Collections
{
    public static partial class Extensions
    {
        /// <summary>
        /// Tries to get a value from <paramref name="dictionary"/>.  Returns TRUE if the value was already in the <paramref name="dictionary"/>,
        /// otherwise creates the <paramref name="value"/> using <paramref name="valueFactory"/>, adds it to the <paramref name="dictionary"/> and returns FALSE.
        /// </summary>
        public static bool TryGetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, out TValue value)
        {
            if (dictionary.TryGetValue(key, out value))
            {
                return true;
            }
            else
            {
                value = valueFactory(key);
                dictionary.Add(key, value);
                return false;
            }
        }

        /// <summary>
        /// Get a existing value from <paramref name="dictionary"/> or adds a value to the <paramref name="dictionary"/> using the <paramref name="valueFactory"/>.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
            {
                value = valueFactory(key);
                dictionary.Add(key, value);
            }
            return value;
        }

        /// <summary>
        /// Get a existing value from <paramref name="dictionary"/> returns the default value for TValue (null, 0, etc).
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue @default = default(TValue))
        {
            return dictionary.TryGetValue(key, out TValue result) ? result : @default;
        }

        public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            foreach (var k in keys)
            {
                dictionary.Remove(k);
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var pair in items)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items, IEqualityComparer<T> comparer = null) => new HashSet<T>(items, comparer);
    }
}