using BusterWood.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BusterWood.Data
{
    public class Relation : IRelation
    {
        readonly UniqueList<Column> _columns = new UniqueList<Column>(new Column.NameEquality());
        readonly List<IExpandable> _data = new List<IExpandable>();
        int rowCount;

        public int RowCount => rowCount;

        public IReadOnlyList<Column> Columns => _columns;

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

        public IReadOnlyList<T> GetData<T>(string columnName)
        {
            int index = _columns.IndexOf(new Column(columnName, null));
            return index >= 0 ? (IReadOnlyList<T>)_data[index] : null;
        }

        internal List<T> GetDataInternal<T>(string columnName)
        {
            int index = _columns.IndexOf(new Column(columnName, null));
            return index >= 0 ? (List<T>)_data[index] : null;
        }

        public IReadOnlyList<T> GetData<T>(int index) => (IReadOnlyList<T>)_data[index];

        internal List<T> GetDataInternal<T>(int index) => (List<T>)_data[index];

        public IList GetData(int index) => _data[index]; // TODO: a readonly wrapper?

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
        readonly Relation _relation;

        public int RowIndex { get; }

        public Row(Relation relation, int index)
        {
            _relation = relation;
            RowIndex = index;
        }

        public object Get(int ordinal) => _relation.GetData(ordinal)[RowIndex];

        public T Get<T>(int ordinal) => _relation.GetData<T>(ordinal)[RowIndex];

        public T Get<T>(string columnName) => _relation.GetData<T>(columnName)[RowIndex];

        public void Set<T>(int ordinal, T value) => _relation.GetDataInternal<T>(ordinal)[RowIndex] = value;

        public void Set<T>(string columnName, T value) => _relation.GetDataInternal<T>(columnName)[RowIndex] = value;
    }

}
