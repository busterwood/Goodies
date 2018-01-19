using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BusterWood.Reflection.Emit;

namespace BusterWood.Equality
{
    /// <summary>Runtime creation of an <see cref="IEqualityComparer{T}"/> based on property names.</summary>
    public static class EqualityComparer
    {
        static readonly ConcurrentDictionary<Key, object> _comparers = new ConcurrentDictionary<Key, object>();

        public static IEqualityComparer<T> Create<T>(params string[] properties) => (IEqualityComparer<T>)_comparers.GetOrAdd(new Key(typeof(T), properties, StringComparer.Ordinal), CreateInstance);

        public static IEqualityComparer<T> Create<T>(StringComparer stringComparer, params string[] properties) => (IEqualityComparer<T>)_comparers.GetOrAdd(new Key(typeof(T), properties, stringComparer), CreateInstance);

        private static object CreateInstance(Key key)
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Equality_" + key), AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule("mod1");
            var eqType = typeof(IEqualityComparer<>).GetTypeInfo().MakeGenericType(key.Type);
            var typeBuilder = modBuilder.DefineType($"BusterWood.{key.Type}Equality", TypeAttributes.Class, typeof(object), new[] { eqType });

            // define interface implementation
            var iface = typeof(IEqualityComparer<>).GetTypeInfo().MakeGenericType(new[] { key.Type });
            typeBuilder.AddInterfaceImplementation(iface);

            var strEq = typeBuilder.DefineField("strEq", typeof(StringComparer), FieldAttributes.InitOnly | FieldAttributes.Private);

            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(StringComparer) });
            var il = ctor.GetILGenerator();
            il.This().Call(typeof(object).GetTypeInfo().GetConstructor(new Type[0]));
            il.This().Arg(1).Store(strEq);
            il.Return();

            //typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            if (key.Type.GetTypeInfo().IsClass)
            {
                ClassEqualityBuilder.DefineEquals(key, typeBuilder, strEq);
                ClassEqualityBuilder.DefineGetHashCode(key, typeBuilder, strEq);
            }
            else
            {
                StructEqualityBulider.DefineEquals(key, typeBuilder, strEq);
                StructEqualityBulider.DefineGetHashCode(key, typeBuilder, strEq);
            }

            var tinfo = typeBuilder.CreateTypeInfo();
            return Activator.CreateInstance(tinfo.AsType(), key.StringComparer);
        }

    }
}
