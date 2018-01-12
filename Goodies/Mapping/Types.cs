using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BusterWood.Mapping
{
    public static class Types
    {
        internal static readonly Dictionary<Type, DbType> TypeToDbType;
        internal static readonly Dictionary<DbType, Type> DBTypeToType;

        static Types()
        {
            TypeToDbType = new Dictionary<Type, DbType>
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime2,  // note: explicit use to datetime2, rather than datetime
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = DbType.DateTime2,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset
            };
            DBTypeToType = new Dictionary<DbType, Type>
            {
                [DbType.AnsiString] = typeof(string),
                [DbType.AnsiStringFixedLength] = typeof(string),
                [DbType.DateTime] = typeof(DateTime),
            };
            foreach (var pair in TypeToDbType)
            {
                var type = pair.Key;
                if (type.IsGenericType) continue; // ignore nullables
                DBTypeToType.Add(pair.Value, type);
            }
        }

        public static Type TypeFromSqlTypeName(string sqlType)
        {
            switch (sqlType)
            {
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                    return typeof(string);
                case "numeric":
                    return typeof(decimal);
                case "bit":
                    return typeof(bool);
                case "tinyint":
                    return typeof(byte);
                case "smallint":
                    return typeof(short);
                case "int":
                    return typeof(int);
                case "bigint":
                    return typeof(long);
                case "datetime":
                case "datetime2":
                    return typeof(DateTime);
                case "datetimeoffset":
                    return typeof(DateTimeOffset);
                case "timestamp":
                    return typeof(byte[]);
                case "uniqueidentifier":
                    return typeof(Guid);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, "Unknown SQL type");
            }
        }

        [Pure]
        public static bool AreCompatible(Type inType, Type outType)
        {
            return inType == outType || CanBeCast(inType, outType) || CanBeCastViaExplicitOperator(inType, outType);
        }

        [Pure]
        public static bool CanBeCast(Type inType, Type outType)
        {
            return outType.IsAssignableFrom(inType)
                || (inType.IsPrimitiveOrEnum() && IsNullable(outType) && outType.GetGenericArguments()[0].IsPrimitiveOrEnum())
                   || (inType.IsPrimitive && outType.IsPrimitive)
                   || (outType.IsEnum && inType.IsPrimitive) // enum assignment is not handled in "IsAssignableFrom"
                   || (outType.IsEnum && inType.IsEnum) 
                   || (outType.IsPrimitive && inType.IsEnum);
        }

        public static bool CanBeCastViaExplicitOperator(Type inType, Type outType)
        {
            return GetExplicitCastOperator(inType, outType) != null;
        }

        public static MethodInfo GetExplicitCastOperator(Type inType, Type outType)
        {
            // try the output type first
            var method = outType.GetMethod("op_Explicit", BindingFlags.Static | BindingFlags.Public, null, new Type[] { inType }, null);
            if (method != null && method.ReturnType == outType)
                return method;

            // try the input type
            method = inType.GetMethod("op_Explicit", BindingFlags.Static | BindingFlags.Public, null, new Type[] { inType }, null);
            if (method != null && method.ReturnType == outType)
                return method;
            return null;
        }

        [Pure]
        public static bool AreInSomeSenseCompatible(Type inType, Type outType)
        {
            return AreCompatible(inType, outType) 
                || (IsNullable(inType) && AreCompatible(inType.GetGenericArguments()[0], outType))
                || (IsNullable(outType) && AreCompatible(inType, outType.GetGenericArguments()[0]));
        }

        [Pure]
        public static Type PropertyOrFieldType(this MemberInfo member)
        {
            Contract.Requires(member != null);
            var prop = member as PropertyInfo;
            if (prop != null) return prop.PropertyType;
            return ((FieldInfo) member).FieldType;
        }

        [Pure]
        public static bool CanBeNull(Type type)
        {
            if (type.IsPrimitive) return false;
            if (IsNullable(type)) return true;
            if (type.IsEnum) return false;
            if (!type.IsClass) return false;
            return true;
        }

        [Pure]
        public static bool IsNullable(this Type type)
        {
            Contract.Requires(type != null);
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        [Pure]
        public static bool IsNullableEnum(this Type type)
        {
            Contract.Requires(type != null);
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0].IsEnum;
        }

        [Pure]
        public static bool IsNullablePrimitiveOrEnum(this Type type)
        {
            Contract.Requires(type != null);
            if (IsPrimitiveOrEnum(type)) return true;
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsPrimitiveOrEnum(type.GetGenericArguments()[0]);
        }

        [Pure]
        public static bool IsPrimitiveOrEnum(this Type type)
        {
            Contract.Requires(type != null);
            return type.IsPrimitive || type.IsEnum;
        }


        [Pure]
        public static Type NullableOf(this Type type)
        {
            Contract.Requires(type != null);
            Contract.Requires(type.IsGenericType);
            Contract.Requires(type.GetGenericTypeDefinition() == typeof(Nullable<>));
            return type.GetGenericArguments()[0];
        }

        public static Dictionary<string, MemberInfo> WritablePropertiesAndFields<T>()
        {
            return WriteablePublicFieldsAndProperties(typeof(T)).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        }

        public static IReadOnlyCollection<Thing> WriteablePublicThings(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            var fields = type.GetFields(PublicInstance).Where(field => !field.IsInitOnly).Select(fi => (Thing)new Field(fi));
            var props = type.GetProperties(PublicInstance).Where(prop => prop.CanWrite).Select(pi => (Thing)new Property(pi));
            return new List<Thing>(fields.Concat(props));
        }

        public static IReadOnlyCollection<Thing> ReadablePublicThings(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            var fields = type.GetFields(PublicInstance).Select(fi => (Thing)new Field(fi));
            var props = type.GetProperties(PublicInstance).Where(prop => prop.CanRead).Select(pi => (Thing)new Property(pi));
            return new List<Thing>(fields.Concat(props));
        }

        internal static ConstructorInfo LongestConstructor(Type type)
        {
            return type.GetConstructors().OrderByDescending(ci => ci.GetParameters().Length).FirstOrDefault();
        }

        internal static IReadOnlyCollection<Thing> ConstructorThings(ConstructorInfo ctor)
        {
            return ctor.GetParameters().Select(pi => (Thing)new Parameter(pi)).ToList();
        }

        public static IEnumerable<MemberInfo> WriteablePublicFieldsAndProperties(Type type)
        {
            Contract.Requires(type != null);
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            return type.GetFields(PublicInstance).Where(field => !field.IsInitOnly).Cast<MemberInfo>()
                   .Concat(type.GetProperties(PublicInstance).Where(prop => prop.CanWrite));
        }

        public static Dictionary<string, MemberInfo> ReadablePropertiesAndFields<T>() => ReadablePropertiesAndFieldsDictionary(typeof(T));

        public static Dictionary<string, MemberInfo> ReadablePropertiesAndFieldsDictionary(Type typeT)
        {
            return ReadablePublicFieldsAndProperties(typeT).ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<MemberInfo> ReadablePublicFieldsAndProperties(Type type)
        {
            Contract.Requires(type != null);
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
            return type.GetFields(PublicInstance).Cast<MemberInfo>()
                   .Concat(type.GetProperties(PublicInstance).Where(prop => prop.CanRead));
        }

    }

    [Flags]
    enum TypeFlags
    {
        Primative,
        Enum,
        NullablePrimative,
        NullableEnum,
    }
}