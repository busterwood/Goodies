using System;
using System.Collections.Generic;

namespace BusterWood.Collections
{
    public static class DictionaryExtensions
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

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;
            if (!dic.TryGetValue(key, out value))
            {
                value = valueFactory(key);
                dic.Add(key, value);
            }
            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue @default = default(TValue))
        {
            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : @default;
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

    }
}