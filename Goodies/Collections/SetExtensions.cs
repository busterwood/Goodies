using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Collections
{
    public static class SetExtensions
    {
        public static bool IsProperSubsetOf<S, T>(this S set, IEnumerable<T> other) where S : IReadOnlySet<T> => set.IsSubsetOf(other) && other.Any(x => !set.Contains(x));

        public static bool IsProperSupersetOf<S, T>(this S set, IEnumerable<T> other) where S : IReadOnlySet<T> => set.IsSupersetOf(other) && set.Any(x => !other.Contains(x, set.Equality));

        public static bool IsSubsetOf<S, T>(this S set, IEnumerable<T> other) where S : IReadOnlySet<T> => set.All(x => other.Contains(x, set.Equality));

        public static bool IsSupersetOf<S, T>(this S set, IEnumerable<T> other) where S : IReadOnlySet<T> => other.All(x => set.Contains(x));

        public static bool Overlaps<S, T>(this S set, IEnumerable<T> other) where S : IReadOnlySet<T> => other.Any(x => set.Contains(x));

        public static bool SetEquals<S, T>(this S set, IEnumerable<T> other) where S : IReadOnlySet<T> => set.IsSubsetOf(other) && set.IsSupersetOf(other);

        /// <summary>Returns a <see cref="UniqueList{T}"/> of values from <paramref name="items"/>.</summary>
        public static UniqueList<T> ToUniqueList<T>(this IEnumerable<T> items, IEqualityComparer<T> equality = null)
        {
            equality  = equality ?? EqualityComparer<T>.Default;
            var ul = items as UniqueList<T>;
            if (ul != null && ul.Equality == equality)
                return ul.Copy();
            
            var result = new UniqueList<T>(equality);
            foreach (var item in items)
                result.Add(item);
            return result;
        }

        /// <summary>Adds <paramref name="item"/> to the set.  Returns FALSE if <paramref name="item"/> is null</summary>
        public static bool Add<T>(this ISet<T> set, T? item) where T : struct => item.HasValue ? set.Add(item.Value) : false;

        /// <summary>Adds <paramref name="item"/> to the set.  Returns FALSE if <paramref name="item"/> is null</summary>
        public static bool Union<T>(this ISet<T> set, T? item) where T : struct => set.Add(item);

        /// <summary>Adds <paramref name="item"/> to the set.  Returns FALSE if <paramref name="item"/> is null</summary>
        public static bool Union<T>(this ISet<T> set, T item) => item != null ? set.Add(item) : false;
    }
}