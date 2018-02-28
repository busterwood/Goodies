using BusterWood.Testing;
using System;

namespace BusterWood.Monies
{
    public class MoneyTests
    {
        public static void cannot_create_money_with_invalid_isocode(Test t)
        {
            foreach (var ccy in new string[] {"", " ", "   ", "A", "AB", "ABCC" })
            {
                t.AssertThrows<ArgumentOutOfRangeException>(() => new Money(1m, ccy));
            }
        }

        public static void can_add_money_with_same_currency(Test t)
        {
            var result = new Money(1, "GBP") + new Money(1, "GBP");
            if (result.Amount != 2m)
                t.Error($"result.Amount returned {result.Amount}");
            if (result.Currency != "GBP")
                t.Error($"result.Currency returned '{result.Currency}'");
        }

        public static void cannot_add_money_with_different_currency(Test t)
        {
            t.AssertThrows<InvalidOperationException>(() => new Money(1, "GBP") + new Money(1, "EUR"));
        }

        public static void can_subtract_money_with_same_currency(Test t)
        {
            var result = new Money(2, "GBP") - new Money(1, "GBP");
            t.Assert(() => 1m.Equals(result.Amount));
            t.Assert(() => "GBP".Equals(result.Currency));
        }

        public static void cannot_subtract_money_with_different_currency(Test t)
        {
            t.AssertThrows<InvalidOperationException>(() => new Money(1, "GBP") - new Money(1, "EUR"));
        }

        public static void can_multiply_by_a_number(Test t)
        {
            var result = new Money(2, "GBP") * 2;
            t.Assert(() => 4m.Equals(result.Amount));
            t.Assert(() => "GBP".Equals(result.Currency));
        }

        public static void can_divide_by_a_number(Test t)
        {
            var result = new Money(4, "GBP") / 2;
            t.Assert(() => 2m.Equals(result.Amount));
            t.Assert(() => "GBP".Equals(result.Currency));
        }

        public static void can_be_equal_with_same_amount_and_currency(Test t)
        {
            if (!new Money(2, "GBP").Equals(new Money(2, "GBP")))
                t.Error("not equals");
        }

        public static void not_equal_with_same_amount_and_differnet_currency(Test t)
        {
            if (new Money(2, "GBP").Equals(new Money(2, "USD")))
                t.Error("currencies differ");
        }

        public static void not_equal_with_differnet_amount_and_same_currency(Test t)
        {
            if (new Money(1.00m, "GBP").Equals(new Money(1.01m, "GBP")))
                t.Error("amounts differ");
        }

        public static void can_be_equal_with_operator_with_same_amount_and_currency(Test t)
        {
            if (new Money(2, "GBP") != new Money(2, "GBP"))
                t.Error("same");
        }

        public static void not_equal_with_operator_with_same_amount_and_differnet_currency(Test t)
        {
            if (new Money(2, "GBP") == new Money(1, "USD"))
                t.Error("currencies differ");
        }

        public static void not_equal_with_operator_with_differnet_amount_and_same_currency(Test t)
        {
            t.Assert(() => new Money(1.00m, "GBP") != new Money(1.01m, "GBP"));
        }

        public static void can_be_not_equal_with_operator2_with_same_amount_and_currency(Test t)
        {
            t.Assert(() => new Money(2, "GBP") == new Money(2, "GBP"));
        }

        public static void not_equal_with_operator2_with_same_amount_and_differnet_currency(Test t)
        {
            t.Assert(() => new Money(2, "GBP") != new Money(1, "USD"));
        }

        public static void not_equal_with_operator2_with_differnet_amount_and_same_currency(Test t)
        {
            t.Assert(() => new Money(1.00m, "GBP") != new Money(1.01m, "GBP"));
        }

        public static void compare_returns_zero_when_money_is_equal(Test t)
        {
            t.Assert(() => 10m.GBP().CompareTo(10m.GBP()) == 0);
        }

        public static void compare_returns_minus_one_when_left_value_is_less_than_right_value(Test t)
        {
            t.Assert(() => 9m.GBP().CompareTo(10m.GBP()) < 0);
        }

        public static void compare_returns_plus_one_when_left_value_is_more_than_right_value(Test t)
        {
            t.Assert(() => 11m.GBP().CompareTo(10m.GBP()) > 0);
        }

        public static void can_negate_positive_money(Test t)
        {
            var m = 1m.GBP();
            t.Assert(() => -1m == -m.Amount);
        }

        public static void can_negate_negative_money(Test t)
        {
            var m = -1m.GBP();
            t.Assert(() => 1m == -m.Amount);
        }

        public static void less_than_on_different_currencys_throws_exception(Test t)
        {
            t.AssertThrows<InvalidOperationException>(() => 10m.GBP() < 11m.USD());
        }

        public static void less_than_or_equal_on_different_currencys_throws_exception(Test t)
        {
            t.AssertThrows<InvalidOperationException>(() => 10m.GBP() <= 11m.USD());
        }

        public static void greater_than_on_different_currencys_throws_exception(Test t)
        {
            t.AssertThrows<InvalidOperationException>(() => 10m.GBP() > 11m.USD());
        }

        public static void greater_than_or_equal_on_different_currencys_throws_exception(Test t)
        {
            t.AssertThrows<InvalidOperationException>(() => 10m.GBP() >= 11m.USD());
        }

        public static void less_than(Test t)
        {
            t.Assert(() => 10m.GBP() < 11m.GBP());
        }

        public static void not_less_than_when_equal(Test t)
        {
            t.AssertNot(() => 10m.GBP() < 10m.GBP());
        }

        public static void not_less_than_when_more_than(Test t)
        {
            t.AssertNot(() => 11m.GBP() < 10m.GBP());
        }

        public static void more_than(Test t)
        {
            t.Assert(() => 12m.GBP() > 11m.GBP());
        }

        public static void not_more_than_when_equal(Test t)
        {
            t.AssertNot(() => 10m.GBP() > 10m.GBP());
        }

        public static void not_more_than_when_less_than(Test t)
        {
            t.AssertNot(() => 9m.GBP() > 10m.GBP());
        }

        public static void less_than_or_equal(Test t)
        {
            t.Assert(() => 10m.GBP() <= 11m.GBP());
        }

        public static void less_than_or_equal_when_equal(Test t)
        {
            t.Assert(() => 10m.GBP() <= 10m.GBP());
        }

        public static void not_less_than_or_equal_when_more_than(Test t)
        {
            t.AssertNot(() => 11m.GBP() <= 10m.GBP());
        }

        public static void more_than_or_equal(Test t)
        {
            t.Assert(() => 12m.GBP() >= 11m.GBP());
        }

        public static void more_than_or_equal_when_equal(Test t)
        {
            t.Assert(() => 10m.GBP() >= 10m.GBP());
        }

        public static void not_more_than_or_equal_when_less_than(Test t)
        {
            t.AssertNot(() => 9m.GBP() >= 10m.GBP());
        }

        public static void tostring_uses_default_decimal_places_for_currency(Test t)
        {
            var cases = new[]
            {
                new { Ccy="GBP", Expected = "10.00 GBP" },
                new { Ccy="USD", Expected = "10.00 USD" },
                new { Ccy="EUR", Expected = "10.00 EUR" },
                new { Ccy="JPY", Expected = "10 JPY" },
                new { Ccy="JOD", Expected = "10.000 JOD" },
                new { Ccy="MGA", Expected = "10.0 MGA" },
                new { Ccy="XBT", Expected = "10.00000000 XBT" },
            };
            foreach (var c in cases)
            {
                var m = new Money(10m, c.Ccy);
                t.Assert(() => c.Expected == m.ToString());
            }
        }
    }
}
