using BusterWood.Collections;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Data
{
    public interface IRelation : IEnumerable<Row>
    {
        IReadOnlyUniqueList<Column> Columns { get; }
        int RowCount { get; }
        Row this[int column] { get; }
        //IList ColumnData(int column);
        //ColumnData<T> ColumnData<T>(int column);
        T GetData<T>(int row, int column);
        void SetData<T>(int row, int column, T value);
        //void SetData(int row, int column, object value);
    }

}