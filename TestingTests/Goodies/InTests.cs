using BusterWood.Goodies;
using BusterWood.Testing;

namespace UnitTests
{
    public class InTests
    {
        public static void in_returns_true_when_value_is_in_list(Test t)
        {
            var input = Numbers.One;
            t.Assert(() => input.In(Numbers.One, Numbers.Two));
        }

        public static void in_returns_false_when_value_not_in_list(Test t)
        {
            var input = Numbers.Three;
            t.AssertNot(() => input.In(Numbers.One, Numbers.Two));
        }

        enum Numbers
        {
            One,
            Two,
            Three
        }
    }
}
