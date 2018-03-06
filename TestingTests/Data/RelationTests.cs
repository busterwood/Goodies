using BusterWood.Testing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

namespace BusterWood.Data
{
    public class RelationTests
    {        
        public void can_add_a_row(Test t)
        {
            var rel = new Relation();
            rel.AddColumn<int>("id");
            rel.AddColumn<string>("name");
            var r = rel.AddRow();
            t.Assert(() => 0 == r.Get<int>("id"));
            t.Assert(() => null == r.Get<string>("name"));
        }
        
        public void can_set_the_data_in_the_added_row(Test t)
        {
            var rel = new Relation();
            rel.AddColumn<int>("id");
            rel.AddColumn<string>("name");
            var r = rel.AddRow();
            r.Set(0, 1);
            r.Set(1, "hello");
            t.Assert(() => 1 == r.Get<int>("id"));
            t.Assert(() => "hello" == r.Get<string>("name"));
        }

        public void memory_used_list(Test t)
        {
            if (Tests.Short)
                return;

            foreach (var count in new[] { 1000, 100000, 1000000 })
            {
                var sw = new Stopwatch();
                var before = GC.GetTotalMemory(true);
                sw.Start();
                var list = new List<Order>();
                for (int i = 0; i < count; i++)
                {
                    list.Add(new Order(i));
                }
                sw.Stop();
                Console.WriteLine($"Generic list of {list.Count:N0} items took {sw.Elapsed.TotalMilliseconds:N1}MS to populate");
                sw.Start();
                var after = GC.GetTotalMemory(true);
                Console.WriteLine($"Generic list of {list.Count:N0} items took {sw.Elapsed.TotalMilliseconds:N1}MS to populate & GC, used {after - before:N0} bytes");
            }
        }

        public void memory_used_relation(Test t)
        {
            if (Tests.Short)
                return;

            foreach (var count in new[] { 1000, 100000, 1000000 })
            {
                var sw = new Stopwatch();
                var before = GC.GetTotalMemory(true);
                sw.Start();
                var rel = new Relation();
                rel.AddColumn<int>("Id");
                rel.AddColumn<string>("Name");
                rel.AddColumn<DateTime>("When");
                rel.AddColumn<Guid>("Reference");

                for (int i = 0; i < count; i++)
                {
                    rel.AddRow();
                    rel.SetData(i, 0, i);
                    rel.SetData(i, 1, "Name " + i);
                    rel.SetData(i, 2, DateTime.UtcNow);
                    rel.SetData(i, 3, Guid.NewGuid());
                }
                sw.Stop();
                Console.WriteLine($"Relation of {rel.RowCount:N0} items took {sw.Elapsed.TotalMilliseconds:N1}MS to populate");
                sw.Start();
                var after = GC.GetTotalMemory(true);
                Console.WriteLine($"Relation of {rel.RowCount:N0} items took {sw.Elapsed.TotalMilliseconds:N1}MS to populate, used {after - before:N0} bytes");
            }
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
