using NUnit.Framework;

namespace BusterWood.Mapping
{
    [TestFixture]
    public class UsingSource
    {
        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_property(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var result = Mapping.CreateFromSource(new Thing[] { col }, new TestPropertyId { OrderId = 1 }.GetType());
                var mapping = result.Mapped;
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.From);
                Assert.IsNotNull(map.To);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_field(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var result = Mapping.CreateFromSource(new Thing[] { col }, new TestFieldId { OrderId = 1 }.GetType());
                var mapping = result.Mapped;
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.From);
                Assert.IsNotNull(map.To);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_property_without_id_suffix(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var result = Mapping.CreateFromSource(new Thing[] { col }, new TestProperty { Order = 1 }.GetType());
                var mapping = result.Mapped;
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.From);
                Assert.IsNotNull(map.To);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_field_without_id_suffix(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var result = Mapping.CreateFromSource(new Thing[] { col }, new TestField { Order = 1 }.GetType());
                var mapping = result.Mapped;
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.From);
                Assert.IsNotNull(map.To);
            }
        }

        [Test]
        public void trace_logs_things_not_mapped_including_prefix()
        {
            var ob = new ConsoleObserver();
            using (Mapping.Trace.Subscribe(ob))
            {
                Mapping.CreateFromSource(typeof(SingleProp<string>), typeof(SingleProp<int>), "Things");
                Assert.AreEqual(1, ob.Values.Count, "count");
                Assert.IsTrue(ob.Values[0].StartsWith("Things: "), ob.Values[0]);
            }
        }
    }

    [TestFixture]
    public class UsingDestination
    {
        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_property(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var result = Mapping.CreateFromDestination(new TestPropertyId { OrderId = 1 }.GetType(), new Thing[] { col });
                var mapping = result.Mapped;
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.To);
                Assert.IsNotNull(map.From);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        public void maps_column_to_field(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var result = Mapping.CreateFromDestination(new TestFieldId { OrderId = 1 }.GetType(), new Thing[] { col });
                var mapping = result.Mapped;
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.To);
                Assert.IsNotNull(map.From);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_property_without_id_suffix(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var result = Mapping.CreateFromDestination(new TestProperty { Order = 1 }.GetType(), new Thing[] { col });
                var mapping = result.Mapped;
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.To);
                Assert.IsNotNull(map.From);
            }
        }

        [TestCase("OrderId")]
        [TestCase("ORDERID")]
        [TestCase("ORDER_ID")]
        [TestCase("Order_Id")]
        [TestCase("Order")]
        [TestCase("ORDER")]
        public void maps_column_to_field_without_id_suffix(string colName)
        {
            using (Mapping.Trace.Subscribe(new ConsoleObserver()))
            {
                var col = new Column(0, colName, typeof(int));
                var result = Mapping.CreateFromDestination(new TestField { Order = 1 }.GetType(), new Thing[] { col });
                var mapping = result.Mapped;
                Assert.AreEqual(1, mapping.Count, "count");
                var map = mapping[0];
                Assert.AreEqual(col, map.To);
                Assert.IsNotNull(map.From);
            }
        }

        [Test]
        public void trace_logs_things_not_mapped_including_prefix()
        {
            var ob = new ConsoleObserver();
            using (Mapping.Trace.Subscribe(ob))
            {
                Mapping.CreateFromDestination(typeof(SingleProp<string>), typeof(SingleProp<int>), "Things");
                Assert.AreEqual(1, ob.Values.Count, "count");
                Assert.IsTrue(ob.Values[0].StartsWith("Things: "));
            }
        }
    }

    struct TestStruct<T>
    {
        public T Value { get; set; }
    }

    class TestSingle<T>
    {
        public T Value { get; set; }
    }

    class TestPropertyId
    {
        public int OrderId { get; set; }
    }

    class TestFieldId
    {
        public int OrderId;
    }

    class TestProperty
    {
        public int Order { get; set; }
    }

    class TestField
    {
        public int Order;
    }

    class NullableLong
    {
        public long? Id { get; set; }
    }

    class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class Booking
    {
        public int Id { get; set; }
        public State State { get; set; }
    }

    enum State
    {
        First = 1,
        Second,
    }

}
