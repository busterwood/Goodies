using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BusterWood.Mapping
{
    public static class ObjectEnumerator
    {       
        /// <summary>Turns an object into a sequence of key value pairs</summary>
        /// <remarks>Uses reflection, TODO: a version that uses LINQ expressions</remarks>
        public static IEnumerable<KeyValuePair<string, object>> AsSeq(this object item)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

            if (item == null)
                return Enumerable.Empty<KeyValuePair<string, object>>();

            var type = item.GetType();

            var fields = type.GetFields(PublicInstance)
                .Select(f => new KeyValuePair<string, object>(f.Name, f.GetValue(item)));

            var properties = type.GetProperties(PublicInstance)
                .Where(p => p.CanRead)
                .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(item)));
            
            return fields.Concat(properties);
        }

        /// <summary>Turns a sequence of key value pairs into an object using the ctor or writeable fields and properties (if the ctor has zero parameters)</summary>
        /// <remarks>Uses reflection, TODO: a version that uses LINQ expressions</remarks>
        public static T New<T>(this IEnumerable<KeyValuePair<string, object>> values)
        {
            if (values == null)
                return default(T);

            var longestCtor = typeof(T).GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (longestCtor?.GetParameters().Length > 0)
                return NewCtor<T>(longestCtor, values);
            else
                return NewSetProperties<T>(values);
        }

        private static T NewSetProperties<T>(IEnumerable<KeyValuePair<string, object>> values)
        {
            var writeable = Types.WriteablePublicFieldsAndProperties(typeof(T)).ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase); // case insensitive
            object item = Activator.CreateInstance(typeof(T)); // force boxing so setting values via reflection works for structs
            foreach (var pair in values)
            {
                MemberInfo member;
                if (writeable.TryGetValue(pair.Key, out member))
                {
                    member.SetValue(item, pair.Value);
                }
            }
            return (T)item; // unbox struct
        }

        private static void SetValue(this MemberInfo member, object obj, object value)
        {
            var targetType = Types.PropertyOrFieldType(member);
            value = ConvertValue(value, targetType);
            if (member is PropertyInfo)
                ((PropertyInfo)member).SetValue(obj, value);
            else
                ((FieldInfo)member).SetValue(obj, value);
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (Types.IsNullable(targetType) && value != null && value.GetType() != targetType)
            {
                value = Convert.ChangeType(value, targetType.GenericTypeArguments[0]);
            }
            return value;
        }

        private static T NewCtor<T>(ConstructorInfo ctor, IEnumerable<KeyValuePair<string, object>> values)
        {
            var map = values.ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase); // case insensitive

            var args = ctor.GetParameters()
                .Select(p => map.ContainsKey(p.Name) ? ConvertValue(map[p.Name], p.ParameterType) : Default(p.ParameterType))
                .ToArray();

            return (T)ctor.Invoke(args);
        }

        private static object Default(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
