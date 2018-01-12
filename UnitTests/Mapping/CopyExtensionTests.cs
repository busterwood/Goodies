using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

namespace BusterWood.Mapping
{
    [TestFixture]
    public class CopyExtensionTests
    {
        [Test]
        public void can_clone_string_property()
        {
            var input = new SingleProp<string>{ Value = "hello" };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreSame(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_bool_property()
        {
            var input = new SingleProp<bool>{ Value = true };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_byte_property()
        {
            var input = new SingleProp<byte>{ Value = 1 };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_short_property()
        {
            var input = new SingleProp<short>{ Value = 1 };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_int_property()
        {
            var input = new SingleProp<int>{ Value = 1 };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_long_property()
        {
            var input = new SingleProp<long>{ Value = 1 };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_datetime_property()
        {
            var input = new SingleProp<DateTime>{ Value = DateTime.Now };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_float_property()
        {
            var input = new SingleProp<float>{ Value = 1.1f };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_double_property()
        {
            var input = new SingleProp<double>{ Value = 1.1d };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_decimal_property()
        {
            var input = new SingleProp<decimal>{ Value = 1.1m };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreEqual(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_array_property()
        {
            var input = new SingleProp<decimal[]>{ Value = new [] {1m, 2m} };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreSame(input.Value, copy.Value);
        }

        [Test]
        public void can_clone_list_property()
        {
            var input = new SingleProp<List<string>>{ Value = new List<string> {"one", "two"} };
            var copy = input.Copy();
            Assert.AreNotSame(input, copy);
            Assert.AreSame(input.Value, copy.Value);
        }

        [Test]
        public void can_copy_enum_to_int()
        {
            var input = new SingleProp<TestEnum> { Value = TestEnum.Something};
            var output = input.Copy<SingleProp<TestEnum>, SingleProp<int>>();
            Assert.AreEqual((int)input.Value, output.Value);
        }

        [Test]
        public void can_copy_enum_to_long()
        {
            var input = new SingleProp<TestEnum> { Value = TestEnum.Something };
            var output = input.Copy<SingleProp<TestEnum>, SingleProp<long>>();
            Assert.AreEqual((long)input.Value, output.Value);
        }

        [Test]
        public void can_copy_enum_to_another_enum()
        {
            var input = new SingleProp<TestEnum> { Value = TestEnum.Something };
            var output = input.Copy<SingleProp<TestEnum>, SingleProp<TestEnum2>>();
            Assert.AreEqual((int)input.Value, (int)output.Value);
        }

        [Test]
        public void can_copy_int_to_enum()
        {
            var input = new SingleProp<int> { Value = 1};
            var output = input.Copy<SingleProp<int>, SingleProp<TestEnum>>();
            Assert.AreEqual((TestEnum)input.Value, output.Value);
        }

        [Test]
        public void can_copy_long_to_enum()
        {
            var input = new SingleProp<long> { Value = 1 };
            var output = input.Copy<SingleProp<long>, SingleProp<TestEnum>>();
            Assert.AreEqual((TestEnum)input.Value, output.Value);
        }

        [Test]
        public void can_copy_nullable_long_to_nullable_enum()
        {
            var input = new SingleProp<long?> { Value = 1 };
            SingleProp<TestEnum?> output = input.Copy<SingleProp<long?>, SingleProp<TestEnum?>>();
            Assert.AreEqual((TestEnum)input.Value, output.Value);
        }

        [Test]
        public void can_copy_nullable_enum_to_nullable_int()
        {
            var input = new SingleProp<TestEnum?> { Value = TestEnum.Something };
            var output = input.Copy<SingleProp<TestEnum?>, SingleProp<int?>>();
            Assert.AreEqual(1, output.Value);
        }

        [Test]
        public void can_copy_null_value_nullable_enum_to_nullable_int_as_null()
        {
            var input = new SingleProp<TestEnum?> { Value = null};
            var output = input.Copy<SingleProp<TestEnum?>, SingleProp<int?>>();
            Assert.AreEqual(null, output.Value);
        }

        [Test]
        public void can_copy_nullable_int_to_int()
        {
            var input = new SingleProp<int?> { Value = 1 };
            var output = input.Copy<SingleProp<int?>, SingleProp<int>>();
            Assert.AreEqual(1, output.Value);
        }

        [Test]
        public void can_copy_long_to_nullable_long() {
            var input = new SingleProp<long> { Value = 1 };
            var output = input.Copy<SingleProp<long>, SingleProp<long?>>();
            Assert.AreEqual(input.Value, output.Value);
        }

        [Test]
        public void can_copy_nullable_long_to_long() {
            var input = new SingleProp<long?> { Value = 1 };
            var output = input.Copy<SingleProp<long?>, SingleProp<long>>();
            Assert.AreEqual(input.Value.Value, output.Value);
        }

        [Test]
        public void can_copy_null_nullable_long_to_default() {
            var input = new SingleProp<long?> { Value = (long?)null };
            var output = input.Copy<SingleProp<long?>, SingleProp<long>>();
            Assert.AreEqual(0, output.Value);
        }

        [Test]
        public void can_copy_nullable_int_to_long() {
            var input = new SingleProp<int?> { Value = 1 };
            var output = input.Copy<SingleProp<int?>, SingleProp<long>>();
            Assert.AreEqual(1, output.Value);
        }

        [Test]
        public void can_copy_null_nullable_int_to_long() {
            var input = new SingleProp<int?> { Value = (int?)null };
            var output = input.Copy<SingleProp<int?>, SingleProp<long>>();
            Assert.AreEqual(0, output.Value);
        }

        [Test]
        public void can_copy_name_ending_in_id_to_name_without_id() {
            var input = new SingleId<int> { ValueId = 1 };
            var output = input.Copy<SingleId<int>, SingleProp<int>>();
            Assert.AreEqual(1, output.Value);
        }

        [Test]
        public void can_copy_nullable_name_ending_in_id_to_name_without_id()
        {
            var input = new SingleId<int?> { ValueId = 1 };
            var output = input.Copy<SingleId<int?>, SingleProp<int>>();
            Assert.AreEqual(1, output.Value);
        }

        [Test]
        public void can_copy_name_ending_in_id_to_name_without_id_enum()
        {
            var input = new SingleId<int> { ValueId = 1 };
            var output = input.Copy<SingleId<int>, SingleProp<TestEnum>>();
            Assert.AreEqual(TestEnum.Something, output.Value);
        }

        [Test]
        public void can_copy_name_without_id_to_name_ending_in_id_enum()
        {
            var ob = new ConsoleObserver();
            using (Mapping.Trace.Subscribe(ob))
            {
                var input = new SingleProp<TestEnum> { Value = TestEnum.Something };
                var output = input.Copy<SingleProp<TestEnum>, SingleId<int>>();
                Assert.AreEqual(1, output.ValueId);
            }
        }

        [Test]
        public void can_copy_name_with_underscores_to_name_without_underscores()
        {
            var input = new WithUnderscore {SOME_VALUE = 1};
            var output = input.Copy<WithUnderscore, WithoutUnderscore>();
            Assert.AreEqual(1, output.SomeValue);
        }

        [Test]
        public void can_copy_class_to_struct()
        {
            var input = new SingleProp<int> { Value = 1 };
            var output = input.Copy<SingleProp<int>, SinglePropStruct<int>>();
            Assert.AreEqual(input.Value, output.Value);
        }

        [Test]
        public void can_copy_struct_to_class()
        {
            var input = new SinglePropStruct<int> { Value = 1 };
            var output = input.Copy<SinglePropStruct<int>, SingleProp<int>>();
            Assert.AreEqual(input.Value, output.Value);
        }

        [Test]
        public void can_CopyTo_existing_object()
        {
            var input = new SingleProp<int> { Value = 1 };
            var existing = new SingleProp<int>();
            var output = input.CopyTo(existing);
            Assert.AreEqual(existing.Value, input.Value);
            Assert.AreEqual(input.Value, output.Value);
            Assert.AreSame(existing, output);
        }

        [Test]
        public void can_CopyTo_existing_object_when_existing_is_null()
        {
            var input = new SingleProp<int> { Value = 1 };
            SingleProp<int> existing = null;
            var output = input.CopyTo(existing);
            Assert.AreEqual(input.Value, output.Value);
        }

        [Test]
        public void can_copy_array()
        {
            var input = new[] { new SingleProp<int> { Value = 1 } };
            var output = input.CopyAll();
            Assert.NotNull(output);
            Assert.AreEqual(1, output.Count());
            Assert.AreEqual(1, output.First().Value);
        }
    }

    public class WithUnderscore
    {
        public int SOME_VALUE { get; set; }

    }
    public class WithoutUnderscore
    {
        public int SomeValue { get; set; }

    }

    public class SingleProp<T>
    {
        public T Value { get; set; }
    }

    public struct SinglePropStruct<T>
    {
        public T Value { get; set; }
    }

    public class SingleId<T>
    {
        public T ValueId { get; set; }
    }

    public enum TestEnum
    {
        Undefined = 0,
        Something = 1
    }

    public enum TestEnum2
    {
        Undefined = 0,
        Something = 1
    }
}