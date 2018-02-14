using BusterWood.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Data
{
    public class Relation : IRelation, IEnumerable<Row>, IRowCounter
    {
        readonly UniqueList<Column> _columns = new UniqueList<Column>(new Column.NameEquality());
        int capacity;
        int rowCount;

        public int RowCount => rowCount;

        public IReadOnlyUniqueList<Column> Columns => _columns;

        public int AddColumn<T>(string name)
        {
            var col = new Column<T>(name, typeof(T), this);
            if (_columns.Add(col))
            {
                col.EnsureCapacity(RowCount);
            }
            return _columns.IndexOf(col);
        }

        public Row this[int row] => new Row(this, row);

        public Row AddRow()
        {
            if (rowCount == capacity)
            {
                // expand
                capacity = capacity == 0 ? 4 : capacity * 2;

                for (int i = 0; i < _columns.Count; i++)
                {
                    _columns[i].EnsureCapacity(capacity);
                }
            }
            return new Row(this, rowCount++);
        }

        public IEnumerator<Row> GetEnumerator()
        {
            for (int i = 0; i < rowCount; i++)
            {
                yield return new Row(this, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T GetData<T>(int row, int column)
        {
            var col = (Column<T>)_columns[column];
            return col[row];
        }

        public void SetData<T>(int row, int column, T value)
        {
            var col = (Column<T>)_columns[column];
            col[row] = value;
        }

        //public void SetData(int row, int column, object value)
        //{
        //    var col = _columns[column];
        //    col.SetValue(value, row);
        //}
    }

    public abstract class Column 
    {
        IRowCounter _rowCounter;

        public string Name { get; }

        public Type Type { get; }

        public int Count => _rowCounter.RowCount;

        public object this[int index] => GetBoxedValue(index);

        internal abstract object GetBoxedValue(int index);

        internal abstract void EnsureCapacity(int capacity);

        public Column(string name, Type type, IRowCounter rowCounter)
        {
            Name = name;
            Type = type;
            _rowCounter = rowCounter;
        }

        public class NameEquality : IEqualityComparer<Column>
        {
            public bool Equals(Column x, Column y) => string.Equals(x.Name, y.Name);

            public int GetHashCode(Column c) => c.Name?.GetHashCode() ?? 0;
        }
    }

    public interface IRowCounter
    {
        int RowCount { get; }
    }

    public sealed class Column<T> : Column, IReadOnlyList<T>
    {
        internal T[] _data;

        public Column(string name, Type type, IRowCounter rowCounter) : base(name, type, rowCounter)
        {
            _data = new T[0];
        }

        public new T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();
                return _data[index];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException();
                _data[index] = value;
            }
        }

        internal override object GetBoxedValue(int index) => this[index];

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return _data[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal override void EnsureCapacity(int capacity)
        {
            if (capacity == 0 || capacity < _data.Length)
                return;

            int newSize = _data.Length == 0 ? 4 : _data.Length * 2;
            while (capacity < newSize)
                newSize *= 2;

            Array.Resize(ref _data, newSize);
        }
    }



    public struct Row // DynamicObject? this would involve boxing?
    {
        internal readonly Relation Relation;

        public int RowIndex { get; }

        public Row(Relation relation, int index)
        {
            Relation = relation;
            RowIndex = index;
        }

        //public object Get(int column) => Relation.ColumnData(column)[RowIndex];
        public T Get<T>(int column) => Relation.GetData<T>(RowIndex, column);
        public void Set<T>(int column, T value) => Relation.SetData(RowIndex, column, value);
    }

    public static partial class Extensions
    {
        //public static ColumnData<T> ColumnData<T>(this IRelation relation, string columnName)
        //{
        //    int column = relation.Columns.IndexOf(new Column(columnName, null));
        //    return column >= 0 ? relation.ColumnData<T>(column) : new ColumnData<T>();
        //}

        //public static IList ColumnData(this IRelation relation, string columnName)
        //{
        //    int column = relation.Columns.IndexOf(new Column(columnName, null));
        //    return column >= 0 ? relation.ColumnData(column) : null;
        //}

        //public static T Get<T>(this Row row, string columnName)
        //{
        //    var column = row.Relation.Columns.IndexOf(new Column(columnName, null));
        //    return row.Get<T>(column);
        //}

        //public static object Get(this Row row, string columnName)
        //{
        //    var column = row.Relation.Columns.IndexOf(new Column(columnName, null));
        //    return row.Get(column);
        //}

        //public static Row AddRow(this IRelation relation, params object[] values)
        //{
        //    var row = relation.AddRow();
        //    int colCount = relation.Columns.Count;
        //    for (int i = 0; i < values.Length; i++)
        //    {
        //        if (i >= colCount)
        //            break;
        //        relation.SetData(row.RowIndex, i, values[i]);
        //    }
        //    return row;
        //}

        //public static IEnumerable<Row> Where<T>(this IRelation relation, int column, Func<T, bool> predicate)
        //{
        //    int i = 0;
        //    foreach (T val in relation.ColumnData<T>(column))
        //    {
        //        if (predicate(val))
        //            yield return relation[i];
        //        i++;
        //    }
        //}
    }
}
