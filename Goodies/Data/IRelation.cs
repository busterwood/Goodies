using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Data
{
    public interface IRelation
    {
        Row this[int index] { get; }

        IReadOnlyList<Column> Columns { get; }
        int RowCount { get; }

        IList GetData(int index);
        IReadOnlyList<T> GetData<T>(int index);
        IReadOnlyList<T> GetData<T>(string columnName);
    }

}