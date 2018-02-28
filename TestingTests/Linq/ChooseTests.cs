using BusterWood.Linq;
using BusterWood.Testing;
using System.Linq;

namespace BusterWood.Linq
{
    public class ClassChooseTests
    {
        public void choose_returns_a_value_when_function_returns_a_value(Test t)
        {
            var input = new[] { "1" };
            var output = input.Choose(s => s.Length == 1 ? s : null).ToList();
            t.Assert(() => 1 == output.Count);
            t.Assert(() => "1" == output[0]);
        }

        public void choose_does_not_return_a_value_when_function_returns_null(Test t)
        {
            var input = new[] { "one" };
            var output = input.Choose(s => s.Length == 1 ? s : null).ToList();
            t.Assert(() => 0 == output.Count);
        }
    }

    public class StructChooseTests
    {
        public void choose_returns_a_value_when_function_returns_a_value(Test t)
        {
            var input = new[] { 10 };
            var output = input.Choose(i => i > 0 ? i : (int?)null).ToList();
            t.Assert(() => 1 == output.Count);
            t.Assert(() => 10 == output[0]);
        }

        public void choose_does_not_return_a_value_when_function_returns_null(Test t)
        {
            var input = new[] { -1 };
            var output = input.Choose(i => i > 0 ? i : (int?)null).ToList();
            t.Assert(() => 0 == output.Count);
        }
    }
}
