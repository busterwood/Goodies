using BusterWood.Monies;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class MoneyBagTests
    {
        [Test]
        public void is_initially_empty()
        {
            Assert.IsTrue(new MoneyBag().IsEmpty);
        }

        [TestCase("GBP")]
        [TestCase("USD")]
        public void empty_bag_contain_zero_for_any_currency(string currency)
        {
            var bag = new MoneyBag();
            Assert.AreEqual(new Money(0m, currency), bag[currency]);
        }

        [Test]
        public void can_add_single_currency()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            Assert.AreEqual(10m.GBP(), bag["GBP"]);
        }

        [Test]
        public void can_add_single_currency_multiple_times()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(10m.GBP());
            Assert.AreEqual(20m.GBP(), bag["GBP"]);
        }

        [Test]
        public void can_add_and_substract()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Subtract(9m.GBP());
            Assert.AreEqual(1m.GBP(), bag["GBP"]);
        }

        [Test]
        public void bag_with_money_added_is_not_empty()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            Assert.IsFalse(bag.IsEmpty);
        }

        [Test]
        public void can_add_different_currencies()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            Assert.AreEqual(10m.GBP(), bag["GBP"]);
            Assert.AreEqual(11m.USD(), bag["USD"]);
        }

        [Test]
        public void can_enumerate_bag_contents()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            var list = new List<Money>(bag);
            Assert.AreEqual(2, list.Count, "Count");
            Assert.IsTrue(list.Contains(10m.GBP()));
            Assert.IsTrue(list.Contains(11m.USD()));
        }
        
        [Test]
        public void can_remove_money()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            var gbpRemoved = bag.Remove("GBP");
            Assert.AreEqual(10m.GBP(), gbpRemoved);
            Assert.AreEqual(0m.GBP(), bag["GBP"]);
        }

        [Test]
        public void removing_money_does_not_affect_other_currencies()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            bag.Remove("GBP");
            Assert.AreEqual(11m.USD(), bag["USD"]);
        }

        [Test]
        public void to_string_formats_using_decimal_places()
        {
            var bag = new MoneyBag();
            bag.Add(10m.GBP());
            bag.Add(11m.USD());
            Assert.AreEqual("[ 10.00 GBP; 11.00 USD ]", bag.ToString());
        }
    }
}
