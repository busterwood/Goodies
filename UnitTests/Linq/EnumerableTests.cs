using BusterWood.Collections;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Linq
{
    [TestFixture]
    public class EnumerableTests
    {
        [Test]
        public void can_mutate_withone_lookup()
        {
            var orders = new Order[] { new Order { OrderId = 1 }, new Order { OrderId = 2 } };
            var items = new OrderItem[] { new OrderItem { OrderId = 1 }, new OrderItem { OrderId = 1 }, new OrderItem { OrderId = 2 } };

            orders.SetRelationship(o => o.OrderId, items.ToHashLookup(i => i.OrderId), (order, oi) => order.Items = oi);

            Assert.AreEqual(2, orders.ElementAt(0).Items.Count);
            Assert.AreEqual(1, orders.ElementAt(1).Items.Count);
        }

        void non_test(IEnumerable<Order> master, IEnumerable<OrderItem> details)
        {
            var lookup = details.ToHashLookup(GetKey);
            var e = master.GetEnumerator();
            for (;;)
            {
                if (!e.MoveNext())
                    return;
                Order order = e.Current;
                order.Items = lookup[order.OrderId];
            }
        }

        [Test]
        public void can_set_relationship_via_expression()
        {
            var orders = new Order[] { new Order { OrderId = 1 }, new Order { OrderId = 2 } };
            var items = new OrderItem[] { new OrderItem { OrderId = 1 }, new OrderItem { OrderId = 1 }, new OrderItem { OrderId = 2 } };
            orders.SetRelationship(items, (o, oi) => o.OrderId == oi.OrderId); // Expression version - needs code generation

            Assert.AreEqual(2, orders.ElementAt(0).Items.Count);
            Assert.AreEqual(1, orders.ElementAt(1).Items.Count);
        }

        static int GetKey(OrderItem item)
        {
            return item.OrderId;
        }

        public class Order
        {
            public int OrderId { get; set; }
            public IReadOnlyList<OrderItem> Items { get; set; }
        }

        public class OrderItem
        {
            public int OrderId { get; set; }
        }
    }



}
