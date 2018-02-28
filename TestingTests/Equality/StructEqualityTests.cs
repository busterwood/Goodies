using System;
using BusterWood.Testing;

namespace BusterWood.Equality
{
    public class StructEqualityTests
    {
        public static void can_check_equality_with_integer(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 2, Name = "world" };
            t.Assert(() => eq.Equals(left, right));
        }

        public static void not_equal_if_int_property_value_different(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "world" };
            t.AssertNot(() => eq.Equals(left, right));
        }

        public static void can_check_equality_with_string(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "hello" };
            t.Assert(() => eq.Equals(left, right));
        }

        public static void can_check_equality_with_string_comparer(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(StringComparer.OrdinalIgnoreCase, nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "HELLO" };
            t.Assert(() => eq.Equals(left, right));
        }

        public static void not_equal_if_string_property_value_different(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 2, Name = "world" };
            t.AssertNot(() => eq.Equals(left, right));
        }

        public static void can_check_equality_with_multiple_properties(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id), nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 2, Name = "hello" };
            t.Assert(() => eq.Equals(left, right));
        }

        public static void not_equal_if_first_of_multiple_properties_does_not_match(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id), nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "hello" };
            t.AssertNot(() => eq.Equals(left, right));
        }

        public static void not_equal_if_last_of_multiple_properties_does_not_match(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id), nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 2, Name = "world" };
            t.AssertNot(() => eq.Equals(left, right));
        }

        public static void can_get_hashcode_of_int_property(Test t)
        {
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id));
            t.Assert(() => 0 != eq.GetHashCode(left));
        }

        public static void can_get_hashcode_of_string_property(Test t)
        {
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Name));
            t.Assert(() => 0 != eq.GetHashCode(left));
        }

        public static void can_get_hashcode_of_null_string_property(Test t)
        {
            Test1 left = new Test1 { Id = 2, Name = null };
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Name));
            t.Assert(() => 0 == eq.GetHashCode(left));
        }

        public static void hashcodes_are_equals_when_using_string_comparer(Test t)
        {
            var eq = EqualityComparer.Create<Test1>(StringComparer.OrdinalIgnoreCase, nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "HELLO" };
            t.Assert(() => eq.GetHashCode(left) == eq.GetHashCode(right));
        }

        public struct Test1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

    }
}
