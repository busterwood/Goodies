using BusterWood.Collections;
using BusterWood.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BusterWood.Linq
{
    public static class Enumerables
    {
        /// <summary>Creation of a list with a predicate.</summary>
        public static List<T> ToList<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var lst = new List<T>();
            foreach (var item in source)
            {
                if (predicate(item))
                    lst.Add(item);
            }
            return lst;
        }

        /// <summary>returns up to <paramref name="count"/> values from the <paramref name="source"/>, padding the result with <paramref name="default"/> values</summary>
        public static T[] ToArrayOf<T>(this IEnumerable<T> source, int count, T @default = default(T))
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // short cut if passed an array of the correct size
            var a = source as T[];
            if (a != null && a.Length == count)
                return a;

            // create a new array and populate it
            var result = new T[count];
            var e = source.GetEnumerator();
            int i = 0;
            for (; i < count && e.MoveNext(); i++)
                result[i] = e.Current;
            for (;  i < count; i++)
                result[i] = @default;
            return result;
        }

        /// <summary>returns up to <paramref name="count"/> values from the <paramref name="source"/>, padding the result with <paramref name="default"/> values</summary>
        public static List<T> ToListOf<T>(this IEnumerable<T> source, int count, T @default = default(T))
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // short cut if passed an list of the correct size
            var lst = source as List<T>;
            if (lst != null && lst.Count == count)
                return lst;

            // create a new list and populate it
            var result = new List<T>(count);
            result.AddRange(source.Take(count));
            while (result.Count < count)
                result.Add(@default);
            return result;
        }

        public static IEnumerable<IEnumerable<T>> SplitOn<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var group = new List<T>();
            foreach (var item in source)
            {
                if (predicate(item) && group.Count > 0)
                {
                    yield return group.ToArray();
                    group.Clear();
                }
                group.Add(item);
            }
            if (group.Count > 0)
                yield return group.ToArray();
        }

        public static IEnumerable<T> Concat<T>(this T head, IEnumerable<T> rest)
        {
            yield return head;
            foreach (var i in rest)
                yield return i;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> rest, T tail)
        {
            foreach (var i in rest)
                yield return i;
            yield return tail;
        }

        /// <summary>Returns a sequence of integers from <paramref name="start"/> up to and including <paramref name="end"/></summary>
        public static IEnumerable<int> To(this int start, int end)
        {
            for (int i = start; i <= end; i++)
                yield return i;
        }

        /// <summary>Returns a sequence of <paramref name="count"/> integers starting with <paramref name="start"/></summary>
        public static IEnumerable<int> For(this int start, int count)
        {
            int end = start + count;
            for (int i = start; i < end; i++)
                yield return i;
        }

        /// <summary>Returns the input items without the first one</summary>
        public static IEnumerable<T> Head<T>(this IEnumerable<T> source, int count = 1) => source.Take(count);

        /// <summary>Returns the input items without the first one</summary>
        public static IEnumerable<T> Rest<T>(this IEnumerable<T> source, int skip = 1) => source.Skip(skip);

        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int times) => Enumerable.Repeat(source, times).SelectMany(x => x);

        /// <summary>Returns results of the <paramref name="chooser"/> function that return a value (the result of the function is not null)</summary>
        public static IEnumerable<TResult> Choose<T, TResult>(this IEnumerable<T> items, Func<T, TResult> chooser) where TResult : class
            => items.Select(chooser).Where(res => res != null);

        /// <summary>Returns results of the <paramref name="chooser"/> function that return a value (the result of the function is not null)</summary>
        public static IEnumerable<TResult> Choose<T, TResult>(this IEnumerable<T> items, Func<T, TResult?> chooser) where TResult : struct
            => items.Select(chooser).Where(res => res.HasValue).Select(res => res.Value);        /// <summary>Returns results of the <paramref name="chooser"/> function that return a value (the result of the function is not null)</summary>

        /// <summary>Applies the given function to successive elements, returning the first value where the <paramref name="chooser"/> function returns a some value (the result of the function is not null)</summary>
        public static TResult Pick<T, TResult>(this IEnumerable<T> items, Func<T, TResult> chooser) where TResult : class
            => items.Select(chooser).FirstOrDefault(res => res != null);

        /// <summary>Applies the given function to successive elements, returning the first value where the <paramref name="chooser"/> function returns a some value (the result of the function is not null)</summary>
        public static TResult? Pick<T, TResult>(this IEnumerable<T> items, Func<T, TResult?> chooser) where TResult : struct
            => items.Select(chooser).FirstOrDefault(res => res.HasValue);

        /// <summary>
        /// Returns a sequence that yields sliding windows of containing elements drawn from the input sequence. 
        /// Each window is returned as a fresh array.
        /// </summary>
        /// <exception cref="ArgumentNullException">thrown when input sequence is null</exception>
        /// <exception cref="ArgumentException">thrown when the input sequence is empty</exception>
        public static IEnumerable<T[]> Window<T>(this IEnumerable<T> items, int windowSize)
        {
            Contract.RequiresNotNull(items);
            Contract.Requires(windowSize > 0);

            var e = items.GetEnumerator();

            // fill initial window
            var result = new T[windowSize];
            for (int i = 0; i < windowSize; i++)
            {
                if (!e.MoveNext())
                {
                    if (i == 0)
                        throw new ArgumentException("Sequence is empty");
                    Array.Resize(ref result, i);
                    yield return result;
                    yield break;
                }

                result[i] = e.Current;
            }

            // return first window
            yield return result;

            // subsequent windows
            while (e.MoveNext())
            {
                var prev = result;
                result = new T[windowSize];
                Array.Copy(prev, 1, result, 0, windowSize - 1);
                result[windowSize - 1] = e.Current;
                yield return result;
            }
        }

        public static T? TryFind<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct
        {
            foreach (var v in source)
            {
                if (predicate(v))
                    return v;
            }
            return null;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> source, T item, IEqualityComparer<T> comparer = null) where T : struct
        {
            var comp = comparer ?? EqualityComparer<T>.Default;
            int i = 0;
            foreach (var v in source)
            {
                if (comp.Equals(v, item))
                    return i;
                i++;
            }
            return -1;
        }

        public static void SetRelationship<T, TKey, TOther>(this IEnumerable<T> source, Func<T, TKey> keyExtractor, ILookup<TKey, TOther> details, Action<T, IEnumerable<TOther>> mutator)
        {
            foreach (var item in source)
            {
                var key = keyExtractor(item);
                mutator(item, details[key]);
            }
        }

        public static void SetRelationship<T, TKey, TOther>(this IEnumerable<T> source, Func<T, TKey> keyExtractor, HashLookup<TKey, TOther> details, Action<T, IReadOnlyList<TOther>> mutator)
        {
            foreach (var item in source)
            {
                var key = keyExtractor(item);
                mutator(item, details[key]);
            }
        }

        public static void SetRelationship<T, TKey, TOther, TOther2>(this IEnumerable<T> source, Func<T, TKey> keyExtractor, HashLookup<TKey, TOther> details, Action<T, IReadOnlyList<TOther>> mutator, HashLookup<TKey, TOther2> details2, Action<T, IReadOnlyList<TOther2>> mutator2)
        {
            foreach (var item in source)
            {
                var key = keyExtractor(item);
                mutator(item, details[key]);
                mutator2(item, details2[key]);
            }
        }

        // Expression version - needs code generation
        //public static void SetRelationship<T, TOther>(this IEnumerable<T> source, IEnumerable<TOther> other, Expression<Func<T, TOther, bool>> keyEquality)
        //{
        //    foreach (var item in source)
        //    {
        //    }
        //}

    }
}
