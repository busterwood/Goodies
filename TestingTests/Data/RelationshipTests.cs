using BusterWood.Data;
using BusterWood.Testing;
using System.Collections.Generic;
using System.Linq;

namespace TestingTests.Data
{
    public class RelationshipTests
    {
        public static void can_relate_master_detail(Test t)
        {
            List<Basket> baskets = new List<Basket> { new Basket { Id = 1 }, new Basket { Id = 2 } };
            List<Order> orders = new List<Order> { new Order { Id = 10, BasketId = 1 }, new Order { Id = 11, BasketId = 1 }, new Order { Id = 15, BasketId = 2 } };

            var result = new Relationships<Basket, Order>(baskets, orders)
                .HasMany(b => b.Id, o => o.BasketId, (b, ords) => { b.Orders = ords; })
                .Relate();

            t.Assert(() => result.First == baskets);
            t.Assert(() => result.Second == orders);

            t.Assert(() => baskets[0].Orders.Contains(orders[0]));
            t.Assert(() => baskets[0].Orders.Contains(orders[1]));
            t.Assert(() => baskets[0].Orders.Count == 2);

            t.Assert(() => baskets[1].Orders.Contains(orders[2]));
            t.Assert(() => baskets[1].Orders.Count == 1);
        }

        public static void can_relate_three_levels_of_master_detail(Test t)
        {
            List<Basket> baskets = new List<Basket> { new Basket { Id = 1 }, new Basket { Id = 2 } };
            List<Order> orders = new List<Order> { new Order { Id = 10,  BasketId = 1 }, new Order { Id = 11,  BasketId = 1 }, new Order { Id = 15,  BasketId = 2 } };
            List<Allocation> allocations = new List<Allocation> { new Allocation { Id = 20, OrderId = 10 }, new Allocation { Id = 21, OrderId = 10 }, new Allocation { Id = 22, OrderId = 15 } };

            var result = new Relationships<Basket, Order, Allocation>(baskets, orders, allocations)
                .HasMany(b => b.Id, o => o.BasketId, (b, ords) => { b.Orders = ords; })
                .HasMany(o => o.Id, a => a.OrderId, (o, allocs) => { o.Allocations = allocs; })
                .Relate();

            t.Assert(() => result.First == baskets);

            t.Assert(() => baskets[0].Orders.Contains(orders[0]));
            t.Assert(() => baskets[0].Orders.Contains(orders[1]));
            t.Assert(() => baskets[0].Orders.Count == 2);

            t.Assert(() => baskets[1].Orders.Contains(orders[2]));
            t.Assert(() => baskets[1].Orders.Count == 1);

            t.Assert(() => orders[0].Allocations.Contains(allocations[0]));
            t.Assert(() => orders[0].Allocations.Contains(allocations[1]));
            t.Assert(() => orders[0].Allocations.Count == 2);

            t.Assert(() => orders[1].Allocations.Count == 0);

            t.Assert(() => orders[2].Allocations.Contains(allocations[2]));
            t.Assert(() => orders[2].Allocations.Count == 1);
        }

        public static void can_relate_four_classes(Test t)
        {
            var baskets = new [] { new Basket { Id = 1 }, new Basket { Id = 2 } };
            var orders = new [] { new Order { Id = 10, BasketId = 1 }, new Order { Id = 11, BasketId = 1 }, new Order { Id = 15, BasketId = 2 } };
            var allocations = new [] { new Allocation { Id = 20, OrderId = 10 }, new Allocation { Id = 21, OrderId = 10 }, new Allocation { Id = 22, OrderId = 15 } };
            var tickets = new[] { new Ticket { Id = 30, OrderId = 10, BasketId = 1 }, new Ticket { Id = 31, OrderId = 11, BasketId = 1 }, new Ticket { Id = 32, OrderId = 15, BasketId = 2 } };

            var result = new Relationships<Basket, Order, Allocation, Ticket>(baskets, orders, allocations, tickets)
                .HasMany(b => b.Id, o => o.BasketId, (b, ords) => { b.Orders = ords; })
                .HasMany(o => o.Id, a => a.OrderId, (o, allocs) => { o.Allocations = allocs; })
                .HasMany(o => o.Id, tkt => tkt.OrderId, (o, tkts) => { o.Tickets = tkts; })
                .Relate();

            t.Assert(() => result.First == baskets);

            t.Assert(() => baskets[0].Orders.Contains(orders[0]));
            t.Assert(() => baskets[0].Orders.Contains(orders[1]));
            t.Assert(() => baskets[0].Orders.Count == 2);

            t.Assert(() => baskets[1].Orders.Contains(orders[2]));
            t.Assert(() => baskets[1].Orders.Count == 1);

            t.Assert(() => orders[0].Allocations.Contains(allocations[0]));
            t.Assert(() => orders[0].Allocations.Contains(allocations[1]));
            t.Assert(() => orders[0].Allocations.Count == 2);

            t.Assert(() => orders[1].Allocations.Count == 0);

            t.Assert(() => orders[2].Allocations.Contains(allocations[2]));
            t.Assert(() => orders[2].Allocations.Count == 1);

            t.Assert(() => orders[0].Tickets.Contains(tickets[0]));
            t.Assert(() => orders[0].Tickets.Count == 1);

            t.Assert(() => orders[1].Tickets.Contains(tickets[1]));
            t.Assert(() => orders[1].Tickets.Count == 1);

            t.Assert(() => orders[2].Tickets.Contains(tickets[2]));
            t.Assert(() => orders[2].Tickets.Count == 1);
        }

        class Basket
        {
            public int Id { get; set; }
            public IReadOnlyList<Order> Orders { get; set; }
        }

        class Order
        {
            public int Id;
            public int BasketId;
            public IReadOnlyList<Allocation> Allocations { get; set; }
            public IReadOnlyList<Ticket> Tickets { get; set; }
        }

        class Allocation
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
        }

        class Ticket
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int BasketId { get; set; }
        }

        class DestinationDesk
        {
            public int Id { get; set; }
            public int TicketId { get; set; }
        }

    }
}
