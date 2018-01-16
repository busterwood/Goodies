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
            return ReadValueRecursive(value, map.From.Type, map.To.Type);
        }

        static Expression ReadValueRecursive(Expression value, Type fromType, Type toType)
        {
            if (fromType == toType)
            {
                return value;
            }

            if (Types.IsNullable(fromType))
            {
                if (Types.IsNullable(toType))
                {
                    var toArgType = toType.GetGenericArguments()[0];
                    // recursive call with unwrapped generic type
                    return Expression.Condition(Expression.PropertyOrField(value, "HasValue"), Expression.Convert(ReadValueRecursive(value, fromType, toArgType), toType), Expression.Default(toType));
                }

                // then toType is not nullable at this point

                // use value or default of nullable value
                var fromArgType = fromType.GetGenericArguments()[0];
                value = Expression.Call(value, fromType.GetMethod("GetValueOrDefault", Type.EmptyTypes));
                fromType = fromArgType;

                // we might have the correct types now
                if (fromType == toType)
                {
                    return value;
                }
            }

            if (Types.CanBeCast(fromType, toType)) // e.g. int? to long
            {
                return Expression.Convert(value, toType);
            }

            var cast = Types.GetExplicitCastOperator(fromType, toType);
            if (cast != null) // e.g. Int32<>? to int
            {
                return Expression.Call(cast, value);
            }

            throw new InvalidOperationException("Should never get here as type compatibility has already been checked");
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
