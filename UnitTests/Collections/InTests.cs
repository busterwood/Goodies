using BusterWood.Goodies;
using BusterWood.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
