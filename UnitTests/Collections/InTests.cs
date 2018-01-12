using BusterWood.Goodies;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class InTests
    {
        [Test]
        public void in_returns_true_when_value_is_in_list()
        {
            var input = Numbers.One;
            Assert.IsTrue(input.In(Numbers.One, Numbers.Two));
        }

        [Test]
        public void in_returns_false_when_value_not_in_list()
        {
            var input = Numbers.Three;
            Assert.IsFalse(input.In(Numbers.One, Numbers.Two));
        }

        enum Numbers
        {
            One,
            Two,
            Three
        }
    }
}
