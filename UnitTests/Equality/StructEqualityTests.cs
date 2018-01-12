using NUnit.Framework;
using System;
using BusterWood.Equality;

namespace UnitTests
{
    [TestFixture]
    public class StructEqualityTests
    {
        [Test]
        public void can_check_equality_with_integer()
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 2, Name = "world" };
            Assert.AreEqual(true, eq.Equals(left, right));
        }

        [Test]
        public void not_equal_if_int_property_value_different()
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "world" };
            Assert.AreEqual(false, eq.Equals(left, right));
        }

        [Test]
        public void can_check_equality_with_string()
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "hello" };
            Assert.AreEqual(true, eq.Equals(left, right));
        }

        [Test]
        public void can_check_equality_with_string_comparer()
        {
            var eq = EqualityComparer.Create<Test1>(StringComparer.OrdinalIgnoreCase, nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "HELLO" };
            Assert.AreEqual(true, eq.Equals(left, right));
        }

        [Test]
        public void not_equal_if_string_property_value_different()
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 2, Name = "world" };
            Assert.AreEqual(false, eq.Equals(left, right));
        }

        [Test]
        public void can_check_equality_with_multiple_properties()
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id), nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 2, Name = "hello" };
            Assert.AreEqual(true, eq.Equals(left, right));
        }

        [Test]
        public void not_equal_if_first_of_multiple_properties_does_not_match()
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id), nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "hello" };
            Assert.AreEqual(false, eq.Equals(left, right));
        }

        [Test]
        public void not_equal_if_last_of_multiple_properties_does_not_match()
        {
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id), nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 2, Name = "world" };
            Assert.AreEqual(false, eq.Equals(left, right));
        }

        [Test]
        public void can_get_hashcode_of_int_property()
        {
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Id));
            Assert.AreNotEqual(0, eq.GetHashCode(left));
        }

        [Test]
        public void can_get_hashcode_of_string_property()
        {
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Name));
            Assert.AreNotEqual(0, eq.GetHashCode(left));
        }

        [Test]
        public void can_get_hashcode_of_null_string_property()
        {
            Test1 left = new Test1 { Id = 2, Name = null };
            var eq = EqualityComparer.Create<Test1>(nameof(Test1.Name));
            Assert.AreEqual(0, eq.GetHashCode(left));
        }

        [Test]
        public void hashcodes_are_equals_when_using_string_comparer()
        {
            var eq = EqualityComparer.Create<Test1>(StringComparer.OrdinalIgnoreCase, nameof(Test1.Name));
            Test1 left = new Test1 { Id = 2, Name = "hello" };
            Test1 right = new Test1 { Id = 99, Name = "HELLO" };
            Assert.AreEqual(eq.GetHashCode(left), eq.GetHashCode(right));
        }

        public struct Test1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

    }
}
