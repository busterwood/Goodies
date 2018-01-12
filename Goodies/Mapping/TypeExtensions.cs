using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace BusterWood.Mapping
{
    public static class TypeExtensions
    {
        [Pure]
        public static bool IsImmutable(this Type type)
        {
            Contract.Requires(type != null);
            return IsImmutable(type, new HashSet<Type>());
        }

        private static bool IsImmutable(this Type type, HashSet<Type> alreadyChecked)
        {
            Contract.Requires(type != null);
            Contract.Requires(alreadyChecked != null);

            if (type.IsPrimitiveOrEnum() || alreadyChecked.Contains(type))
                return true;
            alreadyChecked.Add(type);

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
            return fields.All(f => f.IsInitOnly && f.FieldType.IsImmutable(alreadyChecked)) && type.BaseType?.IsImmutable(alreadyChecked) != false;
        }
    }
}