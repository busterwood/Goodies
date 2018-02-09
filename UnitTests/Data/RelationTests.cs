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
    }
}
