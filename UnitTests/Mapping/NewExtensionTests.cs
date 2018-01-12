using NUnit.Framework;
using System.Collections.Generic;

namespace BusterWood.Mapping
{
    [TestFixture]
    public class NewExtensionTests
    {
        [Test]
        public void new_returns_null_when_called_for_class()
        {
            var data = new KeyValuePair<string, object>[0];
            var item = data.New<ClassWithProps>();
            Assert.IsNotNull(item);
        }

        [Test]
        public void new_sets_property_on_class()
        {
            var data = new [] { new KeyValuePair<string, object>(nameof(ClassWithProps.Text), "hello") };
            var item = data.New<ClassWithProps>();
            Assert.AreEqual("hello", item.Text);
        }

        [Test]
        public void new_converts_nullable_properties_between_types()
        {
            var data = new[] { new KeyValuePair<string, object>(nameof(ClassWithProps.OptField), 1) };
            var item = data.New<ClassWithProps>();
            Assert.AreEqual(1, item.OptField);
        }

        [Test]
        public void new_sets_field_on_class()
        {
            var data = new[] { new KeyValuePair<string, object>(nameof(ClassWithProps.OptField), 1L) };
            var item = data.New<ClassWithProps>();
            Assert.AreEqual(1L, item.OptField);
        }

        [Test]
        public void new_returns_null_when_called_for_struct()
        {
            var data = new KeyValuePair<string, object>[0];
            var item = data.New<StructWithProps>();
            Assert.IsNotNull(item);
        }

        [Test]
        public void new_sets_property_on_struct()
        {
            var data = new[] { new KeyValuePair<string, object>(nameof(StructWithProps.Text), "hello") };
            var item = data.New<StructWithProps>();
            Assert.AreEqual("hello", item.Text);
        }

        [Test]
        public void new_sets_field_on_struct()
        {
            var data = new[] { new KeyValuePair<string, object>(nameof(StructWithProps.OptField), 1L) };
            var item = data.New<StructWithProps>();
            Assert.AreEqual(1L, item.OptField);
        }

        [Test]
        public void calls_ctor_with_default_struct_value_when_parameter_not_in_map()
        {
            var data = new[] { new KeyValuePair<string, object>(nameof(ClassWithCtor.Text), "hello") };
            var item = data.New<ClassWithCtor>();
            Assert.AreEqual("hello", item.Text);
            Assert.AreEqual(0, item.Number);
        }

        [Test]
        public void calls_ctor_with_default_class_value_when_parameter_not_in_map()
        {
            var data = new[] { new KeyValuePair<string, object>(nameof(ClassWithCtor.Number), 1L) };
            var item = data.New<ClassWithCtor>();
            Assert.AreEqual(null, item.Text);
            Assert.AreEqual(1L, item.Number);
        }

        class ClassWithProps
        {
            public string Text { get; set; }
            public long? OptField;
        }

        class ClassWithCtor
        {
            public string Text { get;  }
            public long Number { get; }

            public ClassWithCtor(string Text, long Number)
            {
                this.Text = Text;
                this.Number = Number;
            }
        }

        struct StructWithProps
        {
            public string Text { get; set; }
            public long? OptField;
        }
    }
}
