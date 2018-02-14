using BusterWood.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Data
{
    public class Relation : IRelation, IEnumerable<Row>
    {
        readonly UniqueList<Column> _columns = new UniqueList<Column>(new Column.NameEquality());
        readonly List<IExpandable> _data = new List<IExpandable>();
        int rowCount;

        public int RowCount => rowCount;

        public IReadOnlyUniqueList<Column> Columns => _columns;

        public int AddColumn<T>(string name)
        {
            var col = new Column(name, typeof(T));
            if (_columns.Add(col))
            {
                _data.Add(new ExpandableList<T>(rowCount));
            }
            return _columns.IndexOf(col);
        }

        public Row this[int index] => new Row(this, index);

        public Row AddRow()
        {
            for (int i = 0; i < _data.Count; i++)
            {
                _data[i].Expand();
            }
            return new Row(this, rowCount++);
        }

        public Row AddRow(params object[] values)
        {
            for (int i = 0; i < _data.Count; i++)
            {
                _data[i].Add(values[i]);
            }
            return new Row(this, rowCount++);
        }

        public IReadOnlyList<T> ColumnData<T>(int index) => (IReadOnlyList<T>)_data[index];

        public IList ColumnData(int index) => _data[index]; // TODO: a readonly wrapper?

        public IEnumerator<Row> GetEnumerator()
        {
            for (int i = 0; i < rowCount; i++)
            {
                yield return new Row(this, i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public T GetData<T>(int row, int column) => ((List<T>)_data[column])[row];

        public void SetData<T>(int row, int column, T value) => ((List<T>)_data[column])[row] = value;

        interface IExpandable : IList
        {
            void Expand();
        }

        class ExpandableList<T> : List<T>, IExpandable
        {
            public ExpandableList()
            {
            }

            public ExpandableList(int capacity) : base(capacity)
            {
                var @default = default(T);
                for (int i = 0; i < capacity; i++)
                {
                    Add(@default); //TODO: this looks odd, really we just need to set the Count
                }
            }

            public void Expand()
            {
                Add(default(T));
            }
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
        public static IReadOnlyList<T> ColumnData<T>(this IRelation relation, string columnName)
        {
            int column = relation.Columns.IndexOf(new Column(columnName, null));
            return column >= 0 ? relation.ColumnData<T>(column) : null;
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

        public static T Aggregate<T>(this IRelation relation, int column, Func<T, T, T> aggreation, T initial = default(T))
        {
            var result = initial;
            foreach (T val in relation.ColumnData<T>(column))
            {
                result = aggreation(initial, val);
            }
            return result;
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
