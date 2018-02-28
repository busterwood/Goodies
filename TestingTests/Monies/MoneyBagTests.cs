using BusterWood.Testing;
using System.Collections.Generic;

namespace BusterWood.Monies
{
    public class MoneyBagTests
    {        
        public static void is_initially_empty(Test t)
        {
            t.Assert(() => new MoneyBag().IsEmpty);
        }

        public void empty_bag_contain_zero_for_any_currency(Test t)
        {
            foreach (var currency in new[] { "GBP", "USD" })
            {
                var bag = new MoneyBag();
                t.Assert(() => new Money(0m, currency) == bag[currency]);
            }
        }
        
        public static void can_add_single_currency(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            t.Assert(() => 10m.GBP() == bag["GBP"]);
        }
        
        public static void can_add_single_currency_multiple_times(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(10m.GBP());
            t.Assert(() => 20m.GBP() == bag["GBP"]);
        }
        
        public static void can_add_and_substract(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Subtract(9m.GBP());
            t.Assert(() => 1m.GBP() == bag["GBP"]);
        }
        
        public static void bag_with_money_added_is_not_empty(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            t.AssertNot(() => bag.IsEmpty);
        }
        
        public static void can_add_different_currencies(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            t.Assert(() => 10m.GBP() == bag["GBP"]);
            t.Assert(() => 11m.USD() == bag["USD"]);
        }
        
        public static void can_enumerate_bag_contents(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            var list = new List<Money>(bag);
            t.Assert(() => list.Count == 2);
            t.Assert(() => list.Contains(10m.GBP()));
            t.Assert(() => list.Contains(11m.USD()));
        }
                
        public static void can_remove_money(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            var gbpRemoved = bag.Remove("GBP");
            t.Assert(() => 10m.GBP() == gbpRemoved);
            t.Assert(() => 0m.GBP() == bag["GBP"]);
        }
        
        public static void removing_money_does_not_affect_other_currencies(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            bag.Remove("GBP");
            t.Assert(() => 11m.USD() == bag["USD"]);
        }
        
        public static void to_string_formats_using_decimal_places(Test t)
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            t.Assert(() => "[ 10.00 GBP; 11.00 USD ]" == bag.ToString());
        }
    }
}
