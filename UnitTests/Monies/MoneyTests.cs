using BusterWood.Monies;
using NUnit.Framework;
using System;

namespace UnitTests
{
    [TestFixture]
    public class MoneyTests
    {
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        [TestCase("A")]
        [TestCase("AB")]
        [TestCase("ABCC")]
        public void cannot_create_money_with_invalid_isocode(string isoCode)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Money(1m, isoCode));
        }

        [Test]
        public void can_add_money_with_same_currency()
        {
            var result = new Money(1, "GBP") + new Money(1, "GBP");
            Assert.AreEqual(2m, result.Amount, "Amount");
            Assert.AreEqual("GBP", result.Currency, "IsoCode");
        }

        [Test]
        public void cannot_add_money_with_different_currency()
        {
            Assert.Throws<InvalidOperationException>(() => { var result = new Money(1, "GBP") + new Money(1, "EUR"); });
        }

        [Test]
        public void can_subtract_money_with_same_currency()
        {
            var result = new Money(2, "GBP") - new Money(1, "GBP");
            Assert.AreEqual(1m, result.Amount, "Amount");
            Assert.AreEqual("GBP", result.Currency, "IsoCode");
        }

        [Test]
        public void cannot_subtract_money_with_different_currency()
        {
            Assert.Throws<InvalidOperationException>(() => { var result = new Money(1, "GBP") - new Money(1, "EUR"); });
        }

        [Test]
        public void can_multiply_by_a_number()
        {
            var result = new Money(2, "GBP") * 2;
            Assert.AreEqual(4m, result.Amount, "Amount");
            Assert.AreEqual("GBP", result.Currency, "IsoCode");
        }

        [Test]
        public void can_divide_by_a_number()
        {
            var result = new Money(4, "GBP") / 2;
            Assert.AreEqual(2m, result.Amount, "Amount");
            Assert.AreEqual("GBP", result.Currency, "IsoCode");
        }

        [Test]
        public void can_be_equal_with_same_amount_and_currency()
        {
            Assert.AreEqual(new Money(2, "GBP"), new Money(2, "GBP"));
        }

        [Test]
        public void not_equal_with_same_amount_and_differnet_currency()
        {
            Assert.AreNotEqual(new Money(2, "GBP"), new Money(1, "USD"));
        }

        [Test]
        public void not_equal_with_differnet_amount_and_same_currency()
        {
            Assert.AreNotEqual(new Money(1.00m, "GBP"), new Money(1.01m, "GBP"));
        }

        [Test]
        public void can_be_equal_with_operator_with_same_amount_and_currency()
        {
            Assert.IsTrue(new Money(2, "GBP") == new Money(2, "GBP"));
        }

        [Test]
        public void not_equal_with_operator_with_same_amount_and_differnet_currency()
        {
            Assert.IsFalse(new Money(2, "GBP") == new Money(1, "USD"));
        }

        [Test]
        public void not_equal_with_operator_with_differnet_amount_and_same_currency()
        {
            Assert.IsFalse(new Money(1.00m, "GBP") == new Money(1.01m, "GBP"));
        }

        [Test]
        public void can_be_not_equal_with_operator2_with_same_amount_and_currency()
        {
            Assert.IsFalse(new Money(2, "GBP") != new Money(2, "GBP"));
        }

        [Test]
        public void not_equal_with_operator2_with_same_amount_and_differnet_currency()
        {
            Assert.IsTrue(new Money(2, "GBP") != new Money(1, "USD"));
        }

        [Test]
        public void not_equal_with_operator2_with_differnet_amount_and_same_currency()
        {
            Assert.IsTrue(new Money(1.00m, "GBP") != new Money(1.01m, "GBP"));
        }

        [Test]
        public void compare_returns_zero_when_money_is_equal()
        {
            Assert.AreEqual(0, 10m.GBP().CompareTo(10m.GBP()));
        }

        [Test]
        public void compare_returns_minus_one_when_left_value_is_less_than_right_value()
        {
            Assert.AreEqual(-1, 9m.GBP().CompareTo(10m.GBP()));
        }

        [Test]
        public void compare_returns_plus_one_when_left_value_is_more_than_right_value()
        {
            Assert.AreEqual(1, 11m.GBP().CompareTo(10m.GBP()));
        }

        [Test]
        public void can_negate_positive_money()
        {
            var m = 1m.GBP();
            Assert.AreEqual(-1m, -m.Amount);
        }

        [Test]
        public void can_negate_negative_money()
        {
            var m = -1m.GBP();
            Assert.AreEqual(1m, -m.Amount);
        }


        [Test]
        public void less_than_on_different_currencys_throws_exception()
        {
            Assert.Throws<InvalidOperationException>(() => { var r = 10m.GBP() < 11m.USD(); });
        }

        [Test]
        public void less_than_or_equal_on_different_currencys_throws_exception()
        {
            Assert.Throws<InvalidOperationException>(() => { var r = 10m.GBP() <= 11m.USD(); });
        }

        [Test]
        public void greater_than_on_different_currencys_throws_exception()
        {
            Assert.Throws<InvalidOperationException>(() => { var r = 10m.GBP() > 11m.USD(); });
        }

        [Test]
        public void greater_than_or_equal_on_different_currencys_throws_exception()
        {
            Assert.Throws<InvalidOperationException>(() => { var r = 10m.GBP() >= 11m.USD(); });
        }

        [Test]
        public void less_than()
        {
            Assert.IsTrue(10m.GBP() < 11m.GBP());
        }

        [Test]
        public void not_less_than_when_equal()
        {
            Assert.IsFalse(10m.GBP() < 10m.GBP());
        }

        [Test]
        public void not_less_than_when_more_than()
        {
            Assert.IsFalse(11m.GBP() < 10m.GBP());
        }

        [Test]
        public void more_than()
        {
            Assert.IsTrue(12m.GBP() > 11m.GBP());
        }

        [Test]
        public void not_more_than_when_equal()
        {
            Assert.IsFalse(10m.GBP() > 10m.GBP());
        }

        [Test]
        public void not_more_than_when_less_than()
        {
            Assert.IsFalse(9m.GBP() > 10m.GBP());
        }

        [Test]
        public void less_than_or_equal()
        {
            Assert.IsTrue(10m.GBP() <= 11m.GBP());
        }

        [Test]
        public void less_than_or_equal_when_equal()
        {
            Assert.IsTrue(10m.GBP() <= 10m.GBP());
        }

        [Test]
        public void not_less_than_or_equal_when_more_than()
        {
            Assert.IsFalse(11m.GBP() <= 10m.GBP());
        }

        [Test]
        public void more_than_or_equal()
        {
            Assert.IsTrue(12m.GBP() >= 11m.GBP());
        }

        [Test]
        public void more_than_or_equal_when_equal()
        {
            Assert.IsTrue(10m.GBP() >= 10m.GBP());
        }

        [Test]
        public void not_more_than_or_equal_when_less_than()
        {
            Assert.IsFalse(9m.GBP() >= 10m.GBP());
        }

        [TestCase("GBP", "10.00 GBP")]
        [TestCase("USD", "10.00 USD")]
        [TestCase("EUR", "10.00 EUR")]
        [TestCase("JPY", "10 JPY")]
        [TestCase("JOD", "10.000 JOD")]
        [TestCase("MGA", "10.0 MGA")]
        [TestCase("XBT", "10.00000000 XBT")]
        public void tostring_uses_default_decimal_places_for_currency(string currency, string expected)
        {
            var m = new Money(10m, currency);
            Assert.AreEqual(expected, m.ToString());
        }
    }
}
