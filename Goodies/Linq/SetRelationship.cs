using BusterWood.Collections;
using BusterWood.Goodies;
using BusterWood.Reflection.Emit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace BusterWood.Linq
{
    public static partial class Enumerables
    {
        static readonly MostlyReadDictionary<Key, Delegate> _methods = new MostlyReadDictionary<Key, Delegate>();

        //Expression version - needs code generation
        /// <summary>
        /// NOTE: only works on public types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOther"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <param name="keyEquality"></param>
        public static void SetRelationship<T, TOther>(this IEnumerable<T> source, IEnumerable<TOther> other, Expression<Func<T, TOther, bool>> keyEquality)
        {
            var binExp = keyEquality.Body as BinaryExpression;
            //TODO: check nulls
            //TODO: check this is an equality expression

            var left = binExp.Left as MemberExpression;
            var leftMem = left.Member;
            var leftProp = leftMem as PropertyInfo;

            var right = binExp.Right as MemberExpression;
            var rightMem = right.Member;
            var rightProp = rightMem as PropertyInfo;

            if (leftProp.DeclaringType == typeof(TOther))
            {
                // swap left and right
                Swapper.Swap(ref left, ref right);
                Swapper.Swap(ref leftMem, ref rightMem);
                Swapper.Swap(ref leftProp, ref rightProp);
            }

            if (leftProp.DeclaringType != typeof(T))
                throw new ArgumentException("Expected an expression in the form left.Prop == right.Prop");

            if (rightProp.DeclaringType != typeof(TOther))
                throw new ArgumentException("Expected an expression in the form left.Prop == right.Prop");

            var key = new Key(typeof(T), typeof(TOther), leftProp, rightProp);
            var action = (Action<IEnumerable<T>, IEnumerable<TOther>>)_methods.GetOrAdd(key, CreateSetRelationship);
            action(source, other);
        }

        static Delegate CreateSetRelationship(Key key)
        {
            // start the assembly building process
            var asnName = key.Master.Name + "To" + key.Detail.Name;
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SetRelationship_" + asnName), AssemblyBuilderAccess.RunAndCollect);
            var modBuilder = asmBuilder.DefineDynamicModule("mod1");
            var typeBuilder = modBuilder.DefineType($"BusterWood.Linq.SetRelationship{key}", TypeAttributes.Class, typeof(object));
            var getKeyMethod = DefineGetKey(key.RightProp, typeBuilder, key.Detail);
            var setRelationship = DefineSetRelationship(key, typeBuilder, getKeyMethod);
            var type = typeBuilder.CreateTypeInfo().AsType();

            //create delegate to call static method
            var method = type.GetMethod("SetRelationship");
            var actionType = typeof(Action<,>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(key.Master), typeof(IEnumerable<>).MakeGenericType(key.Detail));
            return method.CreateDelegate(actionType);
        }

        private static MethodBuilder DefineGetKey(PropertyInfo rightProp, TypeBuilder typeBuilder, Type detailsType)
        {
            var method = typeBuilder.DefineMethod("GetKey", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, rightProp.PropertyType, new[] { detailsType });
            var il = method.GetILGenerator();
            il.Arg(0).GetProperty(rightProp);
            il.Return();
            return method;
        }

        private static MethodBuilder DefineSetRelationship(Key key, TypeBuilder typeBuilder, MethodBuilder getKeyMethod)
        {
            var relatedProp = FindRelated(key.Master, key.Detail);

            Type masterEnumerable = typeof(IEnumerable<>).MakeGenericType(key.Master);
            Type detailEnumerable = typeof(IEnumerable<>).MakeGenericType(key.Detail);

            var method = typeBuilder.DefineMethod("SetRelationship", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, null, new[] { masterEnumerable, detailEnumerable });
            var il = method.GetILGenerator();

            Type lookupType = typeof(HashLookup<,>).MakeGenericType(key.RightProp.PropertyType, key.Detail);
            var lookup = il.DeclareLocal(lookupType);
            var enm = il.DeclareLocal(typeof(IEnumerator<>).MakeGenericType(key.Master));
            var master = il.DeclareLocal(key.Master);

            il.Arg(1); // push arg2 so we can later call ToHashLookup

            //// new Func<TOther, TKey> to static GetKey method
            var ctor = typeof(Func<,>).MakeGenericType(key.Detail, key.RightProp.PropertyType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
            il.Null().Load(getKeyMethod).New(ctor);

            // lookup = ToHashLookup(IEnumerable<TOther>, Func<Tother, TKey>)
            var toLookup = typeof(BusterWood.Collections.Extensions).GetMethod(nameof(BusterWood.Collections.Extensions.ToHashLookup)).MakeGenericMethod(key.RightProp.PropertyType, key.Detail);
            il.Call(toLookup);
            il.Store(lookup);

            // enum = IEnumerable<T>.GetEnumerator
            il.Arg(0).CallVirt(masterEnumerable.GetMethod("GetEnumerator")).Store(enm);

            var exit = il.DefineLabel();
            var moveNext = il.DefineLabel();
            il.MarkLabel(moveNext);

            //if (!enm.MoveNext) return;
            il.Load(enm).CallVirt<IEnumerator>("MoveNext").IfFalseGoto(exit);

            // var master = enum.Current
            Type masterEnumerator = typeof(IEnumerator<>).MakeGenericType(key.Master);
            il.Load(enm).GetProperty(masterEnumerator.GetProperty("Current")).Store(master);

            // master.Details = lookup[master.OrderId]
            il.Load(master);
            il.Load(lookup);
            il.Load(master).GetProperty(key.LeftProp);
            il.Call(lookupType.GetMethod("get_Item"));
            il.SetProperty(relatedProp);

            il.Goto(moveNext);

            il.MarkLabel(exit);
            il.Return();

            return method;
        }

        private static PropertyInfo FindRelated(Type master, Type detail)
        {
            var enumType = typeof(IEnumerable<>).MakeGenericType(detail);
            var found = master.GetProperties()
                .Where(p => p.CanRead && enumType.IsAssignableFrom(p.PropertyType))
                .FirstOrDefault();

            if (found == null)
                throw new InvalidOperationException($"Cannot find collection property on {master.Name} of type IEnumerable<{detail.Name}>");

            return found;
        }

        struct Key
        {
            public Type Master { get; }
            public Type Detail { get; }
            public PropertyInfo LeftProp { get; }
            public PropertyInfo RightProp { get; }
            //public PropertyInfo RelatedBy { get; }

            public Key(Type master, Type detail, PropertyInfo leftProp, PropertyInfo rightProp) : this()
            {
                Master = master;
                Detail = detail;
                LeftProp = leftProp;
                RightProp = rightProp;
            }
        }

    }
}
