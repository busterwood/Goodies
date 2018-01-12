using BusterWood.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace BusterWood.Mapping
{
    static class ObjectMapper
    {
        static readonly MostlyReadDictionary<TypePair, Delegate> MapMethods = new MostlyReadDictionary<TypePair, Delegate>();

        internal static Func<TIn, TOut, TOut> GetOrAdd<TIn, TOut>()
        {
            return (Func<TIn, TOut, TOut>)MapMethods.GetOrAdd(new TypePair(typeof(TIn), typeof(TOut)), _ => CreateMapDelegate(typeof(TIn), typeof(TOut)));
        }

        /// <summary>Fast copying code that generates a method that does the copy from one type to another</summary>
        static internal Delegate CreateMapDelegate(Type inType, Type outType)
        {
            Contract.Requires(outType != null);
            Contract.Requires(inType != null);
            Contract.Ensures(Contract.Result<Delegate>() != null);

            var result = Mapping.CreateFromSource(inType, outType, inType.Name);
            if (result.Mapped.Count == 0)
                throw new InvalidOperationException("No fields or properties were mapped");
            LambdaExpression lambdaExpression = CreateMappingLambda(inType, outType, result.Mapped);
            return lambdaExpression.Compile();
        }

        static LambdaExpression CreateMappingLambda(Type inType, Type outType, List<Mapping<Thing, Thing>> mapping)
        {
            Contract.Requires(mapping != null);
            Contract.Requires(outType != null);
            Contract.Requires(inType != null);
            Contract.Ensures(Contract.Result<LambdaExpression>() != null);

            if (outType.IsClass && outType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("Output type must have a parameterless constructor");

            var from = Expression.Parameter(inType, "from");
            var to = Expression.Parameter(outType, "to");
            var result = Expression.Parameter(outType, "result");

            var lines = new List<Expression>();
            if (outType.IsClass)
            {
                // result = to ?? new outType
                lines.Add(Expression.Assign(result, Expression.Coalesce(to, Expression.New(outType.GetConstructor(Type.EmptyTypes)))));
            }
            foreach (var map in mapping)
            {
                Expression readValue = ReadValue(from, map);
                lines.Add(Expression.Assign(Expression.PropertyOrField(result, map.To.Name), readValue));
            }
            lines.Add(result); // the return value

            var variables = new[] { result };
            var body = Expression.Block(variables, lines);
            var delegateType = typeof(Func<,,>).MakeGenericType(new[] { inType, outType, outType });
            return Expression.Lambda(delegateType, body, new[] { from, to });
        }

        static Expression ReadValue(ParameterExpression input, Mapping<Thing, Thing> map)
        {
            Expression value = Expression.PropertyOrField(input, map.From.Name);
            var fromType = map.From.Type;
            var toType = map.To.Type;
            if (fromType == toType)
            {
                return value;
            }
            if (Types.CanBeCast(fromType, toType)) // e.g. int to long
            {
                return Expression.Convert(value, toType);
            }
            if (Types.IsNullable(fromType))
            {
                Expression fromValueOrDefault = Expression.Call(value, fromType.GetMethod("GetValueOrDefault", Type.EmptyTypes));
                var fromArgType = Types.NullableOf(fromType);
                if (fromArgType == toType) // e.g. int? to int
                {
                    return fromValueOrDefault;
                }
                var cast = Types.GetExplicitCastOperator(fromArgType, toType); 
                if (cast != null) // e.g. Int32<>? to int
                {
                    return Expression.Call(cast, fromValueOrDefault);
                }
                if (Types.IsNullable(toType))
                {
                    // nullable<> to nullable<> conversion must handle null to null as a special case

                    var toArgType = toType.GetGenericArguments()[0];
                    if (Types.CanBeCast(fromArgType, toType)) // e.g. int? to long?
                    {
                        return Expression.Condition(Expression.PropertyOrField(value, "HasValue"), Expression.Convert(fromValueOrDefault, toType), Expression.Default(toType));
                    }

                    cast = Types.GetExplicitCastOperator(fromArgType, toArgType);  // e.g. Int32<>? to int?
                    if (cast != null)
                        fromValueOrDefault = Expression.Call(cast, fromValueOrDefault);

                    return Expression.Condition(Expression.PropertyOrField(value, "HasValue"), Expression.Convert(fromValueOrDefault, toType), Expression.Default(toType));
                }
                if (Types.CanBeCast(fromArgType, toType)) // e.g. int? to long
                {
                    return Expression.Convert(fromValueOrDefault, toType);
                }
            }
            if (Types.IsNullable(toType))
            {
                var toArgType = toType.GetGenericArguments()[0];
                var cast2 = Types.GetExplicitCastOperator(fromType, toArgType); // e.g. Int32<> to int?
                if (cast2 != null)
                {
                    return Expression.Convert(Expression.Call(cast2, value), toType);
                }
            }
            var cast3 = Types.GetExplicitCastOperator(fromType, toType); // e.g. Int32<> to int
            if (cast3 != null)
            {
                return Expression.Call(cast3, value);
            }
            throw new InvalidOperationException("Should never get here has types compatibility has been checked");
        }

    }

    struct TypePair : IEquatable<TypePair>
    {
        public readonly Type In;
        public readonly Type Out;

        public TypePair(Type @in, Type @out)
        {
            In = @in;
            Out = @out;
        }

        public bool Equals(TypePair other) => In == other.In && Out == other.Out;

        public override bool Equals(object obj) => obj is TypePair && Equals((TypePair)obj);

        public override int GetHashCode()
        {
            unchecked
            {
                return (In.GetHashCode() * 397) ^ Out.GetHashCode();
            }
        }
    }

}
