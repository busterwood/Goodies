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
    public class ClassChooseTests
    {
        [Test]
        public void choose_returns_a_value_when_function_returns_a_value()
        {
            var input = new[] { "1" };
            var output = input.Choose(s => s.Length == 1 ? s : null).ToList();
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("1", output[0]);
        }

        [Test]
        public void choose_does_not_return_a_value_when_function_returns_null()
        {
            var input = new[] { "one" };
            var output = input.Choose(s => s.Length == 1 ? s : null).ToList();
            Assert.AreEqual(0, output.Count);
        }
    }


    [TestFixture]
    public class StructChooseTests
    {
        [Test]
        public void choose_returns_a_value_when_function_returns_a_value()
        {
            var input = new[] { 10 };
            var output = input.Choose(i => i > 0 ? i : (int?)null).ToList();
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual(10, output[0]);
        }

        [Test]
        public void choose_does_not_return_a_value_when_function_returns_null()
        {
            var input = new[] { -1 };
            var output = input.Choose(i => i > 0 ? i : (int?)null).ToList();
            Assert.AreEqual(0, output.Count);
        }
    }
}
