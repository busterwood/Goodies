using BusterWood.Goodies;
using BusterWood.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BusterWood.Collections
{
    /// <summary>List of unique elements, which acts like a set in that you cannot add duplicates.</summary>
    /// <remarks>Performance is comparable to <see cref="HashSet{T}"/>.  Design was inspired by Python's 3.6 new dict</remarks>
    public class UniqueList<T> : IReadOnlyList<T>, ISet<T>, IReadOnlySet<T>, IList<T>
    {
        const int Lower31BitMask = 0x7FFFFFFF;
        const int FREE = -1;    // index is free, terminate probing when looking for an item
        const int DELETED = -2; // mark index with this to indicate a value WAS at this index, and continue probing when looking for an item
        int[] indexes;
        int[] hashCodes;
        T[] values;
        int count;

        /// <summary>Copies the <paramref name="other"/> list.</summary>
        public UniqueList(UniqueList<T> other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            indexes = other.indexes.Copy();
            hashCodes = other.hashCodes.Copy();
            values = other.values.Copy();
            count = other.count;
            Equality = other.Equality;
        }

        /// <summary>Create a new list, optionally setting an <see cref="IEqualityComparer{T}"/></summary>
        public UniqueList(IEqualityComparer<T> equality = null)
        {
            const int InitialSize = 3;
            indexes = new int[InitialSize+1];
            hashCodes = new int[InitialSize];
            values = new T[InitialSize];
            count = 0;
            Equality = equality ?? EqualityComparer<T>.Default;
        }

        /// <summary>The equality comparer used</summary>
        public IEqualityComparer<T> Equality { get; }

        /// <summary>The current capacity of the list, automatically grows if needed</summary>
        public int Capacity => values.Length;

        /// <summary>The number of items in the list</summary>
        public int Count => count;

        public bool IsReadOnly => false;

        /// <summary>Gets or sets the value stored at an <paramref name="index"/> in the list</summary>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is less than zero or greater that allowed</exception>
        /// <exception cref="ArgumentNullException">Throw if you try to set a value to null</exception>
        /// <exception cref="ArgumentException">Thrown when trying to set value if the value already exists in the list at a different <paramref name="index"/></exception>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException();
                return values[index];
            }
            set
            {
                if (index < 0 || index >= count)
                    throw new IndexOutOfRangeException();
                if (value == null)
                    throw new ArgumentNullException();

                var oldItem = values[index];
                var oldHC = PositiveHashCode(oldItem);
                var oldResult = FindSlot(oldItem, oldHC);
                Debug.Assert(oldResult.Found);
                var newHC = PositiveHashCode(value);
                var newResult = FindSlot(value, newHC);

                if (newResult.Found)
                {
                    // maybe setting will result in a duplicate
                    if (newResult.Slot != oldResult.Slot)
                        throw new ArgumentException($"Cannot set index {index} as this would result in a duplicate value");
                    values[index] = value;
                    hashCodes[index] = newHC;
                }
                else
                {
                    // no duplicate
                    SetIndex(oldResult.Slot, DELETED);
                    SetIndex(newResult.FirstFreeSlot, index);
                    values[index] = value;
                    hashCodes[index] = newHC;
                }
            }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Adds an <paramref name="item"/> to the list.  Returns TRUE if the <paramref name="item"/> was added or FALSE if the <paramref name="item"/> is already present in the list.
        /// </summary>
        public bool Add(T item)
        {
            if (item == null)
                return false; // dont allow null to be added, just say it is already there

            if (NeedToResize())
                Resize();

            var hc = PositiveHashCode(item);
            //var res = FindSlot(item, hc);
            // inline FindSlot for 25% better performance 
            FindResult res;
            {
                const int UNSET = -1;
                var slot = hc % indexes.Length;
                int firstFree = UNSET;
                for (;;)
                {
                    int valueIdx = GetIndex(slot);
                    if (valueIdx == FREE)
                    {
                        res = new FindResult(-1, firstFree == UNSET ? slot : firstFree);
                        break;
                    }
                    else if (valueIdx == DELETED)
                    {
                        if (firstFree == UNSET)
                            firstFree = slot;
                    }
                    else if (hc == hashCodes[valueIdx] && Equality.Equals(item, values[valueIdx]))
                    {
                        res = new FindResult(slot, firstFree);
                        break;
                    }

                    // another value is in that slot, try the next index
                    slot += 1;
                    if (slot == indexes.Length)
                        slot = 0;

                    // wrap around termination check no longer needed because there will now always be spare capacity
                }
            }

            if (res.Found)
                return false;

            // not found, add it
            values[count] = item;
            hashCodes[count] = hc;
            SetIndex(res.FirstFreeSlot, count);
            count++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool NeedToResize() => count + 1 >= values.Length;

        void Resize()
        {
            // we want 25% free space in indexes array to keep hashing efficient,
            // but hash codes and values arrays can be smaller than the indexes array
            // For example, to contain 100 items:
            // values = new T[100];
            // hash codes = new int[100];
            // indexes = new int[100 * 1.25 = 125];
            int newSize = (indexes.Length * 2) + 1;
            Array.Resize(ref hashCodes, newSize); 
            Array.Resize(ref values, newSize);

            // recreate indexes from existing values and hashcodes
            indexes = new int[(int)(newSize * 1.25f)];
            for (int i = 0; i < values.Length; i++)
            {
                if (i == count) break;
                var hc = hashCodes[i];
                SetIndexForValue(i, hc);
            }
        }

        void SetIndexForValue(int valueIndex, int hc)
        {
            int firstSlot = hc % indexes.Length;
            int slot = firstSlot;
            for (;;)
            {
                int valueIdx = GetIndex(slot);
                if (valueIdx < 0)
                {
                    SetIndex(slot, valueIndex);
                    break;
                }
                // another value is in that slot, try the next index
                if (++slot == indexes.Length)
                    slot = 0;

                // searched all possible entries and returned back to original slot, must be full, so resize
                Debug.Assert(slot != firstSlot, "all slots are full after resizing, this should not happen!");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int PositiveHashCode(T item) => item.GetHashCode() & Lower31BitMask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetIndex(int slot) => indexes[slot] - 1; // default value is 0, so always store one more than a real index

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetIndex(int slot, int idx) { indexes[slot] = idx + 1; }

        FindResult FindSlot(T item, int hc)
        {
            const int UNSET = -1;
            var slot = hc % indexes.Length;
            int firstFree = UNSET;
            for (;;)
            {
                int valueIdx = GetIndex(slot); 
                if (valueIdx == FREE)
                {
                    return new FindResult(-1, firstFree == UNSET ? slot : firstFree);
                }
                else if (valueIdx == DELETED)
                {
                    if (firstFree == UNSET)
                        firstFree = slot;
                }
                else if (hc == hashCodes[valueIdx] && Equality.Equals(item, values[valueIdx]))
                    return new FindResult(slot, firstFree);

                // another value is in that slot, try the next index
                slot += 1;
                if (slot == indexes.Length)
                    slot = 0;

                // wrap around termination check no longer needed because there will now always be spare capacity
            }
        }

        struct FindResult
        {
            public readonly int Slot;           // the slot the item was found in, or -1 if not found
            public readonly int FirstFreeSlot; // the slot to add to 
            public bool Found => Slot >= 0;

            public FindResult(int slot, int firstFree)
            {
                Slot = slot;
                FirstFreeSlot = firstFree;
            }
        }

        /// <summary>Adds then <paramref name="item"/> if not present, else replaces the existing <paramref name="item"/></summary>
        public void AddOrUpdate(T item)
        {
            if (item == null)
                return; // don't allow null to be added, considered as this.union(empty set)

            int index = IndexOf(item);
            if (index < 0)
                Add(item);
            else
                this[index] = item;
        }

        /// <summary>Return TRUE if the <paramref name="item"/> is present in the list, otherwise returns FALSE</summary>
        public bool Contains(T item)
        {
            if (item == null) return false;
            var hc = PositiveHashCode(item);
            var result = FindSlot(item, hc);
            return result.Found;
        }

        /// <summary>Returns the index of the <paramref name="item"/> in the list, or -1 if the <paramref name="item"/> is not present</summary>
        public int IndexOf(T item)
        {
            if (item == null) return -1;
            var hc = PositiveHashCode(item);
            var result = FindSlot(item, hc);
            return result.Found ? GetIndex(result.Slot) : -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            int i = 0;
            foreach (var v in values)
            {
                if (i == count) yield break;
                yield return v;
                i++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // AddRange
        public void UnionWith(IEnumerable<T> other)
        {
            foreach (var item in other)
                Add(item);
        }

        // in this AND other
        public void IntersectWith(IEnumerable<T> other)
        {
            var notInBoth = other.ToList(x => !Contains(x));
            ExceptWith(notInBoth);
        }

        // RemoveRange
        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var item in other)
                Remove(item);
        }

        // NOT in either this or other
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            var inBoth = other.ToList(x => Contains(x));
            ExceptWith(inBoth);
        }

        public bool IsSubsetOf(IEnumerable<T> other) => SetExtensions.IsSubsetOf(this, other);

        public bool IsSupersetOf(IEnumerable<T> other) => SetExtensions.IsSupersetOf(this, other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => SetExtensions.IsProperSupersetOf(this, other);

        public bool IsProperSubsetOf(IEnumerable<T> other) => SetExtensions.IsProperSubsetOf(this, other);

        public bool Overlaps(IEnumerable<T> other) => SetExtensions.Overlaps(this, other);

        public bool SetEquals(IEnumerable<T> other) => SetExtensions.SetEquals(this, other);

        /// <summary>Removes all items from the list</summary>
        public void Clear()
        {
            Array.Clear(indexes, 0, indexes.Length);
            Array.Clear(hashCodes, 0, count);
            Array.Clear(values, 0, count);
            count = 0;
        }

        /// <summary>Copies items to the <paramref name="array"/> starting at <paramref name="arrayIndex"/>.</summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            int toCopy = Math.Min(count, array.Length - arrayIndex);
            Array.Copy(values, 0, array, arrayIndex, toCopy);
        }

        /// <summary>Not supported</summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Insert(int index, T item) => throw new NotImplementedException();

        /// <summary>
        /// Removes a <paramref name="item"/> from the list, returns TRUE if the <paramref name="item"/> was removed, FALSE if the <paramref name="item"/> was not found.
        /// </summary>
        public bool Remove(T item)
        {
            var hc = PositiveHashCode(item);
            var res = FindSlot(item, hc);
            if (!res.Found)
                return false; // item not found in dictionary

            // it was found in index[slot]
            var idx = GetIndex(res.Slot);
            SetIndex(res.Slot, DELETED); // slot now free but mark it as deleted so probing will skip over it when finding values

            // shift values and hash codes down one, and clear last entry
            int newCount = count - 1;
            if (idx+1 < count)
            {
                Array.Copy(hashCodes, idx + 1, hashCodes, idx, newCount - idx);
                Array.Copy(values, idx + 1, values, idx, newCount - idx);
            }
            hashCodes[newCount] = 0;
            values[newCount] = default(T);

            // the slow bit, reduce by one each entry of the indexes array whose value is greater than the index we just deleted
            int search = idx + 1;
            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] > search)
                    indexes[i] -= 1;
            }
            count -= 1;
            return true;
        }

        /// <summary>Removes an item at a <paramref name="index"/></summary>
        public void RemoveAt(int index)
        {
            var item  = this[index];
            Remove(item);
        }

        /// <summary>Creates a copy of this list</summary>
        public UniqueList<T> Copy() => new UniqueList<T>(this);
    }
}
