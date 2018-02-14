using BusterWood.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Data
{
    public class Relation : IRelation, IEnumerable<Row>
    {
        readonly UniqueList<Column> _columns = new UniqueList<Column>(new Column.NameEquality());
        readonly List<Array> _data = new List<Array>();
        int capacity;
        int rowCount;

        public int RowCount => rowCount;

        public IReadOnlyUniqueList<Column> Columns => _columns;

        public int AddColumn<T>(string name)
        {
            var col = new Column(name, typeof(T));
            if (_columns.Add(col))
            {
                _data.Add(new T[rowCount]);
            }
            return _columns.IndexOf(col);
        }

        public Row this[int row] => new Row(this, row);

        public Row AddRow()
        {
            if (rowCount == capacity)
            {
                // expand
                capacity = capacity == 0 ? 4 : capacity + capacity;
                for (int i = 0; i < _data.Count; i++)
                {
                    var temp = Array.CreateInstance(_columns[i].Type, capacity);
                    Array.Copy(_data[i], temp, rowCount);
                    _data[i] = temp;
                }
            }
            return new Row(this, rowCount++);
        }

        public ColumnData<T> ColumnData<T>(int column) => new ColumnData<T>((T[])_data[column], rowCount);

        public IList ColumnData(int column) => _data[column]; // TODO: a readonly wrapper?

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
            var arr = (T[])_data[column];
            return arr[row];
        }

        public void SetData<T>(int row, int column, T value)
        {
            var arr = (T[])_data[column];
            arr[row] = value;
        }

        public void SetData(int row, int column, object value)
        {
            var arr = _data[column];
            arr.SetValue(value, row);
        }

        interface IExpandable : IList
        {
            void Expand();
        }
    }

    public struct Column 
    {
        public string Name { get; }
        public Type Type { get; }

        public Column(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public class NameEquality : IEqualityComparer<Column>
        {
            public bool Equals(Column x, Column y) => string.Equals(x.Name, y.Name);

            public int GetHashCode(Column c) => c.Name?.GetHashCode() ?? 0;
        }
    }

    public struct ColumnData<T> : IReadOnlyList<T>
    {
        readonly T[] _data;

        public int Count { get; }

        public ColumnData(T[] data, int count) : this()
        {
            _data = data;
            Count = count;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException();
                return _data[index];
            }
            set
            {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException();
                _data[index] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _data[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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

        public object Get(int column) => Relation.ColumnData(column)[RowIndex];
        public T Get<T>(int column) => Relation.GetData<T>(RowIndex, column);
        public void Set<T>(int column, T value) => Relation.SetData(RowIndex, column, value);
    }

    public static partial class Extensions
    {
        public static ColumnData<T> ColumnData<T>(this IRelation relation, string columnName)
        {
            int column = relation.Columns.IndexOf(new Column(columnName, null));
            return column >= 0 ? relation.ColumnData<T>(column) : new ColumnData<T>();
        }

        public static IList ColumnData(this IRelation relation, string columnName)
        {
            int column = relation.Columns.IndexOf(new Column(columnName, null));
            return column >= 0 ? relation.ColumnData(column) : null;
        }

        public static T Get<T>(this Row row, string columnName)
        {
            var column = row.Relation.Columns.IndexOf(new Column(columnName, null));
            return row.Get<T>(column);
        }

        public static object Get(this Row row, string columnName)
        {
            var column = row.Relation.Columns.IndexOf(new Column(columnName, null));
            return row.Get(column);
        }

        public static Row AddRow(this IRelation relation, params object[] values)
        {
            var row = relation.AddRow();
            int colCount = relation.Columns.Count;
            for (int i = 0; i < values.Length; i++)
            {
                if (i >= colCount)
                    break;
                relation.SetData(row.RowIndex, i, values[i]);
            }
            return row;
        }

        public static IEnumerable<Row> Where<T>(this IRelation relation, int column, Func<T, bool> predicate)
        {
            int i = 0;
            foreach (T val in relation.ColumnData<T>(column))
            {
                if (predicate(val))
                    yield return relation[i];
                i++;
            }
        }
    }
}
