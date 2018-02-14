using System.Collections.Generic;

namespace BusterWood.Collections
{
    public interface IReadOnlyUniqueList<T> : IReadOnlyList<T>, IReadOnlySet<T>
    {
        int IndexOf(T item);
    }

    public interface IUniqueList<T> : IReadOnlyUniqueList<T>, ISet<T>, IList<T>
    {
        void AddOrUpdate(T item);
    }
}