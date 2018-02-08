using BusterWood.Collections;
using BusterWood.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BusterWood.Linq
{
    public static partial class Enumerables
    {
        //Expression version - needs code generation
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
                Swap(ref left, ref right);
                Swap(ref leftMem, ref rightMem);
                Swap(ref leftProp, ref rightProp);
            }

            if (leftProp.DeclaringType != typeof(T))
                throw new ArgumentException("Expected an expression in the form left.Prop == right.Prop");

            // left == T
            // right = TOther

            // find the related property to set - look for a property generic collection type on T of IEnumerable<TOther>
            var relatedProp = FindRelated(typeof(T), typeof(TOther));

            // start the assembly building process
            var key = typeof(T).Name + "To" + typeof(TOther).Name;
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SetRelationship_" + key), AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule("mod1");
            var typeBuilder = modBuilder.DefineType($"BusterWood.Linq.SetRelationship{key}", TypeAttributes.Class, typeof(object));

            var getKeyMethod = typeBuilder.DefineMethod("GetKey", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.Final, CallingConventions.Standard, rightProp.PropertyType, new[] { typeof(TOther) });
            {
                // generate the GetKey static method
                var il = getKeyMethod.GetILGenerator();
                il.Arg(0).GetProperty(relatedProp);
                il.Return();
            }

            {
                // generate the SetRelationship static method
                var setRelMethod = typeBuilder.DefineMethod("SetRelationship", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.Final, CallingConventions.Standard, null, new[] { typeof(IEnumerable<T>), typeof(IEnumerable<TOther>) });
                var il = setRelMethod.GetILGenerator();

                Type lookupType = typeof(HashLookup<,>).MakeGenericType(rightProp.PropertyType, typeof(TOther));
                var lookup = il.DeclareLocal(lookupType);
                var enm = il.DeclareLocal(typeof(IEnumerator<>).MakeGenericType(typeof(T)));
                var item = il.DeclareLocal(typeof(T));

                il.Arg(2); // push arg2 so we can later call ToHashLookup

                //// new Func<TOther, TKey> to static GetKey method
                var ctor = typeof(Func<>).MakeGenericType(typeof(TOther), rightProp.PropertyType).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
                il.Load(getKeyMethod).New(ctor);

                // lookup = ToHashLookup(IEnumerable<TOther>, Func<Tother, TKey>)
                var toLookup = typeof(Extensions).GetMethod(nameof(Extensions.ToHashLookup)).MakeGenericMethod(rightProp.PropertyType, typeof(TOther));
                il.Call(toLookup);
                il.Store(lookup);

                // enum = IEnumerable<T>.GetEnumerator
                il.Arg(1).CallVirt<IEnumerable<T>>("GetEnumerator").Store(enm);

                var exit = il.DefineLabel();
                var moveNext = il.DefineLabel();
                il.MarkLabel(moveNext);

                //if (!enm.MoveNext) return;
                il.Load(enm).CallVirt<IEnumerator<T>>("MoveNext").IfFalseGoto(exit);

                // var item = enum.Current
                il.Load(enm).GetProperty<IEnumerator<T>>("Current").Store(item);

                // master.Details = lookup[detail.OrderId]
                il.Load(item);
                il.Load(lookup);
                il.Load(item).GetProperty(leftProp);
                il.Call(lookupType.GetMethod("get_Item"));
                il.SetProperty(relatedProp);

                il.Goto(moveNext);

                il.MarkLabel(exit);
                il.Return();
            }

            var type = typeBuilder.CreateTypeInfo().AsType();
            //TODO: create delegate to call static method
            // call it
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

        private static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }

        //Expression version - needs code generation
        public static void SetRelationship2<T, TOther>(this IEnumerable<T> source, IEnumerable<TOther> other, Expression<Func<T, TOther, bool>> keyEquality)
        {
            var binExp = keyEquality.Body as BinaryExpression;
            var left = binExp.Left as MemberExpression;
            var leftMem = left.Member;
            var leftProp = leftMem as PropertyInfo;

            var right = binExp.Right as MemberExpression;
            var rightMem = right.Member;
            var rightProp = rightMem as PropertyInfo;

            if (leftProp.DeclaringType == typeof(T))
            {
                // left == T
                // right = TOther

                // find the related property to set - look for a property generic collection type on T of IEnumerable<TOther>
                var relatedProp = FindRelated<TOther>(typeof(T));

                var master = Expression.Parameter(typeof(T), "master");
                var details = Expression.Parameter(typeof(IEnumerable<TOther>), "details");
                var lookup = Expression.Variable(typeof(HashLookup<,>).MakeGenericType(rightProp.PropertyType, typeof(TOther)), "lookup");

                var lines = new List<Expression>();
                var loopBody = new List<Expression>();
                var afterLoop = Expression.Label();

                var toHash = typeof(BusterWood.Collections.Extensions).GetMethod("ToHashLookup");
                toHash = toHash.MakeGenericMethod(rightProp.PropertyType, typeof(TOther));

                //var keyFunc = //TODO: make an instance of Func<TOther, rightProp.PropertyType>

                //lines.Add(Expression.Assign(lookup, Expression.Call(toHash, details, keyFunc));
                lines.Add(Expression.Loop(Expression.Block(loopBody)));

                //loopBody.Add()
                throw new NotImplementedException();
            }
        }

        private static PropertyInfo FindRelated<TOther>(Type on)
        {
            var found = on.GetProperties()
                .Where(p => p.CanRead && typeof(IEnumerable<TOther>).IsAssignableFrom(p.PropertyType))
                .FirstOrDefault();

            if (found == null)
                throw new InvalidOperationException($"Cannot find collection property on {on.Name} of type IEnumerable<{typeof(TOther).Name}>");

            return found;
        }
    }
}
