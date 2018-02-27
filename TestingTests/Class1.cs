using BusterWood.Monies;
using BusterWood.Testing;
using System;

namespace TestingTests
{
    public class Program   
    {
        public static int Main()
        {
            //Test.Verbose = true;
            return Tests.Run(typeof(MoneyTests));
        }
    }

    public class MoneyTests
    {
        public void test_cannot_create_money_with_invalid_isocode(Test t)
        {
            foreach (var ccy in new string[] {"", " ", "   ", "A", "AB", "ABCC" })
            {
                try
                {
                    new Money(1m, ccy);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                catch (Exception e)
                {
                    t.Error($"'{ccy}' threw {e}");
                }

            }
        }

        public void test_can_add_money_with_same_currency(Test t)
        {
            var result = new Money(1, "GBP") + new Money(1, "GBP");
            if (result.Amount != 2m)
                t.Error($"result.Amount returned {result.Amount}");
            if (result.Currency != "GBP")
                t.Error($"result.Currency returned '{result.Currency}'");
        }

        //[Test]
        //public void cannot_add_money_with_different_currency()
        //{
        //    Assert.Throws<InvalidOperationException>(() => { var result = new Money(1, "GBP") + new Money(1, "EUR"); });
        //}

        //[Test]
        //public void can_subtract_money_with_same_currency()
        //{
        //    var result = new Money(2, "GBP") - new Money(1, "GBP");
        //    Assert.AreEqual(1m, result.Amount, "Amount");
        //    Assert.AreEqual("GBP", result.Currency, "IsoCode");
        //}

        //[Test]
        //public void cannot_subtract_money_with_different_currency()
        //{
        //    Assert.Throws<InvalidOperationException>(() => { var result = new Money(1, "GBP") - new Money(1, "EUR"); });
        //}

        //[Test]
        //public void can_multiply_by_a_number()
        //{
        //    var result = new Money(2, "GBP") * 2;
        //    Assert.AreEqual(4m, result.Amount, "Amount");
        //    Assert.AreEqual("GBP", result.Currency, "IsoCode");
        //}

        //[Test]
        //public void can_divide_by_a_number()
        //{
        //    var result = new Money(4, "GBP") / 2;
        //    Assert.AreEqual(2m, result.Amount, "Amount");
        //    Assert.AreEqual("GBP", result.Currency, "IsoCode");
        //}

        public void test_can_be_equal_with_same_amount_and_currency(Test t)
        {
            if (!new Money(2, "GBP").Equals(new Money(2, "GBP")))
                t.Error("not equals");
        }

        public void test_not_equal_with_same_amount_and_differnet_currency(Test t)
        {
            if (new Money(2, "GBP").Equals(new Money(2, "USD")))
                t.Error("currencies differ");
        }

        public void test_not_equal_with_differnet_amount_and_same_currency(Test t)
        {
            if (new Money(1.00m, "GBP").Equals(new Money(1.01m, "GBP")))
                t.Error("amounts differ");
        }

        public void test_can_be_equal_with_operator_with_same_amount_and_currency(Test t)
        {
            if (new Money(2, "GBP") != new Money(2, "GBP"))
                t.Error("same");
        }

        public void test_not_equal_with_operator_with_same_amount_and_differnet_currency(Test t)
        {
            if (new Money(2, "GBP") == new Money(1, "USD"))
                t.Error("currencies differ");
        }

        public void test_not_equal_with_operator_with_differnet_amount_and_same_currency(Test t)
        {
            t.Assert(() => new Money(1.00m, "GBP") != new Money(1.01m, "GBP"));
        }

        public void test_can_be_not_equal_with_operator2_with_same_amount_and_currency(Test t)
        {
            t.Assert(() => new Money(2, "GBP") == new Money(2, "GBP"));
        }

        public void test_not_equal_with_operator2_with_same_amount_and_differnet_currency(Test t)
        {
            t.Assert(() => new Money(2, "GBP") != new Money(1, "USD"));
        }

        public void test_not_equal_with_operator2_with_differnet_amount_and_same_currency(Test t)
        {
            t.Assert(() => new Money(1.00m, "GBP") != new Money(1.01m, "GBP"));
        }

        public void test_compare_returns_zero_when_money_is_equal(Test t)
        {
            t.Assert(() => 10m.GBP().CompareTo(10m.GBP()) == 0);
        }

        public void test_compare_returns_minus_one_when_left_value_is_less_than_right_value(Test t)
        {
            t.Assert(() => 9m.GBP().CompareTo(10m.GBP()) < 0);
        }

        public void test_compare_returns_plus_one_when_left_value_is_more_than_right_value(Test t)
        {
            t.Assert(() => 11m.GBP().CompareTo(10m.GBP()) > 0);
        }

        public void test_can_negate_positive_money(Test t)
        {
            var m = 1m.GBP();
            t.Assert(() => -1m == -m.Amount);
        }

        public void test_can_negate_negative_money(Test t)
        {
            var m = -1m.GBP();
            t.Assert(() => 1m == -m.Amount);
        }

        //[Test]
        //public void less_than_on_different_currencys_throws_exception()
        //{
        //    Assert.Throws<InvalidOperationException>(() => { var r = 10m.GBP() < 11m.USD(); });
        //}

        //[Test]
        //public void less_than_or_equal_on_different_currencys_throws_exception()
        //{
        //    Assert.Throws<InvalidOperationException>(() => { var r = 10m.GBP() <= 11m.USD(); });
        //}

        //[Test]
        //public void greater_than_on_different_currencys_throws_exception()
        //{
        //    Assert.Throws<InvalidOperationException>(() => { var r = 10m.GBP() > 11m.USD(); });
        //}

        //[Test]
        //public void greater_than_or_equal_on_different_currencys_throws_exception()
        //{
        //    Assert.Throws<InvalidOperationException>(() => { var r = 10m.GBP() >= 11m.USD(); });
        //}

        public void test_less_than(Test t)
        {
            t.Assert(() => 10m.GBP() < 11m.GBP());
        }

        public void test_not_less_than_when_equal(Test t)
        {
            t.AssertFalse(() => 10m.GBP() < 10m.GBP());
        }

        public void test_not_less_than_when_more_than(Test t)
        {
            t.AssertFalse(() => 11m.GBP() < 10m.GBP());
        }

        public void test_more_than(Test t)
        {
            t.Assert(() => 12m.GBP() > 11m.GBP());
        }

        //[Test]
        //public void not_more_than_when_equal()
        //{
        //    Assert.IsFalse(10m.GBP() > 10m.GBP());
        //}

        //[Test]
        //public void not_more_than_when_less_than()
        //{
        //    Assert.IsFalse(9m.GBP() > 10m.GBP());
        //}

        //[Test]
        //public void less_than_or_equal()
        //{
        //    Assert.IsTrue(10m.GBP() <= 11m.GBP());
        //}

        //[Test]
        //public void less_than_or_equal_when_equal()
        //{
        //    Assert.IsTrue(10m.GBP() <= 10m.GBP());
        //}

        //[Test]
        //public void not_less_than_or_equal_when_more_than()
        //{
        //    Assert.IsFalse(11m.GBP() <= 10m.GBP());
        //}

        //[Test]
        //public void more_than_or_equal()
        //{
        //    Assert.IsTrue(12m.GBP() >= 11m.GBP());
        //}

        //[Test]
        //public void more_than_or_equal_when_equal()
        //{
        //    Assert.IsTrue(10m.GBP() >= 10m.GBP());
        //}

        //[Test]
        //public void not_more_than_or_equal_when_less_than()
        //{
        //    Assert.IsFalse(9m.GBP() >= 10m.GBP());
        //}

        //[TestCase("GBP", "10.00 GBP")]
        //[TestCase("USD", "10.00 USD")]
        //[TestCase("EUR", "10.00 EUR")]
        //[TestCase("JPY", "10 JPY")]
        //[TestCase("JOD", "10.000 JOD")]
        //[TestCase("MGA", "10.0 MGA")]
        //[TestCase("XBT", "10.00000000 XBT")]
        //public void tostring_uses_default_decimal_places_for_currency(string currency, string expected)
        //{
        //    var m = new Money(10m, currency);
        //    Assert.AreEqual(expected, m.ToString());
        //}
    }
}
