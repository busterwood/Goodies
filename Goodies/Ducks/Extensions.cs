using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.MethodAttributes;
using static System.Reflection.CallingConventions;
using BusterWood.Reflection.Emit;

namespace BusterWood.Ducks
{
    static class Extensions
    {
        public static readonly Type[] EmptyTypes = new Type[0];

        public static Type[] ParameterTypes(this MethodInfo method) => method.GetParameters().Select(p => p.ParameterType).ToArray();

        public static Type[] ParameterTypes(this PropertyInfo method) => method.GetIndexParameters().Select(p => p.ParameterType).ToArray();

        public static string AsmName(this Type type) => type.Name.Replace(".", "_").Replace("+", "-");

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, params T[] items) => Enumerable.Concat(source, items);

        public static MethodInfo FindDuckMethod(this Type duck, MethodInfo method, BindingFlags bindingFlags, MissingMethods missingMethods)
        {
            try
            {
                var found = duck.GetRuntimeMethod(method.Name, method.ParameterTypes());
                if (found == null && missingMethods == MissingMethods.NotImplemented)
                    return null;
                if (found == null)
                    throw new InvalidCastException($"Type {duck.Name} does not have a {bindingFlags} method {method.Name}");
                if (found.IsStatic && (bindingFlags & BindingFlags.Instance) != 0)
                    throw new InvalidCastException($"Type {duck.Name} has a method {bindingFlags} method {method.Name} but it is static");
                if (!found.IsStatic && (bindingFlags & BindingFlags.Static) != 0)
                    throw new InvalidCastException($"Type {duck.Name} has a method {bindingFlags} method {method.Name} but it is not static");
                if (found == null)
                    throw new InvalidCastException($"Type {duck.Name} does not have a {bindingFlags} method {method.Name}");
                return found;
            }
            catch (AmbiguousMatchException)
            {
                throw new InvalidCastException($"Type {duck.Name} has an ambiguous match for {bindingFlags} method {method.Name}"); //TODO: parameter list
            }
        }

        public static PropertyInfo FindDuckProperty(this Type duck, PropertyInfo prop, BindingFlags bindingFlags, MissingMethods missingMethods)
        {
            try
            {
                var found = duck.GetRuntimeProperty(prop.Name); 
                if (found == null && missingMethods == MissingMethods.NotImplemented)
                    return null;
                if (found == null)
                    throw new InvalidCastException($"Type {duck.Name} does not have a {bindingFlags} property {prop.Name} or parameters types do not match");
                if (found.PropertyType != prop.PropertyType)
                    throw new InvalidCastException($"Type {duck.Name} property {prop.Name} does not not match interface property type {prop.PropertyType.Name}");
                if (prop.CanRead && !found.CanRead)
                    throw new InvalidCastException($"Type {duck.Name} does not have a {bindingFlags} get property {prop.Name}");
                if (prop.CanWrite && !found.CanWrite)
                    throw new InvalidCastException($"Type {duck.Name} does not have a {bindingFlags} set property {prop.Name}");
                return found;
            }
            catch (AmbiguousMatchException)
            {
                throw new InvalidCastException($"Type {duck.Name} has an ambiguous match for property {prop.Name}"); //TODO: parameter list
            }
        }

        public static EventInfo FindDuckEvent(this Type duck, EventInfo evt, BindingFlags bindingFlags, MissingMethods missingMethods)
        {
            try
            {
                var found = duck.GetTypeInfo().GetEvent(evt.Name, bindingFlags);
                if (found == null && missingMethods == MissingMethods.NotImplemented)
                    return null;
                if (found == null)
                    throw new InvalidCastException($"Type {duck.Name} does not have a {bindingFlags} event {evt.Name}");
                if (evt.EventHandlerType != found.EventHandlerType)
                    throw new InvalidCastException($"Type {duck.Name} {bindingFlags} event {evt.Name} has type {found.EventHandlerType.Name} but expected type {evt.EventHandlerType.Name}");
                return found;
            }
            catch (AmbiguousMatchException)
            {
                throw new InvalidCastException($"Type {duck.Name} has an ambiguous match for {bindingFlags} event {evt.Name}");
            }
        }

        public static ConstructorBuilder DefineConstructor(this TypeBuilder typeBuilder, Type duck, FieldBuilder duckField)
        {
            var ctor = typeBuilder.DefineConstructor(Public, HasThis, new[] { duck });
            var il = ctor.GetILGenerator();
            il.This().Arg(1).Store(duckField); // store the parameter in the duck field
            il.This().Call(typeof(object).GetTypeInfo().GetConstructor(EmptyTypes));
            il.Return();
            return ctor;
        }

        public static MethodBuilder DefineUnwrapMethod(this TypeBuilder typeBuilder, FieldBuilder duckField)
        {
            var create = typeBuilder.DefineMethod(nameof(IDuck.Unwrap), Public | Virtual | Final, HasThis, typeof(object), new Type[0]);
            var il = create.GetILGenerator();
            il.This().Load(duckField); // push duck field
            il.Cast(typeof(object));   // cast duck to object
            il.Return();
            return create;
        }

        public static MethodBuilder DefineStaticCreateMethod(this TypeBuilder typeBuilder, Type duck, ConstructorBuilder ctor, Type paramType)
        {
            var create = typeBuilder.DefineMethod("Create", Public | MethodAttributes.Static, Standard, typeof(object), new[] { paramType });
            var il = create.GetILGenerator();
            il.Arg(0).Cast(duck);   // cast obj to duck
            il.New(ctor);  // call ctor(duck)
            il.Return();
            return create;
        }

        public static MethodBuilder AddMethod(this TypeBuilder typeBuilder, MethodInfo duckMethod, MethodInfo interfaceMethod, FieldBuilder duckField)
        {
            var mb = typeBuilder.DefineMethod(interfaceMethod.Name, Public | Virtual | Final, HasThis, interfaceMethod.ReturnType, interfaceMethod.ParameterTypes());
            var il = mb.GetILGenerator();

            if (duckMethod == null)
            {
                // throw a not implemented exception 
                il.ThrowException(typeof(NotImplementedException));
                il.Return();
                return mb;
            }
            
            il.This().Load(duckField); // push duck field

            // push all the arguments onto the stack
            int i = 1;
            foreach (var p in interfaceMethod.GetParameters())
                il.Arg(i++);

            // call the duck's method
            il.CallVirt(duckMethod);
            il.Return();
            return mb;
        }
        
    }
}