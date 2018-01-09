using System;
using System.Collections.Generic;

namespace BusterWood.Caching
{
    static class DictionaryExtensions
    {
        //public static Maybe<TValue> Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        //{
        //    TValue value;
        //    var got = dictionary.TryGetValue(key, out value);
        //    return got ? Maybe.Some(value) : Maybe.None<TValue>();
        //}

        /// <summary>
        /// Tries to get a value from <paramref name="dictionary"/>.  Returns TRUE if the value was already in the <paramref name="dictionary"/>,
        /// otherwise creates the <paramref name="value"/> using <paramref name="valueFactory"/>, adds it to the <paramref name="dictionary"/> and returns FALSE.
        /// </summary>
        public static bool TryGetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, out TValue value)
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

        public static void RemoveRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            foreach (var k in keys)
            {
                dictionary.Remove(k);
            }
        }

        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var pair in items)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
        }

    }
}