using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BusterWood.Reflection.Emit;

namespace BusterWood.Equality
{

    static class StructEqualityBulider
    {
        public static MethodBuilder DefineEquals(Key key, TypeBuilder typeBuilder, FieldBuilder strEq)
        {
            var method = typeBuilder.DefineMethod("Equals", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis, typeof(bool), new[] { key.Type, key.Type });
            var il = method.GetILGenerator();
            var returnFalse = il.DefineLabel();
            var returnTrue = il.DefineLabel();

            IEnumerable<MethodInfo> equalsOverloads = typeof(object).GetTypeInfo().GetDeclaredMethods(nameof(object.Equals));
            var equals = equalsOverloads.First(m => m.GetParameters().Length == 1);
            var equalsXY = equalsOverloads.First(m => m.GetParameters().Length == 2);
            var strEquals = typeof(StringComparer).GetTypeInfo().GetDeclaredMethods("Equals")
                .First(m => m.GetParameters().Length == 2 && m.GetParameters().All(p => p.ParameterType == typeof(string)));
            foreach (var propName in key.Properties)
            {
                var prop = key.Type.GetTypeInfo().GetDeclaredProperty(propName);
                TypeInfo propType = prop.PropertyType.GetTypeInfo();
                var equatable = typeof(IEquatable<>).GetTypeInfo().MakeGenericType(new[] { prop.PropertyType });
                if (prop.PropertyType == typeof(string))
                {
                    // string equality via field: strEq.Equals(x, y)
                    il.This().Load(strEq);
                    il.Arg1Address().CallGetProperty(prop); // cast to object?
                    il.Arg2Address().CallGetProperty(prop);
                    il.CallVirt(strEquals).IfFalseGoto(returnFalse);
                }
                else if (propType.IsClass)
                {
                    // reference type call object.Equals(x, y)
                    il.Arg1Address().CallGetProperty(prop); // cast to object?
                    il.Arg2Address().CallGetProperty(prop);
                    il.Call(equalsXY).IfFalseGoto(returnFalse);
                }
                else if (propType.IsPrimitive)
                {
                    il.Arg1Address().CallGetProperty(prop); // cast to object?
                    il.Arg2Address().CallGetProperty(prop);
                    il.IfNotEqualGoto(returnFalse);
                }
                //else if (propType.ImplementedInterfaces.Contains(equatable))
                //{
                //    IEnumerable<MethodInfo> eqOverloads = propType.GetDeclaredMethods(nameof(object.Equals));
                //    var equitableXY = equalsOverloads.First(m => m.GetParameters().Length == 2 && m.GetParameters().All(p => p.ParameterType == propType));
                //    il.Arg1().CallGetProperty(prop); // cast to object?
                //    il.Arg2().CallGetProperty(prop);
                //    il.CallVirt(equitableXY).IfFalseGoto(returnFalse);
                //}
                else
                {
                    // ValueType type
                    il.Arg1Address().CallGetProperty(prop);
                    il.Arg2Address().CallGetProperty(prop);
                    il.Call(equals).IfFalseGoto(returnFalse);
                }
            }

            // drops through to here is all properties are equal
            il.MarkLabel(returnTrue);
            il.Load(1).Return();

            // something is not equal
            il.MarkLabel(returnFalse);
            il.Load(0).Return();
            return method;
        }

        public static MethodBuilder DefineGetHashCode(Key key, TypeBuilder typeBuilder, FieldBuilder strEq)
        {
            var method = typeBuilder.DefineMethod("GetHashCode", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis, typeof(int), new[] { key.Type });
            var il = method.GetILGenerator();
            var getHashCodeOverLoads = typeof(object).GetTypeInfo().GetDeclaredMethods(nameof(object.GetHashCode));
            var getHashCode = getHashCodeOverLoads.First(m => m.GetParameters().Length == 0);

            var hc = il.DeclareLocal<int>();
            il.Load(0).Store(hc); // hc = 0

            var strGetHashCode = typeof(StringComparer).GetTypeInfo().GetDeclaredMethods("GetHashCode").First(m => m.GetParameters().Length == 1);
            foreach (var propName in key.Properties)
            {
                var prop = key.Type.GetTypeInfo().GetDeclaredProperty(propName);
                TypeInfo propType = prop.PropertyType.GetTypeInfo();
                var equatable = typeof(IEquatable<>).GetTypeInfo().MakeGenericType(new[] { prop.PropertyType });
                if (prop.PropertyType == typeof(string))
                {
                    // if (obj.Prop != null) hc += strEq.GetHashCode(obj.Prop)
                    var next = il.DefineLabel();
                    var temp = il.DeclareLocal(prop.PropertyType);
                    il.Arg1Address().CallGetProperty(prop).Store(temp);
                    il.Load(temp).Null().IfEqualGoto(next);
                    il.This().Load(strEq);
                    il.Load(temp);
                    il.CallVirt(strGetHashCode).Load(hc).Add().Store(hc);
                    il.MarkLabel(next);
                }
                else if (propType.IsClass)
                {
                    // if (prop != null) hc += prop.GetHashCode();
                    var next = il.DefineLabel();
                    var temp = il.DeclareLocal(prop.PropertyType);
                    il.Arg1Address().CallGetProperty(prop).Store(temp);
                    il.Load(temp).Null().IfEqualGoto(next);
                    il.Load(temp).CallVirt(getHashCode).Load(hc).Add().Store(hc);
                    il.MarkLabel(next);
                }
                else // is a struct
                {
                    // var temp = obj.Prop;
                    // hc += temp.GetHashCode();
                    var ghc = propType.GetDeclaredMethods(nameof(object.GetHashCode)).First(m => m.GetParameters().Length == 0); // GetHashCode must be overridden on a struct
                    var temp = il.DeclareLocal(prop.PropertyType);
                    il.Arg1Address().CallGetProperty(prop).Store(temp);
                    il.LoadAddress(temp).Call(ghc).Load(hc).Add().Store(hc);
                }
            }
            il.Load(hc).Return(); // todo: generate hash code
            return method;
        }
    }
}
