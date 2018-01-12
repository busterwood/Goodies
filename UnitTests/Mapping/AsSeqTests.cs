using NUnit.Framework;
using System.Linq;

namespace BusterWood.Mapping
{
    [TestFixture]
    public class AsSeqTests
    {
        [Test]
        public void empty_sequence_returned_for_null()
        {
            HasProperties item = null;
            Assert.IsNotNull(item.AsSeq());
            Assert.AreEqual(0, item.AsSeq().Count());
        }

        [Test]
        public void sequence_returned_for_object()
        {
            HasProperties item = new HasProperties();
            Assert.IsNotNull(item.AsSeq());
            Assert.AreNotEqual(0, item.AsSeq().Count());
        }

        [Test]
        public void sequence_contains_pair_for_default_value_of_int()
        {
            HasProperties item = new HasProperties();
            Assert.IsTrue(item.AsSeq().Contains(p => p.Key == nameof(HasProperties.Int1)));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void sequence_contains_value_of_struct(int value)
        {
            HasProperties item = new HasProperties { Int1 = value };
            var pair = item.AsSeq().First(p => p.Key == nameof(HasProperties.Int1));
            Assert.AreEqual(value, pair.Value);
        }

        [Test]
        public void sequence_does_not_contain_value_of_null_class()
        {
            HasProperties item = new HasProperties();
            Assert.IsFalse(item.AsSeq().Contains(p => p.Key == nameof(HasProperties.Text)));
        }

        [TestCase("")]
        [TestCase("hello")]
        public void sequence_contains_value_of_class(string value)
        {
            HasProperties item = new HasProperties { Text = value };
            var pair = item.AsSeq().FirstOrDefault(p => p.Key == nameof(HasProperties.Text));
            Assert.AreEqual(value, pair.Value);
        }

        [Test]
        public void sequence_does_not_contain_value_of_nullable_struct()
        {
            HasProperties item = new HasProperties();
            Assert.IsFalse(item.AsSeq().Contains(p => p.Key == nameof(HasProperties.OptLong)));
        }

        [TestCase(0L)]
        [TestCase(1L)]
        public void sequence_contains_value_of_nullable_struct(long value)
        {
            HasProperties item = new HasProperties { OptLong = value };
            var pair = item.AsSeq().FirstOrDefault(p => p.Key == nameof(HasProperties.OptLong));
            Assert.AreEqual(value, pair.Value);
        }

        class HasProperties
        {
            public int Int1 { get; set; }
            public string Text{ get; set; }
            public long? OptLong;
        }
    }
}