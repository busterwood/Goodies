// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Collections
{
    class Grouping<TKey, TElement> : IGrouping<TKey, TElement>, IList<TElement>, IReadOnlyCollection<TElement>
    {
        internal TKey _key;
        internal int _hashCode;
        internal TElement[] _elements;
        internal int _count;
        internal Grouping<TKey, TElement> _hashNext;
        internal Grouping<TKey, TElement> _next;

        internal void Add(TElement element)
        {
            if (_elements.Length == _count)
            {
                Array.Resize(ref _elements, checked(_count * 2));
            }
            _elements[_count] = element;
            _count++;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _elements[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Gets the key of the <see cref="T:System.Linq.IGrouping`2"/>.</summary>
        /// <returns>The key of the <see cref="T:System.Linq.IGrouping`2"/>.</returns>
        public TKey Key => _key;

        public int Count => _count;

        bool ICollection<TElement>.IsReadOnly => true;
        
        public bool Contains(TElement item) => Array.IndexOf(_elements, item, 0, _count) >= 0;

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            Array.Copy(_elements, 0, array, arrayIndex, _count);
        }

        public int IndexOf(TElement item) => Array.IndexOf(_elements, item, 0, _count);

        public TElement this[int index]
        {
            get
            {
                if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
                return _elements[index];
            }
            set { throw new NotSupportedException(); }
        }

        void ICollection<TElement>.Add(TElement item)
        {
            throw new NotSupportedException();
        }

        void ICollection<TElement>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<TElement>.Remove(TElement item)
        {
            throw new NotSupportedException(); 
        }

        void IList<TElement>.Insert(int index, TElement item)
        {
            throw new NotSupportedException();
        }

        void IList<TElement>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
    }

}