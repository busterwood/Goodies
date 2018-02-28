using BusterWood.Testing;
using System;
using System.Linq;

namespace BusterWood.Collections
{
    public static class UniqueListTests
    {
        public static void can_add_unique_value(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(1));
            t.Assert(() => 1 == set.Count);
        }

        public static void adding_an_nullable_struct_with_null_value_returns_false(Test t)
        {
            var set = new UniqueList<int>();
            int? val = null;
            t.Assert(() => !set.Add(val));
            t.Assert(() => 0 == set.Count);
        }

        public static void adding_an_nullable_struct_with_value_returns_false_when_item_already_in_set(Test t)
        {
            var set = new UniqueList<int>() { 1 };
            int? val = 1;
            t.Assert(() => !set.Add(val));
            t.Assert(() => 1 == set.Count);
        }

        public static void adding_an_nullable_struct_with_value_returns_true_when_item_not_in_set(Test t)
        {
            var set = new UniqueList<int> { 1 };
            int? val = 2;
            t.Assert(() => set.Add(val));
            t.Assert(() => 2 == set.Count);
        }

        public static void can_add_unique_strings(Test t)
        {
            var set = new UniqueList<string>();
            t.Assert(() => set.Add(1 + "a"));
            t.Assert(() => 1 == set.Count);
        }

        public static void adding_a_null_returns_false(Test t)
        {
            var set = new UniqueList<string>();
            t.Assert(() => !set.Add(null));
        }

        public static void adding_a_null_does_not_increase_the_collection_count(Test t)
        {
            var set = new UniqueList<string>();
            set.Add(null);
            t.Assert(() => 0 == set.Count);
        }

        public static void cannot_add_duplicate_values(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(1));
            t.Assert(() => !set.Add(1));
            t.Assert(() => 1 == set.Count);
        }

        public static void can_add_unique_value2(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(1));
            t.Assert(() => set.Add(2));
            t.Assert(() => 2 == set.Count);
        }

        public static void can_fill_set(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(1));
            t.Assert(() => set.Add(2));
            t.Assert(() => set.Add(3));
            t.Assert(() => 3 == set.Count);
        }

        public static void can_resize_set(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(1));
            t.Assert(() => set.Add(2));
            t.Assert(() => set.Add(3));
            t.Assert(() => set.Add(4));
            t.Assert(() => set.Add(5));
            t.Assert(() => 5 == set.Count);
        }

        public static void can_access_items_by_index_of_order_added(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(3));
            t.Assert(() => set.Add(1));
            t.Assert(() => set.Add(2));
            t.Assert(() => 3 == set[0]);
            t.Assert(() => 1 == set[1]);
            t.Assert(() => 2 == set[2]);
        }

        public static void enumerates_items_in_the_order_they_were_added(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(3));
            t.Assert(() => set.Add(1));
            t.Assert(() => set.Add(2));
            t.Assert(() => 3 == set.ElementAt(0));
            t.Assert(() => 1 == set.ElementAt(1));
            t.Assert(() => 2 == set.ElementAt(2));
        }

        public static void contains_item_we_have_added(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(3));
            t.Assert(() => set.Contains(3));
        }

        public static void does_not_contain_item_we_have_not_added(Test t)
        {
            var set = new UniqueList<int>();
            t.Assert(() => set.Add(1));
            t.Assert(() => set.Add(2));
            t.Assert(() => set.Add(4));
            t.Assert(() => !set.Contains(3));
        }

        public static void can_delete_item_that_exists(Test t)
        {
            var set = new UniqueList<int> { 1, 2, 3, 4 };
            t.Assert(() => set.Remove(3));

            t.Assert(() => 0 == set.IndexOf(1));
            t.Assert(() => 1 == set.IndexOf(2));
            t.Assert(() => 2 == set.IndexOf(4));
            t.Assert(() => -1 == set.IndexOf(3));
            t.Assert(() => 3 == set.Count);
        }

        public static void can_delete_last_item(Test t)
        {
            var set = new UniqueList<int> { 1, 2, 3, 4 };
            t.Assert(() => set.Remove(4));

            t.Assert(() => 0 == set.IndexOf(1));
            t.Assert(() => 1 == set.IndexOf(2));
            t.Assert(() => 2 == set.IndexOf(3));
            t.Assert(() => -1 == set.IndexOf(4));
            t.Assert(() => 3 == set.Count);
        }

        public static void can_add_same_value_after_deleting(Test t)
        {
            var set = new UniqueList<int> { 1, 2, 3, 4 };
            t.Assert(() => set.Remove(4));
            t.Assert(() => set.Add(4));

            t.Assert(() => 4 == set.Count);
            t.Assert(() => 0 == set.IndexOf(1));
            t.Assert(() => 1 == set.IndexOf(2));
            t.Assert(() => 2 == set.IndexOf(3));
            t.Assert(() => 3 == set.IndexOf(4));
        }

        public static void can_add_same_value_into_deleted_slot(Test t)
        {
            t.Assert(() => 3.GetHashCode() % 7 == 10.GetHashCode() % 7);

            var set = new UniqueList<int> { 1, 2, 3, 4 };
            t.Assert(() => set.Remove(3));
            t.Assert(() => set.Add(10));

            t.Assert(() => 4 == set.Count);
            t.Assert(() => 0 == set.IndexOf(1));
            t.Assert(() => 1 == set.IndexOf(2));
            t.Assert(() => 2 == set.IndexOf(4));
            t.Assert(() => 3 == set.IndexOf(10));
        }

        public static void can_set_value_by_index(Test t)
        {
            var set = new UniqueList<int> { 1, 2, 3 };
            set[0] = 10;

            t.Assert(() => 3 == set.Count);
            t.Assert(() => 0 == set.IndexOf(10));
            t.Assert(() => 1 == set.IndexOf(2));
            t.Assert(() => 2 == set.IndexOf(3));
        }

        public static void can_set_value_by_index_to_itself(Test t)
        {
            var set = new UniqueList<int> { 1, 2, 3 };
            t.Assert(() => 3 == set.Count);
            set[0] = 1;

            t.Assert(() => 3 == set.Count);
            t.Assert(() => 0 == set.IndexOf(1));
            t.Assert(() => 1 == set.IndexOf(2));
            t.Assert(() => 2 == set.IndexOf(3));
        }

        public static void cannot_set_value_by_index_if_value_in_another_location_in_the_list(Test t)
        {
            var set = new UniqueList<int> { 1, 2, 3 };
            t.Assert(() => 3 == set.Count);
            t.AssertThrows<ArgumentException>(() => set[0] = 3, "cannot set when value would be a duplicate");
        }
    }
}