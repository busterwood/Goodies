using BusterWood.Collections;
using BusterWood.Testing;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Linq
{
    public class EnumerableTests
    {
        public void can_setrelationship_with_one_lookup(Test t)
        {
            var orders = new Order[] { new Order { OrderId = 1 }, new Order { OrderId = 2 } };
            var items = new OrderItem[] { new OrderItem { OrderId = 1 }, new OrderItem { OrderId = 1 }, new OrderItem { OrderId = 2 } };

            orders.SetRelationship(o => o.OrderId, items.ToHashLookup(i => i.OrderId), (order, oi) => order.Items = oi);
            t.Assert(() => orders[0].Items.Contains(items[0]));
            t.Assert(() => orders[0].Items.Contains(items[1]));
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
