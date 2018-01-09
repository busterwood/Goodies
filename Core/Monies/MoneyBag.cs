using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BusterWood.Monies
{
    /// <summary>A mutable container for money of one or more currencies, i.e. 10 GBP and 20 USD</summary>
    public class MoneyBag : IEnumerable<Money>
    {
        readonly Dictionary<string, decimal> _contents = new Dictionary<string, decimal>();

        /// <summary>Gets the amount of money in the bag of a specific currency</summary>
        /// <param name="currency">ISO Code of the currency, e.g. GBP, USD, EUR</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if currency is not an ISO Code</exception>
        public Money this[string currency]
        {
            get
            {
                Money.CheckIsoCode(currency);
                decimal amount;
                _contents.TryGetValue(currency, out amount);
                return new Money(amount, currency);
            }
        }

        /// <summary>Increases the amount of a currency in the bag</summary>
        public void Add(Money money)
        {
            Money.CheckIsoCode(money.Currency);
            decimal amount;
            _contents.TryGetValue(money.Currency, out amount);
            _contents[money.Currency] = amount + money.Amount;
        }

        /// <summary>Has any <see cref="Money"/> been added to this bag, or substracted from it?</summary>
        /// <param name="currency">ISO Code of the currency, e.g. GBP, USD, EUR</param>
        public bool Contains(string currency) => _contents.ContainsKey(currency);

        /// <summary>Enumerates the total values of all currencies in this bag</summary>
        public IEnumerator<Money> GetEnumerator()
        {
            foreach (var pair in _contents)
            {
                yield return new Money(pair.Value, pair.Key);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Does the bag contain a non-zero amount of any currency?</summary>
        public bool IsEmpty
        {
            get
            {
                if (_contents.Count == 0)
                    return true;
                foreach (var amount in _contents.Values)
                {
                    if (amount != 0m) return false;
                }
                return true;
            }
        }

        /// <summary>Removes all the money of a specific <paramref name="currency"/> from this bag.</summary>
        /// <param name="currency">The currency to remove</param>
        /// <returns>The amount of <see cref="Money"/> removed</returns>
        public Money Remove(string currency)
        {
            Money.CheckIsoCode(currency);
            decimal amount;
            if (_contents.TryGetValue(currency, out amount))
            {
                _contents.Remove(currency);
                return new Money(amount, currency);
            }
            return new Money(0m, currency);
        }

        /// <summary>Reduces the amount of a currency in the bag</summary>
        public void Subtract(Money money) => Add(-money);

        /// <summary>Shows what currency is in the bag</summary>
        public override string ToString()
        {
            var sb = new StringBuilder(10);
            sb.Append("[ ");
            foreach (var pair in _contents)
            {
                sb.Append(new Money(pair.Value, pair.Key).ToString()).Append("; ");
            }
            if (_contents.Count > 0)
                sb.Length -= 2; // remove last semicolon and space
            sb.Append(" ]");
            return sb.ToString();
        }
    }
}