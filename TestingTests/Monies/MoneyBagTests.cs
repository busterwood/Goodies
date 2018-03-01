using BusterWood.Testing;
using System.Collections.Generic;

namespace BusterWood.Monies
{
    public class MoneyBagTests
    {
        public static void is_initially_empty(Test t)
        {
            if (!(new MoneyBag().IsEmpty))
                t.Error("new MoneyBag().IsEmpty)");
        }

        public void empty_bag_contain_zero_for_any_currency(Test t)
        {
            foreach (var currency in new[] { "GBP", "USD" })
            {
                var bag = new MoneyBag();
                if (!(new Money(0m, currency) == bag[currency]))
                    t.Error("new Money(0m, currency) == bag[currency])");
            }
        }

        public static void can_add_single_currency(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            if (!(10m.GBP() == bag["GBP"]))
                t.Error("10m.GBP() == bag[GBP])");
        }

        public static void can_add_single_currency_multiple_times(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(10m.GBP());
            if (!(20m.GBP() == bag["GBP"]))
                t.Error("20m.GBP() == bag[GBP])");
        }

        public static void can_add_and_substract(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Subtract(9m.GBP());
            if (!(1m.GBP() == bag["GBP"]))
                t.Error("1m.GBP() == bag[GBP])");
        }

        public static void bag_with_money_added_is_not_empty(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            if (bag.IsEmpty)
                t.Error("bag.IsEmpty");
        }

        public static void can_add_different_currencies(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            if (!(10m.GBP() == bag["GBP"]))
                t.Error("10m.GBP() == bag[GBP])");
            if (!(11m.USD() == bag["USD"]))
                t.Error("11m.USD() == bag[USD])");
        }

        public static void can_enumerate_bag_contents(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            var list = new List<Money>(bag);
            if (!(list.Count == 2))
                t.Error("list.Count == 2)");
            if (!(list.Contains(10m.GBP())))
                t.Error("list.Contains(10m.GBP())");
            if (!(list.Contains(11m.USD())))
                t.Error("list.Contains(11m.USD())");
        }

        public static void can_remove_money(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            var gbpRemoved = bag.Remove("GBP");
            if (!(10m.GBP() == gbpRemoved))
                t.Error("10m.GBP() == gbpRemoved)");
            if (!(0m.GBP() == bag["GBP"]))
                t.Error("0m.GBP() == bag[GBP])");
        }

        public static void removing_money_does_not_affect_other_currencies(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            bag.Remove("GBP");
            if (!(11m.USD() == bag["USD"]))
                t.Error("11m.USD() == bag[USD])");
        }

        public static void to_string_formats_using_decimal_places(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            if (!("[ 10.00 GBP; 11.00 USD ]" == bag.ToString()))
                t.Error("[10.00 GBP; 11.00 USD ] == bag.ToString())");
        }
    }
}
