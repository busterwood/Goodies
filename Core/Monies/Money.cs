using System;

namespace BusterWood.Monies
{
    /// <summary>An immutable amount of a specific currency, e.g. 10.99 GBP or 100.01 USD</summary>
    public struct Money : IEquatable<Money>, IComparable<Money>
    {
        /// <summary>No amount of any currency</summary>
        public static Money None;

        /// <summary>The amount of money</summary>
        public decimal Amount { get; }

        /// <summary>ISO Code of the currency, e.g. GBP, USD, EUR</summary>
        public string Currency { get; }

        /// <summary>Create a new currency value</summary>
        /// <param name="amount">Amount of money</param>
        /// <param name="currency">ISO Code of the currency, e.g. GBP, USD, EUR</param>
        public Money(decimal amount, string currency)
        {
            CheckIsoCode(currency);
            Amount = amount;
            Currency = currency;
        }

        internal static void CheckIsoCode(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new ArgumentOutOfRangeException(nameof(currency), "Must be 3 character ISO code");
        }

        /// <summary>Returns a hash code for this money</summary>
        public override int GetHashCode() => Amount.GetHashCode() ^ (Currency == null ? 0 : Currency.GetHashCode());

        /// <summary>Equality based on amount and currency matching</summary>
        public override bool Equals(object obj) => obj is Money && base.Equals((Money)obj);

        /// <summary>Equality based on amount and currency matching</summary>
        public bool Equals(Money other) => Amount == other.Amount && string.Equals(Currency, other.Currency, StringComparison.Ordinal);

        /// <summary>Returns a string containing the <see cref="Amount"/> and <see cref="Currency"/></summary>
        public override string ToString()
        {
            int? dps = Currencies.DecimalPlaces(Currency);
            if (dps == null)
                return Amount + " " + Currency;
            return Amount.ToString("N"+dps) + " " + Currency;
        }

        /// <summary>Returns a string containing a formatted <see cref="Amount"/> and <see cref="Currency"/></summary>
        public string ToString(string format) => $"{Amount.ToString(format)} {Currency}";

        /// <summary>Ordered comparision of money amounts.  Sorts by <see cref="Currency"/> and then by <see cref="Amount"/></summary>
        public int CompareTo(Money other)
        {
            int result = (Currency?.CompareTo(other.Currency ?? "")).GetValueOrDefault();
            if (result != 0) return result;
            return Amount.CompareTo(other.Amount);
        }

        /// <summary>Equality based on amount and currency matching</summary>
        public static bool operator ==(Money left, Money right) => left.Equals(right);

        /// <summary>Inequality based on amount or currency differing</summary>
        public static bool operator !=(Money left, Money right) => !left.Equals(right);

        /// <summary>Negation of an amount of money</summary>
        public static Money operator -(Money m) => new Money(-m.Amount, m.Currency);

        /// <summary>Addition of two money amounts</summary>
        public static Money operator +(Money left, Money right)
        {
            CheckSameCurrency(left.Currency, right.Currency);
            return new Money(left.Amount + right.Amount, left.Currency);
        }

        /// <summary>Subtraction of two money amounts</summary>
        public static Money operator -(Money left, Money right)
        {
            CheckSameCurrency(left.Currency, right.Currency);
            return new Money(left.Amount - right.Amount, left.Currency);
        }

        /// <summary>Multiply money a number of times</summary>
        public static Money operator *(Money value, decimal times) => new Money(value.Amount * times, value.Currency);

        /// <summary>Divide money by a number</summary>
        public static Money operator /(Money value, decimal divisor) => new Money(value.Amount / divisor, value.Currency);

        /// <summary>Is the first value less than the second value?</summary>
        public static bool operator <(Money left, Money right)
        {
            CheckSameCurrency(left.Currency, right.Currency);
            return left.Amount < right.Amount;
        }

        /// <summary>Is the first value more than the second value?</summary>
        public static bool operator >(Money left, Money right)
        {
            CheckSameCurrency(left.Currency, right.Currency);
            return left.Amount > right.Amount;
        }

        /// <summary>Is the first value less than or equal to the second value?</summary>
        public static bool operator <=(Money left, Money right)
        {
            CheckSameCurrency(left.Currency, right.Currency);
            return left.Amount <= right.Amount;
        }

        /// <summary>Is the first value more than or equal to the second value?</summary>
        public static bool operator >=(Money left, Money right)
        {
            CheckSameCurrency(left.Currency, right.Currency);
            return left.Amount >= right.Amount;
        }

        private static void CheckSameCurrency(string left, string right)
        {
            if (!string.Equals(left, right))
                throw new InvalidOperationException($"Different currencies: {left} and {right}");
        }
    }

    public static class Extensions
    {
        public static Money GBP(this decimal value) => new Money(value, "GBP");
        public static Money USD(this decimal value) => new Money(value, "USD");
        public static Money EUR(this decimal value) => new Money(value, "EUR");
    }
}
