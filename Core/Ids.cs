using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BusterWood
{
    /// <summary>Generic ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="TEntity">The type of the ID is for, e.g. Order</typeparam>
    /// <typeparam name="TKey">The type of ID, e.g. int</typeparam>
    /// <remarks>Does NOT support XML serialization</remarks>
    public struct Id<TEntity, TKey> : IEquatable<Id<TEntity, TKey>>
        where TKey : IEquatable<TKey>
    {
        public Id(TKey value)
        {
            Value = value;
        }

        public TKey Value { get; }
        public override string ToString() => $"{typeof(TEntity)} {Value}";
        public override bool Equals(object obj) => obj is Id<TEntity, TKey> && Equals((Id<TEntity, TKey>)obj);
        public bool Equals(Id<TEntity, TKey> other) => Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
        public static explicit operator TKey(Id<TEntity, TKey> id) => id.Value;
        public static explicit operator Id<TEntity, TKey>(TKey id) => new Id<TEntity, TKey>(id);
        public static bool operator ==(Id<TEntity, TKey> left, Id<TEntity, TKey> right) => left.Equals(right);
        public static bool operator !=(Id<TEntity, TKey> left, Id<TEntity, TKey> right) => !left.Equals(right);
    }

    /// <summary>Generic Int32 ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="T">The type of the ID is for, e.g. Order</typeparam>
    public struct IntId<T> : IEquatable<IntId<T>>, IXmlSerializable
    {
        public IntId(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }
        public override string ToString() => $"{typeof(T)} {Value}";
        public override bool Equals(object obj) => obj is IntId<T> && Equals((IntId<T>)obj);
        public bool Equals(IntId<T> other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = reader.ReadElementContentAsInt(); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator int(IntId<T> id) => id.Value;
        public static explicit operator IntId<T>(int id) => new IntId<T>(id);
        public static bool operator ==(IntId<T> left, IntId<T> right) => left.Equals(right);
        public static bool operator !=(IntId<T> left, IntId<T> right) => !left.Equals(right);
    }

    /// <summary>Generic Int64 ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="T">The type of the ID is for, e.g. Order</typeparam>
    public struct LongId<T> : IEquatable<LongId<T>>, IXmlSerializable
    {
        public LongId(long value)
        {
            Value = value;
        }

        public long Value { get; private set; }
        public override string ToString() => $"{typeof(T)} {Value}";
        public override bool Equals(object obj) => obj is LongId<T> && Equals((LongId<T>)obj);
        public bool Equals(LongId<T> other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = reader.ReadElementContentAsLong(); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator long(LongId<T> id) => id.Value;
        public static explicit operator LongId<T>(long id) => new LongId<T>(id);
        public static bool operator ==(LongId<T> left, LongId<T> right) => left.Equals(right);
        public static bool operator !=(LongId<T> left, LongId<T> right) => !left.Equals(right);
    }

    /// <summary>Generic GUID ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="T">The type of the ID is for, e.g. Order</typeparam>
    public struct GuidId<T> : IEquatable<GuidId<T>>, IXmlSerializable
    {
        public GuidId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; private set; }
        public override string ToString() => $"{typeof(T)} {Value}";
        public override bool Equals(object obj) => obj is GuidId<T> && Equals((GuidId<T>)obj);
        public bool Equals(GuidId<T> other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = Guid.Parse(reader.ReadElementContentAsString()); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator Guid(GuidId<T> id) => id.Value;
        public static explicit operator GuidId<T>(Guid id) => new GuidId<T>(id);
        public static bool operator ==(GuidId<T> left, GuidId<T> right) => left.Equals(right);
        public static bool operator !=(GuidId<T> left, GuidId<T> right) => !left.Equals(right);
    }

}