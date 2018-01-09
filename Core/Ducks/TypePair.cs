using System;

namespace BusterWood.Ducks
{
    struct TypePair : IEquatable<TypePair>
    {
        public readonly Type From;
        public readonly Type To;
        public readonly MissingMethods MissingMethods;

        public TypePair(Type from, Type to, MissingMethods missingMethods)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));
            From = from;
            To = to;
            MissingMethods = missingMethods;
        }

        public bool Equals(TypePair other) => Equals(From, other.From) && Equals(To, other.To) && MissingMethods == other.MissingMethods;

        public override bool Equals(object obj) => obj is TypePair ? Equals((TypePair)obj) : false;

        public override int GetHashCode() => From == null ? 0 : From.GetHashCode() + To.GetHashCode() + MissingMethods.GetHashCode();
    }
}
