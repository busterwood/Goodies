using BusterWood.Linq;
using NUnit.Framework;
using System.Linq;

namespace UnitTests
{
    [TestFixture]
    public class ForTests
    {
        [Test]
        public void for_returns_sequence_from_x_to_for_y_numbers()
        {
            var numbers = 10.For(3).ToList();
            Assert.AreEqual(3, numbers.Count);
            Assert.AreEqual(10, numbers[0]);
            Assert.AreEqual(11, numbers[1]);
            Assert.AreEqual(12, numbers[2]);
        }
    }

    [TestFixture]
    public class ToTests
    {
        [Test]
        public void to_returns_sequence_from_x_to_one_less_than_y()
        {
            var numbers = 10.To(12).ToList();
            Assert.AreEqual(3, numbers.Count);
            Assert.AreEqual(10, numbers[0]);
            Assert.AreEqual(11, numbers[1]);
            Assert.AreEqual(12, numbers[2]);
        }
    }
}
