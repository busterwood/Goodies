// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using BusterWood.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Collections
{
    /// <summary>
    /// A key to many value dictionary data type
    /// </summary>
    public class HashLookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        static readonly List<TElement> Empty = new List<TElement>();
        readonly IEqualityComparer<TKey> _comparer;
        Grouping<TKey, TElement>[] _groupings;
        Grouping<TKey, TElement> _lastGrouping;
        int _count;

        public HashLookup(IEqualityComparer<TKey> comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
            _groupings = new Grouping<TKey, TElement>[7];
        }

        /// <summary>Gets the number of groupings (unique keys) in the <see cref="HashLookup{TKey,TElement}"/>.</summary>
        /// <remarks>Does NOT return the number of items in the lookup</remarks>
        public int Count => _count;

        /// <summary>
        /// Add an <paramref name="element"/> to the lookup using supplied <paramref name="key"/>
        /// </summary>
        public void Add(TKey key, TElement element)
        {
            GetOrAddGrouping(key).Add(element);
        }

        /// <summary>
        /// Add some <paramref name="items"/> to the lookup using the <paramref name="keyFunc"/> to get the key for each element
        /// </summary>
        public void AddRange(IEnumerable<TElement> items, Func<TElement, TKey> keyFunc)
        {
            Contract.Requires(items != null);
            Contract.Requires(keyFunc != null);

            TKey lastKey = default(TKey);
            Grouping<TKey, TElement> grouping = null;
            foreach (var element in items)
            {
                var key = keyFunc(element);
                if (grouping == null || _comparer.Equals(key, lastKey) == false)
                    grouping = GetOrAddGrouping(key);
                grouping.Add(element);
                lastKey = key;
            }
        }

        /// <summary>Gets the <see cref="T:System.Collections.Generic.IEnumerable`1"/> sequence of values indexed by a specified key.</summary>
        /// <returns>The <see cref="T:System.Collections.Generic.IEnumerable`1"/> sequence of values indexed by the specified key.</returns>
        /// <param name="key">The key of the desired sequence of values.</param>
        /// <remarks>Returns an empty sequence if the key is not present</remarks>
        IEnumerable<TElement> ILookup<TKey, TElement>.this[TKey key]
        {
            get
            {
                int hashCode = InternalGetHashCode(key);
                Grouping<TKey, TElement> grouping = FindGrouping(key, hashCode);
                var result = grouping ?? (IEnumerable<TElement>) Empty;
                Contract.Ensures(result != null);
                return result;
            }
        }

        /// <summary>Gets the <see cref="T:System.Collections.Generic.IEnumerable`1"/> sequence of values indexed by a specified key.</summary>
        /// <returns>The <see cref="T:System.Collections.Generic.IEnumerable`1"/> sequence of values indexed by the specified key.</returns>
        /// <param name="key">The key of the desired sequence of values.</param>
        /// <remarks>Returns an empty sequence if the key is not present</remarks>
        public IReadOnlyList<TElement> this[TKey key]
        {
            get
            {
                int hashCode = InternalGetHashCode(key);
                Grouping<TKey, TElement> grouping = FindGrouping(key, hashCode);
                var result = grouping ?? (IReadOnlyList<TElement>)Empty;
                Contract.Ensures(result != null);
                return result;
            }
        }

        /// <summary>
        /// Determines whether a specified key exists in the <see cref="HashLookup{TKey,TElement}"/>.
        /// </summary>
        public bool Contains(TKey key)
        {
            int hashCode = InternalGetHashCode(key);
            return FindGrouping(key, hashCode) != null;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            Grouping<TKey, TElement> g = _lastGrouping;
            if (g != null)
            {
                do
                {
                    g = g._next;
                    yield return g;
                }
                while (g != _lastGrouping);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal int InternalGetHashCode(TKey key) => _comparer.GetHashCode(key) & 0x7FFFFFFF;

        internal Grouping<TKey, TElement> GetOrAddGrouping(TKey key)
        {
            int hashCode = InternalGetHashCode(key);
            var grouping = FindGrouping(key, hashCode);
            return grouping ?? AddGrouping(key, hashCode);
        }

        Grouping<TKey, TElement> FindGrouping(TKey key, int hashCode)
        {
            for (Grouping<TKey, TElement> grouping = _groupings[hashCode % _groupings.Length]; grouping != null; grouping = grouping._hashNext)
            {
                if (grouping._hashCode == hashCode && _comparer.Equals(grouping._key, key))
                {
                    return grouping;
                }
            }
            return null;
        }

        Grouping<TKey, TElement> AddGrouping(TKey key, int hashCode)
        {
            if (_count == _groupings.Length)
            {
                Resize();
            }

            int index = hashCode % _groupings.Length;
            var newGrouping = new Grouping<TKey, TElement>
            {
                _key = key,
                _hashCode = hashCode,
                _elements = new TElement[1],
                _hashNext = _groupings[index]
            };
            _groupings[index] = newGrouping;
            if (_lastGrouping == null)
            {
                newGrouping._next = newGrouping;
            }
            else
            {
                newGrouping._next = _lastGrouping._next;
                _lastGrouping._next = newGrouping;
            }

            _lastGrouping = newGrouping;
            _count++;
            return newGrouping;
        }

        void Resize()
        {
            int newSize = checked((_count * 2) + 1);
            var newGroupings = new Grouping<TKey, TElement>[newSize];
            var g = _lastGrouping;
            do
            {
                g = g._next;
                int index = g._hashCode % newSize;
                g._hashNext = newGroupings[index];
                newGroupings[index] = g;
            }
            while (g != _lastGrouping);

            _groupings = newGroupings;
        }
    }


    public static partial class Extensions
    {
        public static HashLookup<TKey, TValue> ToHashLookup<TKey, TValue>(this IEnumerable<TValue> source, Func<TValue, TKey> keyFunc)
        {
            Contract.Requires(source != null);
            var lookup = new HashLookup<TKey, TValue>();
            lookup.AddRange(source, keyFunc);
            Contract.Ensures(lookup != null);
            return lookup;
        }
    }
}

