using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace BusterWood.Mapping
{
    public static partial class Extensions
    {
        /// <summary>Create an output object using the parameter-less constructor and setting public fields and properties</summary>
        public static T Copy<T>(this T input) => Copy<T, T>(input);

        /// <summary>Makes a shallow copy of an object, changing the copy via the <see cref="Action{T}"/></summary>
        /// <remarks>Create an output object using the parameter-less constructor and setting public fields and properties</remarks>
        public static T CopyWith<T>(this T input, Action<T> with)
        {
            Contract.Requires(with != null);
            var copy = Copy<T, T>(input);
            with(copy);
            return copy;
        }

        /// <summary>Create shallow copies of the <paramref name="input"/> objects using the parameter-less constructor and setting public fields and properties</summary>
        public static IEnumerable<T> CopyAll<T>(this IEnumerable<T> input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
            return CopyAll<T, T>(input);
        }

        /// <summary>Create shallow copies of the <paramref name="input"/> objects using the parameter-less constructor and setting public fields and properties</summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each cloned objects</remarks>
        public static IEnumerable<T> CopyAll<T>(this IEnumerable<T> input, Action<T, T> extraAction)
        {
            Contract.Requires(input != null);
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
            return CopyAll<T, T>(input, extraAction);
        }

        /// <summary>Create shallow copies of the <paramref name="input"/> objects using the parameter-less constructor and setting public fields and properties</summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each cloned objects</remarks>
        public static IEnumerable<T> CopyAll<T>(this IEnumerable<T> input, Action<T, T, int> extraAction)
        {
            Contract.Requires(input != null);
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
            return CopyAll<T, T>(input, extraAction);
        }

        /// <summary>Create an output object and copies all properties and fields where the property name and types match</summary>
        public static TOut Copy<TIn, TOut>(this TIn input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<TOut>() != null);
            var copy = ObjectMapper.GetOrAdd<TIn, TOut>();
            return copy(input, default(TOut));
        }

        /// <summary>Copies values from the <paramref name="input"/> to <paramref name="existing"/></summary>
        /// <returns><paramref name="existing"/> or a new object if <paramref name="existing"/> was null</returns>
        public static TOut CopyTo<TIn, TOut>(this TIn input, TOut existing)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<TOut>() != null);
            var copy = ObjectMapper.GetOrAdd<TIn, TOut>();
            return copy(input, existing);
        }

        /// <summary>creates copies of all input objects, copying all properties and fields with matching names and compatible types</summary>
        public static IEnumerable<TOut> CopyAll<TIn, TOut>(this IEnumerable<TIn> input) {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<IEnumerable<TOut>>() != null);
            var copy = ObjectMapper.GetOrAdd<TIn, TOut>();
            return input.Select(i => copy(i, default(TOut)));
        }

        /// <summary>creates copies of all input objects, copying all properties and fields with matching names and compatible types</summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each mapped objects</remarks>
        public static IEnumerable<TOut> CopyAll<TIn, TOut>(this IEnumerable<TIn> input, Action<TIn, TOut> extraAction) {
            Contract.Requires(input != null);
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<TOut>>() != null);

            var copy = ObjectMapper.GetOrAdd<TIn, TOut>();
            foreach (var item in input)
            {
                TOut mapped = copy(item, default(TOut));
                extraAction(item, mapped);
                yield return mapped;
            }
        }

        /// <summary>creates copies of all input objects, copying all properties and fields with matching names and compatible types</summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each mapped objects, passing the index (sequence number) of the item being mapped</remarks>
        public static IEnumerable<TOut> CopyAll<TIn, TOut>(this IEnumerable<TIn> input, Action<TIn, TOut, int> extraAction)
        {
            Contract.Requires(input != null);
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<TOut>>() != null);

            var copy = ObjectMapper.GetOrAdd<TIn, TOut>();
            int i = 0;
            foreach (var item in input)
            {
                TOut mapped = copy(item, default(TOut));
                extraAction(item, mapped, i);
                yield return mapped;
                i++;
            }
        }

    }
}