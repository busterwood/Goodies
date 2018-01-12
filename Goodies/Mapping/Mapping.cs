using System;
using System.Collections.Generic;
using static System.StringComparison;

namespace BusterWood.Mapping
{
    public static class Mapping
    {
        internal static readonly Subject<string> _trace = new Subject<string>();
        public static IObservable<string> Trace => _trace;

        /// <summary>
        /// Creates the mapping between <paramref name="source"/> and <paramref name="destination"/> using the SOURCE to generate candidate names for the mapping
        /// </summary>
        public static MappingResult<Thing, Thing> CreateFromSource(Type source, Type destination, string removablePrefix = null)
            => CreateFromSource(Types.ReadablePublicThings(source), Types.WriteablePublicThings(destination), removablePrefix);

        /// <summary>
        /// Creates the mapping between <paramref name="sourceMappings"/> and <paramref name="destination"/> using the SOURCE to generate candidate names for the mapping
        /// </summary>
        public static MappingResult<Thing, Thing> CreateFromSource(IReadOnlyCollection<Thing> sourceMappings, Type destination, string removablePrefix = null)
            => CreateFromSource(sourceMappings, Types.WriteablePublicThings(destination), removablePrefix);

        internal static MappingResult<TFrom, TTo> CreateFromSource<TFrom, TTo>(IReadOnlyCollection<TFrom> sources, IReadOnlyCollection<TTo> destinations, string removablePrefix = null)
            where TFrom : Thing
            where TTo: Thing
        {
            var comparisons = new List<Func<TFrom, TTo, bool>> {
                NameMatches,
                WithId,
                WithoutId,
            };
            if (!string.IsNullOrWhiteSpace(removablePrefix))
            {
                comparisons.Add((from, to) => WithoutPrefix(from, to, removablePrefix));
            }
            var result = GenericMapping.Create(sources, destinations, comparisons);
            ReportUnmapped(result.Unmapped.From, removablePrefix);
            return result;
        }

        static void ReportUnmapped<T>(List<T> unmapped, string removablePrefix)
            where T : Thing
        {
            var type = string.IsNullOrWhiteSpace(removablePrefix) ? string.Empty : removablePrefix + ": ";
            foreach (var u in unmapped)
            {
                _trace.OnNext($"{type}Cannot find a mapping for {u.Name} or type is not compatible");
            }
        }

        /// <summary>
        /// Creates the mapping between <paramref name="source"/> and <paramref name="destination"/> using the DESTINATION to generate candidate names for the mapping
        /// </summary>
        public static MappingResult<Thing, Thing> CreateFromDestination(Type source, Type destination, string removablePrefix = null)
            => CreateFromDestination(Types.ReadablePublicThings(source), Types.WriteablePublicThings(destination), removablePrefix);

        /// <summary>
        /// Creates the mapping between <paramref name="source"/> and <paramref name="destination"/> using the DESTINATION to generate candidate names for the mapping
        /// </summary>
        public static MappingResult<Thing, Thing> CreateFromDestination(Type source, IReadOnlyCollection<Thing> destination, string removablePrefix = null)
            => CreateFromDestination(Types.ReadablePublicThings(source), destination, removablePrefix);

        /// <summary>
        /// Creates the mapping between <paramref name="sourceMappings"/> and <paramref name="destination"/> using the DESTINATION to generate candidate names for the mapping
        /// </summary>
        public static MappingResult<Thing, Thing> CreateFromDestination(IReadOnlyCollection<Thing> sourceMappings, Type destination, string removablePrefix = null)
            => CreateFromDestination(sourceMappings, Types.WriteablePublicThings(destination), removablePrefix);

        internal static MappingResult<TFrom, TTo> CreateFromDestination<TFrom, TTo>(IReadOnlyCollection<TFrom> sources, IReadOnlyCollection<TTo> destinations, string removablePrefix = null)
            where TFrom : Thing
            where TTo : Thing
        {
            // note: from and to have been reversed
            var comparisons = new List<Func<TFrom, TTo, bool>> {
                (from, to) => NameMatches(to, from),
                (from, to) => WithId(to, from),
                (from, to) => WithoutId(to, from),
            };
            if (!string.IsNullOrWhiteSpace(removablePrefix))
            {
                comparisons.Add((from, to) => WithoutPrefix(to, from, removablePrefix));
            }
            var result = GenericMapping.Create(sources, destinations, comparisons);
            ReportUnmapped(result.Unmapped.To, removablePrefix);
            return result;
        }

        public static bool NameMatches<TFrom, TTo>(TFrom from, TTo to)
            where TFrom : Thing
            where TTo : Thing
        {
            return string.Equals(from.ComparisonName, to.ComparisonName, OrdinalIgnoreCase)
                && Types.AreInSomeSenseCompatible(from.Type, to.Type);
        }

        public static bool WithId<TFrom, TTo>(TFrom from, TTo to)
            where TFrom: Thing
            where TTo : Thing
        {
            if (from.ComparisonName.EndsWith("ID", OrdinalIgnoreCase))
                return false;

            return string.Equals(from.ComparisonName + "ID", to.ComparisonName, OrdinalIgnoreCase)
                && Types.AreInSomeSenseCompatible(from.Type, to.Type);
        }

        public static bool WithoutId<TFrom, TTo>(TFrom from, TTo to)
            where TFrom: Thing
            where TTo : Thing
        {
            if (!from.ComparisonName.EndsWith("ID", OrdinalIgnoreCase))
                return false;

            return string.Equals(from.ComparisonName.Substring(0, from.ComparisonName.Length - 2), to.ComparisonName, OrdinalIgnoreCase)
                && Types.AreInSomeSenseCompatible(from.Type, to.Type);
        }

        public static bool WithoutPrefix<TFrom, TTo>(TFrom from, TTo to, string removablePrefix)
            where TFrom: Thing
            where TTo : Thing
        {
            if (!from.ComparisonName.StartsWith(removablePrefix, OrdinalIgnoreCase))
                return false;

            return string.Equals(from.ComparisonName.Substring(removablePrefix.Length), to.ComparisonName, OrdinalIgnoreCase)
                && Types.AreInSomeSenseCompatible(from.Type, to.Type);
        }

    }
}
