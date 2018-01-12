using System;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Mapping
{
    public static class GenericMapping
    {
        /// <summary>
        /// Tries to map from a <paramref name="source"/> to <paramref name="destination"/> using some <paramref name="comparisons"/>
        /// </summary>
        /// <typeparam name="TFrom">The source type to map from</typeparam>
        /// <typeparam name="TTo">The destination type to map to</typeparam>
        /// <param name="source">The items to map from</param>
        /// <param name="destination">The items to map to</param>
        /// <param name="comparisons">The method of mapping, in priority order</param>
        /// <returns>A result containing some mappings, and any remaining items that could not be mapped</returns>
        public static MappingResult<TFrom, TTo> Create<TFrom, TTo>(IEnumerable<TFrom> source, IEnumerable<TTo> destination, IEnumerable<Func<TFrom, TTo, bool>> comparisons)
        {
            var from = source.ToList();
            var to = destination.ToList();
            var mapped = new List<Mapping<TFrom, TTo>>(from.Count);
            var fromRemaining = new List<TFrom>(from.Count);

            foreach (var comp in comparisons)
            {
                foreach (var f in from)
                {
                    int index = IndexOf(f, to, comp);
                    if (MatchFound(index))
                    {
                        mapped.Add(new Mapping<TFrom, TTo>(f, to[index]));
                        to.RemoveAt(index);
                    }
                    else
                    {
                        fromRemaining.Add(f);
                    }
                }
                from.Clear();
                from.AddRange(fromRemaining);
                fromRemaining.Clear();
            }

            return new MappingResult<TFrom, TTo>(mapped, from, to);
        }

        static int IndexOf<TFrom, TTo>(TFrom from, IEnumerable<TTo> to, Func<TFrom, TTo, bool> comp)
        {
            int i = 0;
            foreach (var t in to)
            {
                if (comp(from, t))
                    return i;
                i++;
            }
            return -1;
        }

        static bool MatchFound(int matchIndex) => matchIndex >= 0;
    }

    /// <summary>
    /// The result of <see cref="GenericMapping.Create{TFrom, TTo}(IEnumerable{TFrom}, IEnumerable{TTo}, IEnumerable{Func{TFrom, TTo, bool}})"/>
    /// </summary>
    public class MappingResult<TFrom, TTo>
    {
        public List<Mapping<TFrom, TTo>> Mapped { get; }
        public Mapping<List<TFrom>, List<TTo>> Unmapped { get; }

        public MappingResult(List<Mapping<TFrom, TTo>> mapped, List<TFrom> unmappedFrom, List<TTo> unmappedTo)
        {
            Mapped = mapped;
            Unmapped = new Mapping<List<TFrom>, List<TTo>>(unmappedFrom, unmappedTo);
        }
    }

    /// <summary>
    /// A single item that has been mapped
    /// </summary>
    public struct Mapping<TFrom, TTo>
    {
        public readonly TFrom From;
        public readonly TTo To;

        public Mapping(TFrom @from, TTo to)
        {
            From = @from;
            To = to;
        }
    }
}

