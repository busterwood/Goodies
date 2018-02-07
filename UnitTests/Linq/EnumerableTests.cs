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
            //orders.SetRelationship(items, (o, oi) => o.OrderId == oi.OrderId); // Expression version - needs code generation
        }

        class Order
        {
            public int OrderId { get; set; }
            public IReadOnlyList<OrderItem> Items { get; set; }
        }

        class OrderItem
        {
            public int OrderId { get; set; }
        }
    }
}
