using BusterWood.Ducks;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BusterWood.Goodies
{
    /// <summary>Generic ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="TEntity">The type of the ID is for, e.g. Order</typeparam>
    /// <typeparam name="TKey">The type of ID, e.g. int</typeparam>
    /// <remarks>Does NOT support XML serialization</remarks>
    public struct Id<TEntity, TKey> : IEquatable<Id<TEntity, TKey>>, IXmlSerializable
        where TKey : IEquatable<TKey>
    {
        public Id(TKey value)
        {
            Value = value;
        }

        public TKey Value { get; private set; }
        public override string ToString() => $"{typeof(TEntity)} {Value}";
        public override bool Equals(object obj) => obj is Id<TEntity, TKey> && Equals((Id<TEntity, TKey>)obj);
        public bool Equals(Id<TEntity, TKey> other) => Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = Duck.Cast<IParser<TKey>>(typeof(TKey)).Parse(reader.ReadElementContentAsString()); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator TKey(Id<TEntity, TKey> id) => id.Value;
        public static explicit operator Id<TEntity, TKey>(TKey id) => new Id<TEntity, TKey>(id);
        public static bool operator ==(Id<TEntity, TKey> left, Id<TEntity, TKey> right) => left.Equals(right);
        public static bool operator !=(Id<TEntity, TKey> left, Id<TEntity, TKey> right) => !left.Equals(right);
    }

    public interface IParser<T>
    {
        T Parse(string text);
    }

    /// <summary>Generic Int32 ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="T">The type of the ID is for, e.g. Order</typeparam>
    public struct Int32<T> : IEquatable<Int32<T>>, IXmlSerializable
    {
        public Int32(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }
        public override string ToString() => $"{typeof(T)} {Value}";
        public override bool Equals(object obj) => obj is Int32<T> && Equals((Int32<T>)obj);
        public bool Equals(Int32<T> other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = reader.ReadElementContentAsInt(); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator int(Int32<T> id) => id.Value;
        public static explicit operator Int32<T>(int id) => new Int32<T>(id);
        public static bool operator ==(Int32<T> left, Int32<T> right) => left.Equals(right);
        public static bool operator !=(Int32<T> left, Int32<T> right) => !left.Equals(right);
    }

    /// <summary>Generic Int64 ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="T">The type of the ID is for, e.g. Order</typeparam>
    public struct Int64<T> : IEquatable<Int64<T>>, IXmlSerializable
    {
        public Int64(long value)
        {
            Value = value;
        }

        public long Value { get; private set; }
        public override string ToString() => $"{typeof(T)} {Value}";
        public override bool Equals(object obj) => obj is Int64<T> && Equals((Int64<T>)obj);
        public bool Equals(Int64<T> other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = reader.ReadElementContentAsLong(); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator long(Int64<T> id) => id.Value;
        public static explicit operator Int64<T>(long id) => new Int64<T>(id);
        public static bool operator ==(Int64<T> left, Int64<T> right) => left.Equals(right);
        public static bool operator !=(Int64<T> left, Int64<T> right) => !left.Equals(right);
    }

    /// <summary>Generic GUID ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="T">The type of the ID is for, e.g. Order</typeparam>
    public struct Guid<T> : IEquatable<Guid<T>>, IXmlSerializable
    {
        public Guid(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; private set; }
        public override string ToString() => $"{typeof(T)} {Value}";
        public override bool Equals(object obj) => obj is Guid<T> && Equals((Guid<T>)obj);
        public bool Equals(Guid<T> other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = Guid.Parse(reader.ReadElementContentAsString()); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator Guid(Guid<T> id) => id.Value;
        public static explicit operator Guid<T>(Guid id) => new Guid<T>(id);
        public static bool operator ==(Guid<T> left, Guid<T> right) => left.Equals(right);
        public static bool operator !=(Guid<T> left, Guid<T> right) => !left.Equals(right);
    }    
    
    /// <summary>Generic string ID type to prevent different types of IDs being mixed up, e.g. client ID and order ID</summary>
    /// <typeparam name="T">The type of the ID is for, e.g. Order</typeparam>
    public struct String<T> : IEquatable<String<T>>, IXmlSerializable
    {
        public String(String value)
        {
            Value = value;
        }

        public String Value { get; private set; }
        public override string ToString() => $"{typeof(T)} {Value}";
        public override bool Equals(object obj) => obj is String<T> && Equals((String<T>)obj);
        public bool Equals(String<T> other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = reader.ReadElementContentAsString(); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator String(String<T> id) => id.Value;
        public static explicit operator String<T>(String id) => new String<T>(id);
        public static bool operator ==(String<T> left, String<T> right) => left.Equals(right);
        public static bool operator !=(String<T> left, String<T> right) => !left.Equals(right);
    }    

    /// <summary>Generic decimal value type to prevent different types of things being mixed up, e.g. price and quantity</summary>
    /// <typeparam name="T">The type of the ID is for, e.g. Order</typeparam>
    public struct Decimal<T> : IEquatable<Decimal<T>>, IXmlSerializable, IFormattable
    {
        public Decimal(Decimal value)
        {
            Value = value;
        }

        public Decimal Value { get; private set; }
        public override string ToString() => $"{typeof(T)} {Value}";
        public string ToString(string format, IFormatProvider formatProvider) => $"{typeof(T)} {Value.ToString(format, formatProvider)}";
        public override bool Equals(object obj) => obj is Decimal<T> && Equals((Decimal<T>)obj);
        public bool Equals(Decimal<T> other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        XmlSchema IXmlSerializable.GetSchema() => null;
        void IXmlSerializable.ReadXml(XmlReader reader) { Value = reader.ReadElementContentAsDecimal(); }
        void IXmlSerializable.WriteXml(XmlWriter writer) { writer.WriteValue(Value); }
        public static explicit operator Decimal(Decimal<T> id) => id.Value;
        public static explicit operator Decimal<T>(Decimal id) => new Decimal<T>(id);
        public static bool operator ==(Decimal<T> left, Decimal<T> right) => left.Equals(right);
        public static bool operator !=(Decimal<T> left, Decimal<T> right) => !left.Equals(right);
    }

}