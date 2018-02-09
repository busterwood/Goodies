using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Data
{
    [TestFixture]
    public class RelationTests
    {
        [Test]
        public void can_add_a_row()
        {
            var rel = new Relation();
            rel.AddColumn<int>("id");
            rel.AddColumn<string>("name");
            var r = rel.AddRow();
            Assert.AreEqual(0, r.Get<int>("id"));
            Assert.AreEqual(null, r.Get<string>("name"));
        }

        [Test]
        public void can_set_the_data_in_the_added_row()
        {
            var rel = new Relation();
            rel.AddColumn<int>("id");
            rel.AddColumn<string>("name");
            var r = rel.AddRow();
            r.Set(0, 1);
            r.Set(1, "hello");
            Assert.AreEqual(1, r.Get<int>("id"));
            Assert.AreEqual("hello", r.Get<string>("name"));
        }

        [TestCase(100)]
        [TestCase(100000)]
        public void memory_used_list(int count)
        {
            var before = GC.GetTotalMemory(true);
            var list = new List<Order>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Order(i));
            }
            var after = GC.GetTotalMemory(true);
            Console.WriteLine($"Genric list of {list.Count:N0} items used {after - before:N0} bytes");
        }

        [TestCase(100)]
        [TestCase(100000)]
        public void memory_used_relation(int count)
        {
            var before = GC.GetTotalMemory(true);
            var rel = new Relation();
            rel.AddColumn<int>("Id");
            rel.AddColumn<string>("Name");
            rel.AddColumn<DateTime>("When");
            rel.AddColumn<Guid>("Reference");

            for (int i = 0; i < count; i++)
            {
                var o = new Order(i);
                var r = rel.AddRow();
                r.Set(0, o.Id);
                r.Set(1, o.Name);
                r.Set(2, o.When);
                r.Set(3, o.Reference);
            }
            var after = GC.GetTotalMemory(true);
            Console.WriteLine($"Relation of {rel.RowCount:N0} items used {after - before:N0} bytes");
        }

        class Order
        {
            public int Id;
            public string Name;
            public DateTime When;
            public Guid Reference;

            public Order(int i)
            {
                Id = i;
                Name = "Name " + i;
                When = DateTime.UtcNow;
                Reference = Guid.NewGuid();
            }
        }
    }
}
