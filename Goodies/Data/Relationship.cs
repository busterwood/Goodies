using BusterWood.Collections;
using BusterWood.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace BusterWood.Data
{
    public static partial class Extensions
    {
#if !REAL_MAPPER
        public static List<T> ToList<T>(this DbDataReader reader)
        {
            return new List<T>();
        }
#endif 
        public static Relationships<T1, T2> Read<T1, T2>(this DbDataReader reader)
        {
            List<T1> ones = reader.ToList<T1>();
            Contract.Assert(reader.NextResult());
            List<T2> twos = reader.ToList<T2>();
            return new Relationships<T1, T2>(ones, twos);
        }

        public static Relationships<T1, T2, T3> Read<T1, T2, T3>(this DbDataReader reader)
        {
            List<T1> ones = reader.ToList<T1>();
            Contract.Assert(reader.NextResult());
            List<T2> twos = reader.ToList<T2>();
            Contract.Assert(reader.NextResult());
            List<T3> threes = reader.ToList<T3>();
            return new Relationships<T1, T2, T3>(ones, twos, threes);
        }

        public static Relationships<T1, T2, T3, T4> Read<T1, T2, T3, T4>(this DbDataReader reader)
        {
            List<T1> ones = reader.ToList<T1>();
            Contract.Assert(reader.NextResult());
            List<T2> twos = reader.ToList<T2>();
            Contract.Assert(reader.NextResult());
            List<T3> threes = reader.ToList<T3>();
            Contract.Assert(reader.NextResult());
            List<T4> fours = reader.ToList<T4>();
            return new Relationships<T1, T2, T3, T4>(ones, twos, threes, fours);
        }

        public static Relationships<T1, T2, T3, T4, T5> Read<T1, T2, T3, T4, T5>(this DbDataReader reader)
        {
            List<T1> ones = reader.ToList<T1>();
            Contract.Assert(reader.NextResult());
            List<T2> twos = reader.ToList<T2>();
            Contract.Assert(reader.NextResult());
            List<T3> threes = reader.ToList<T3>();
            Contract.Assert(reader.NextResult());
            List<T4> fours = reader.ToList<T4>();
            Contract.Assert(reader.NextResult());
            List<T5> fives = reader.ToList<T5>();
            return new Relationships<T1, T2, T3, T4, T5>(ones, twos, threes, fours, fives);
        }
    }

    public interface IRelate
    {
        void Relate();
    }

    internal class Relationship<T1, T2, TKey> : IRelate
    {
        readonly IEnumerable<T1> ones;
        readonly IEnumerable<T2> twos;
        readonly Func<T1, TKey> t1Related;
        readonly Func<T2, TKey> t2Related;
        readonly Action<T1, IReadOnlyList<T2>> addRelated;

        public Relationship(IEnumerable<T1> ones, IEnumerable<T2> twos, Func<T1, TKey> t1Related, Func<T2, TKey> t2Related, Action<T1, IReadOnlyList<T2>> addRelated)
        {
            this.ones = ones;
            this.twos = twos;
            this.t1Related = t1Related;
            this.t2Related = t2Related;
            this.addRelated = addRelated;
        }

        public void Relate()
        {
            var t2lookup = twos.ToHashLookup(t2Related);
            foreach (var t1 in ones)
            {
                var key = t1Related(t1);
                addRelated(t1, t2lookup[key]);
            }
        }
    }

    public abstract class Relationships : IRelate
    {
        internal readonly List<IRelate> relationships = new List<IRelate>();

        public void Relate()
        {
            foreach (var r in relationships)
            {
                r.Relate();
            }
        }
    }

    public class Relationships<T1, T2> : Relationships
    {
        public IEnumerable<T1> First { get; }
        public IEnumerable<T2> Second { get; }

        public Relationships(IEnumerable<T1> ones, IEnumerable<T2> twos)
        {
            First = ones;
            Second = twos;
        }

        //TODO: HasOne()

        public Relationships<T1, T2> HasMany<TKey>(Func<T1, TKey> t1Related, Func<T2, TKey> t2Related, Action<T1, IReadOnlyList<T2>> addRelated)
        {
            relationships.Add(new Relationship<T1, T2, TKey>(First, Second, t1Related, t2Related, addRelated));
            return this;
        }

        public new Relationships<T1, T2> Relate()
        {
            base.Relate();
            return this;
        }
    }

    public static partial class Extensions
    {
        //TODO: convert expression to relationship
        public static Relationships<T1, T2> HasMany<T1, T2>(this Relationships<T1, T2> rel, Expression<Func<T1, T2>> expression, Action<T1, IReadOnlyList<T2>> addRelated)
        {
            throw new NotImplementedException("Extract relationship from expression");
            //expression.
        }

    }

    public class Relationships<T1, T2, T3> : Relationships
    {
        public IEnumerable<T1> First { get; }
        public IEnumerable<T2> Second { get; }
        public IEnumerable<T3> Third { get; }

        public Relationships(IEnumerable<T1> ones, IEnumerable<T2> twos, IEnumerable<T3> threes)
        {
            First = ones;
            Second = twos;
            Third = threes;
        }

        public Relationships<T1, T2, T3> HasMany<TKey>(Func<T1, TKey> t1Related, Func<T2, TKey> t2Related, Action<T1, IReadOnlyList<T2>> addRelated)
        {
            relationships.Add(new Relationship<T1, T2, TKey>(First, Second, t1Related, t2Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3> HasMany<TKey>(Func<T1, TKey> t1Related, Func<T3, TKey> t3Related, Action<T1, IReadOnlyList<T3>> addRelated)
        {
            relationships.Add(new Relationship<T1, T3, TKey>(First, Third, t1Related, t3Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3> HasMany<TKey>(Func<T2, TKey> t2Related, Func<T3, TKey> t3Related, Action<T2, IReadOnlyList<T3>> addRelated)
        {
            relationships.Add(new Relationship<T2, T3, TKey>(Second, Third, t2Related, t3Related, addRelated));
            return this;
        }

        public new Relationships<T1, T2, T3> Relate()
        {
            base.Relate();
            return this;
        }

    }

    public class Relationships<T1, T2, T3, T4> : Relationships
    {
        public IEnumerable<T1> First { get; }
        public IEnumerable<T2> Second { get; }
        public IEnumerable<T3> Third { get; }
        public IEnumerable<T4> Fourth { get; }

        public Relationships(IEnumerable<T1> ones, IEnumerable<T2> twos, IEnumerable<T3> threes, IEnumerable<T4> fours)
        {
            First = ones;
            Second = twos;
            Third = threes;
            Fourth = fours;
        }

        public Relationships<T1, T2, T3, T4> HasMany<TKey>(Func<T1, TKey> t1Related, Func<T2, TKey> t2Related, Action<T1, IReadOnlyList<T2>> addRelated)
        {
            relationships.Add(new Relationship<T1, T2, TKey>(First, Second, t1Related, t2Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4> HasMany<TKey>(Func<T1, TKey> t1Related, Func<T3, TKey> t3Related, Action<T1, IReadOnlyList<T3>> addRelated)
        {
            relationships.Add(new Relationship<T1, T3, TKey>(First, Third, t1Related, t3Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4> HasMany<TKey>(Func<T1, TKey> t1Related, Func<T4, TKey> t4Related, Action<T1, IReadOnlyList<T4>> addRelated)
        {
            relationships.Add(new Relationship<T1, T4, TKey>(First, Fourth, t1Related, t4Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4> HasMany<TKey>(Func<T2, TKey> t2Related, Func<T3, TKey> t3Related, Action<T2, IReadOnlyList<T3>> addRelated)
        {
            relationships.Add(new Relationship<T2, T3, TKey>(Second, Third, t2Related, t3Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4> HasMany<TKey>(Func<T2, TKey> t2Related, Func<T4, TKey> t4Related, Action<T2, IReadOnlyList<T4>> addRelated)
        {
            relationships.Add(new Relationship<T2, T4, TKey>(Second, Fourth, t2Related, t4Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4> HasMany<TKey>(Func<T3, TKey> t3Related, Func<T4, TKey> t4Related, Action<T3, IReadOnlyList<T4>> addRelated)
        {
            relationships.Add(new Relationship<T3, T4, TKey>(Third, Fourth, t3Related, t4Related, addRelated));
            return this;
        }

        public new Relationships<T1, T2, T3, T4> Relate()
        {
            base.Relate();
            return this;
        }

    }

    public class Relationships<T1, T2, T3, T4, T5> : Relationships
    {
        public IEnumerable<T1> First { get; }
        public IEnumerable<T2> Second { get; }
        public IEnumerable<T3> Third { get; }
        public IEnumerable<T4> Fourth { get; }
        public IEnumerable<T5> Fifth { get; }

        public Relationships(IEnumerable<T1> ones, IEnumerable<T2> twos, IEnumerable<T3> threes, IEnumerable<T4> fours, IEnumerable<T5> fives)
        {
            First = ones;
            Second = twos;
            Third = threes;
            Fourth = fours;
            Fifth = fives;
        }

        public Relationships<T1, T2, T3, T4, T5> HasMany<TKey>(Func<T1, TKey> t1Related, Func<T2, TKey> t2Related, Action<T1, IReadOnlyList<T2>> addRelated)
        {
            relationships.Add(new Relationship<T1, T2, TKey>(First, Second, t1Related, t2Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T1, TKey> t1Related, Func<T3, TKey> t3Related, Action<T1, IReadOnlyList<T3>> addRelated)
        {
            relationships.Add(new Relationship<T1, T3, TKey>(First, Third, t1Related, t3Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T1, TKey> t1Related, Func<T4, TKey> t4Related, Action<T1, IReadOnlyList<T4>> addRelated)
        {
            relationships.Add(new Relationship<T1, T4, TKey>(First, Fourth, t1Related, t4Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T1, TKey> t1Related, Func<T5, TKey> t5Related, Action<T1, IReadOnlyList<T5>> addRelated)
        {
            relationships.Add(new Relationship<T1, T5, TKey>(First, Fifth, t1Related, t5Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T2, TKey> t2Related, Func<T3, TKey> t3Related, Action<T2, IReadOnlyList<T3>> addRelated)
        {
            relationships.Add(new Relationship<T2, T3, TKey>(Second, Third, t2Related, t3Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T2, TKey> t2Related, Func<T4, TKey> t4Related, Action<T2, IReadOnlyList<T4>> addRelated)
        {
            relationships.Add(new Relationship<T2, T4, TKey>(Second, Fourth, t2Related, t4Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T2, TKey> t2Related, Func<T5, TKey> t5Related, Action<T2, IReadOnlyList<T5>> addRelated)
        {
            relationships.Add(new Relationship<T2, T5, TKey>(Second, Fifth, t2Related, t5Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T3, TKey> t3Related, Func<T4, TKey> t4Related, Action<T3, IReadOnlyList<T4>> addRelated)
        {
            relationships.Add(new Relationship<T3, T4, TKey>(Third, Fourth, t3Related, t4Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T3, TKey> t3Related, Func<T5, TKey> t5Related, Action<T3, IReadOnlyList<T5>> addRelated)
        {
            relationships.Add(new Relationship<T3, T5, TKey>(Third, Fifth, t3Related, t5Related, addRelated));
            return this;
        }

        public Relationships<T1, T2, T3, T4, T5> AddRelationship<TKey>(Func<T4, TKey> t4Related, Func<T5, TKey> t5Related, Action<T4, IReadOnlyList<T5>> addRelated)
        {
            relationships.Add(new Relationship<T4, T5, TKey>(Fourth, Fifth, t4Related, t5Related, addRelated));
            return this;
        }

        public new Relationships<T1, T2, T3, T4, T5> Relate()
        {
            base.Relate();
            return this;
        }
    }

}
