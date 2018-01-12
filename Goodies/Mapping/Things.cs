using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace BusterWood.Mapping
{
    /// <summary>The thing being mapped, i.e. a <see cref="Field"/> or <see cref="Property"/> or <see cref="Parameter"/> or <see cref="Column"/></summary>
    /// <remarks>Not a great name but I can't think of a better one!</remarks>
    public interface Thing
    {
        /// <summary>name with underscores removed</summary>
        string ComparisonName { get; }
        string Name { get; }
        Type Type { get; }
    }

    public struct Field : Thing
    {
        public FieldInfo Wrapped { get; }
        public string ComparisonName => Wrapped.Name.Replace("_", "");
        public string Name => Wrapped.Name;
        public Type Type => Wrapped.FieldType;

        public Field(FieldInfo field)
        {
            Contract.Requires(field != null);
            Wrapped = field;
        }
    }

    public struct Property : Thing
    {
        public PropertyInfo Wrapped { get; }
        public string ComparisonName => Wrapped.Name.Replace("_", "");
        public string Name => Wrapped.Name;
        public Type Type => Wrapped.PropertyType;

        public Property(PropertyInfo prop)
        {
            Contract.Requires(prop != null);
            Wrapped = prop;
        }
    }

    public struct Parameter : Thing
    {
        public ParameterInfo Wrapped { get; }
        public string ComparisonName => Wrapped.Name.Replace("_", "");
        public string Name => Wrapped.Name;
        public Type Type => Wrapped.ParameterType;

        public Parameter(ParameterInfo parameter)
        {
            Contract.Requires(parameter != null);
            Wrapped = parameter;
        }
    }

    /// <remarks>
    /// This needs to override equality as it is used by <see cref="DataReaderMapper.MetaData.Equals(DataReaderMapper.MetaData)"/>
    /// </remarks>
    public struct Column : Thing, IEquatable<Column>
    {
        readonly string name;
        readonly Type type;

        public string ComparisonName => name.Replace("_", "");
        public string Name => name;
        public Type Type => type;
        public int Ordinal { get; }
        public string DataTypeName { get; }

        public Column(int ordinal, string name, Type type, string dataTypeName = "")
        {
            Ordinal = ordinal;
            this.type = type;
            this.name = name;
            DataTypeName = dataTypeName;
        }

        public override bool Equals(object obj) => obj is Column && Equals((Column)obj);

        public bool Equals(Column other)
        {
            return string.Equals(name, other.name, StringComparison.OrdinalIgnoreCase) && Equals(type, other.type);
        }

        public override int GetHashCode()
        {
            unchecked { return name.GetHashCode() * type.GetHashCode(); }
        }
    }

    public struct ObjectThing : Thing
    {
        public string ComparisonName => Name.Replace("_", "");
        public string Name { get; }
        public Type Type => typeof(object);

        public ObjectThing(string name)
        {
            Contract.Requires(name != null);
            Name = name;
        }
    }
}
