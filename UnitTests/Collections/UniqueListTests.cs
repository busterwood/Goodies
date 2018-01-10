using BusterWood.Collections;
using NUnit.Framework;
using System;
using System.Linq;

namespace UnitTests
{
    [TestFixture]
    public class UniqueListTests
    {
        [Test]
        public void can_add_unique_value()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(1), "set.Add(1)");
            Assert.AreEqual(1, set.Count, "set.Count");
        }

        [Test]
        public void adding_an_nullable_struct_with_null_value_returns_false()
        {
            var set = new UniqueList<int>();
            int? val = null;
            Assert.AreEqual(false, set.Add(val), "set.Add(null)");
            Assert.AreEqual(0, set.Count, "set.Count");
        }

        [Test]
        public void adding_an_nullable_struct_with_value_returns_false_when_item_already_in_set()
        {
            var set = new UniqueList<int>() { 1 };
            int? val = 1;
            Assert.AreEqual(false, set.Add(val), "set.Add(null)");
            Assert.AreEqual(1, set.Count, "set.Count");
        }

        [Test]
        public void adding_an_nullable_struct_with_value_returns_true_when_item_not_in_set()
        {
            var set = new UniqueList<int> { 1 };
            int? val = 2;
            Assert.AreEqual(true, set.Add(val), "set.Add(null)");
            Assert.AreEqual(2, set.Count, "set.Count");
        }

        [Test]
        public void can_add_unique_strings()
        {
            var set = new UniqueList<string>();
            Assert.AreEqual(true, set.Add(1+"a"));
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void adding_a_null_returns_false()
        {
            var set = new UniqueList<string>();
            Assert.AreEqual(false, set.Add(null));
        }

        [Test]
        public void adding_a_null_does_not_increase_the_collection_count()
        {
            var set = new UniqueList<string>();
            set.Add(null);
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void cannot_add_duplicate_values()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(1));
            Assert.AreEqual(false, set.Add(1));
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void can_add_unique_value2()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(1));
            Assert.AreEqual(true, set.Add(2));
            Assert.AreEqual(2, set.Count);
        }

        [Test]
        public void can_fill_set()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(1));
            Assert.AreEqual(true, set.Add(2));
            Assert.AreEqual(true, set.Add(3));
            Assert.AreEqual(3, set.Count);
        }

        [Test]
        public void can_resize_set()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(1));
            Assert.AreEqual(true, set.Add(2));
            Assert.AreEqual(true, set.Add(3));
            Assert.AreEqual(true, set.Add(4));
            Assert.AreEqual(true, set.Add(5));
            Assert.AreEqual(5, set.Count);
        }

        [Test]
        public void can_access_items_by_index_of_order_added()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(3));
            Assert.AreEqual(true, set.Add(1));
            Assert.AreEqual(true, set.Add(2));
            Assert.AreEqual(3, set[0]);
            Assert.AreEqual(1, set[1]);
            Assert.AreEqual(2, set[2]);
        }

        [Test]
        public void enumerates_items_in_the_order_they_were_added()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(3));
            Assert.AreEqual(true, set.Add(1));
            Assert.AreEqual(true, set.Add(2));
            Assert.AreEqual(3, set.ElementAt(0));
            Assert.AreEqual(1, set.ElementAt(1));
            Assert.AreEqual(2, set.ElementAt(2));
        }

        [Test]
        public void contains_item_we_have_added()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(3));
            Assert.AreEqual(true, set.Contains(3));
        }

        [Test]
        public void does_not_contain_item_we_have_not_added()
        {
            var set = new UniqueList<int>();
            Assert.AreEqual(true, set.Add(1));
            Assert.AreEqual(true, set.Add(2));
            Assert.AreEqual(true, set.Add(4));
            Assert.AreEqual(false, set.Contains(3));
        }

        [Test]
        public void can_delete_item_that_exists()
        {
            var set = new UniqueList<int> { 1, 2, 3, 4 };
            Assert.AreEqual(true, set.Remove(3));

            Assert.AreEqual(0, set.IndexOf(1), "set.IndexOf(1)");
            Assert.AreEqual(1, set.IndexOf(2), "set.IndexOf(2)");
            Assert.AreEqual(2, set.IndexOf(4), "set.IndexOf(4)");
            Assert.AreEqual(-1, set.IndexOf(3), "set.IndexOf(3)");
            Assert.AreEqual(3, set.Count, "count");
        }

        [Test]
        public void can_delete_last_item()
        {
            var set = new UniqueList<int> { 1, 2, 3, 4 };
            Assert.AreEqual(true, set.Remove(4));

            Assert.AreEqual(0, set.IndexOf(1), "set.IndexOf(1)");
            Assert.AreEqual(1, set.IndexOf(2), "set.IndexOf(2)");
            Assert.AreEqual(2, set.IndexOf(3), "set.IndexOf(3)");
            Assert.AreEqual(-1, set.IndexOf(4), "set.IndexOf(4)");
            Assert.AreEqual(3, set.Count, "count");
        }

        [Test]
        public void can_add_same_value_after_deleting()
        {
            var set = new UniqueList<int> { 1, 2, 3, 4 };
            Assert.AreEqual(true, set.Remove(4));
            Assert.AreEqual(true, set.Add(4));

            Assert.AreEqual(4, set.Count, "count");
            Assert.AreEqual(0, set.IndexOf(1), "set.IndexOf(1)");
            Assert.AreEqual(1, set.IndexOf(2), "set.IndexOf(2)");
            Assert.AreEqual(2, set.IndexOf(3), "set.IndexOf(3)");
            Assert.AreEqual(3, set.IndexOf(4), "set.IndexOf(4)");
        }

        [Test]
        public void can_add_same_value_into_deleted_slot()
        {
            Assert.AreEqual(3.GetHashCode() % 7, 10.GetHashCode() % 7);

            var set = new UniqueList<int> { 1, 2, 3, 4 };
            Assert.AreEqual(true, set.Remove(3));
            Assert.AreEqual(true, set.Add(10));

            Assert.AreEqual(4, set.Count, "count");
            Assert.AreEqual(0, set.IndexOf(1), "set.IndexOf(1)");
            Assert.AreEqual(1, set.IndexOf(2), "set.IndexOf(2)");
            Assert.AreEqual(2, set.IndexOf(4), "set.IndexOf(3)");
            Assert.AreEqual(3, set.IndexOf(10), "set.IndexOf(10)");
        }


        [Test]
        public void can_set_value_by_index()
        {
            var set = new UniqueList<int> { 1, 2, 3 };
            set[0] = 10;

            Assert.AreEqual(3, set.Count, "count");
            Assert.AreEqual(0, set.IndexOf(10), "set.IndexOf(10)");
            Assert.AreEqual(1, set.IndexOf(2), "set.IndexOf(2)");
            Assert.AreEqual(2, set.IndexOf(3), "set.IndexOf(3)");
        }

        [Test]
        public void can_set_value_by_index_to_itself()
        {
            var set = new UniqueList<int> { 1, 2, 3 };
            Assert.AreEqual(3, set.Count, "count");
            set[0] = 1;

            Assert.AreEqual(3, set.Count, "count");
            Assert.AreEqual(0, set.IndexOf(1), "set.IndexOf(1)");
            Assert.AreEqual(1, set.IndexOf(2), "set.IndexOf(2)");
            Assert.AreEqual(2, set.IndexOf(3), "set.IndexOf(3)");
        }

        [Test]
        public void cannot_set_value_by_index_if_value_in_another_location_in_the_list()
        {
            var set = new UniqueList<int> { 1, 2, 3 };
            Assert.AreEqual(3, set.Count, "count");
            Assert.Throws<ArgumentException>(() => set[0] = 3);
        }
    }
}