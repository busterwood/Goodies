using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.MethodAttributes;
using static System.Reflection.CallingConventions;
using BusterWood.Reflection.Emit;

namespace BusterWood.Ducks
{
    static class Delegates
    {
        static readonly MostlyReadDictionary<TypePair, IDuckDelegateFactory> casts = new MostlyReadDictionary<TypePair, IDuckDelegateFactory>();

        internal static object Cast(Delegate from, Type to, MissingMethods missingMethods)
        {
            var factory = casts.GetOrAdd(new TypePair(from.GetType(), to, missingMethods), pair => CreateProxy(pair.From, pair.To, pair.MissingMethods));
            return factory.Create(from);
        }

        /// <param name="duck">The duck</param>
        /// <param name="interface">the interface to cast <paramref name="duck"/></param>
        /// <param name="missingMethods">How to handle missing methods</param>
        static IDuckDelegateFactory CreateProxy(Type duck, Type @interface, MissingMethods missingMethods)
        {
            if (duck == null)
                throw new ArgumentNullException(nameof(duck));
            if (@interface == null)
                throw new ArgumentNullException(nameof(@interface));
            if (!@interface.GetTypeInfo().IsInterface)
                throw new ArgumentException($"{@interface} is not an interface");

            string assemblyName = "Ducks_Instance_" + @interface.AsmName() + "_" + duck.AsmName() + ".dll";
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName);

            TypeBuilder proxyBuilder = moduleBuilder.DefineType("Proxy");
            foreach (var face in @interface.GetTypeInfo().GetInterfaces().Concat(@interface, typeof(IDuck)))
                proxyBuilder.AddInterfaceImplementation(face);
    
            var duckField = proxyBuilder.DefineField("duck", duck, FieldAttributes.Private | FieldAttributes.InitOnly);

            var ctor = proxyBuilder.DefineConstructor(duck, duckField);

            bool defined = false;
            foreach (var face in @interface.GetTypeInfo().GetInterfaces().Concat(@interface))
                DefineMembers(duck, face, proxyBuilder, duckField, ref defined);

            proxyBuilder.DefineUnwrapMethod(duckField);

            var factoryBuilder = CreateFactory(moduleBuilder, duck, ctor);
            proxyBuilder.CreateTypeInfo();
            return (IDuckDelegateFactory)Activator.CreateInstance(factoryBuilder.CreateTypeInfo().AsType());
        }

        static TypeBuilder CreateFactory(ModuleBuilder moduleBuilder, Type duck, ConstructorInfo ctor)
        {
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Factory");
            typeBuilder.AddInterfaceImplementation(typeof(IDuckDelegateFactory));
            typeBuilder.DefineDefaultConstructor(Public);

            var create = typeBuilder.DefineMethod("Create", Public | Virtual | Final, HasThis, typeof(object), new[] { typeof(Delegate) });
            var il = create.GetILGenerator();
            il.Arg1().Cast(duck); // cast delegate to duck
            il.New(ctor);  // call ctor(duck)
            il.Return();
            return typeBuilder;
        }

        static void DefineMembers(Type duck, Type @interface, TypeBuilder typeBuilder, FieldBuilder duckField, ref bool defined)
        {
            foreach (var method in @interface.GetTypeInfo().GetMethods().Where(mi => !mi.IsSpecialName)) // ignore get and set property methods
            {
                if (defined)
                    throw new InvalidCastException("More than one method one interface");
                CheckDelegateMatchesMethod(duck, @interface, method);
                var duckMethod = duck.GetTypeInfo().GetMethod("Invoke");
                typeBuilder.AddMethod(duckMethod, method, duckField);
                defined = true;
            }
        }

        static void CheckDelegateMatchesMethod(Type duck, Type @interface, MethodInfo method)
        {
            var duckMethod = duck.GetTypeInfo().GetMethod("Invoke");
            if (method.GetParameters().Length != duckMethod.GetParameters().Length)
                throw new InvalidCastException($"Delegate has a different number of parameters to {@interface.Name}.{method.Name}");

            int i = 0;
            var dps = duckMethod.GetParameters();
            foreach (var mp in method.GetParameters())
            {
                if (mp.ParameterType != dps[i].ParameterType)
                    throw new InvalidCastException($"Parameters types differs at index {i}, delegate parameter type {dps[i].ParameterType.Name} does not match {@interface.Name}.{method.Name} parameter type {mp.ParameterType.Name}");
                i++;
            }
            if (method.ReturnType != duckMethod.ReturnType)
                throw new InvalidCastException($"Return type differs, delegate returns {dps[i].ParameterType.Name} but method {@interface.Name}.{method.Name} returns {method.Name}");
        }
                
    }
}