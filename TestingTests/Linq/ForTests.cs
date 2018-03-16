using BusterWood.Testing;
using System.Linq;

namespace BusterWood.Linq
{
    public class ForTests
    {        
        public static void for_returns_sequence_from_x_to_for_y_numbers(Test t)
        {
            var numbers = 10.For(3).ToList();
            t.Assert(3, numbers.Count);
            t.Assert(10, numbers[0]);
            t.Assert(11, numbers[1]);
            t.Assert(12, numbers[2]);
        }

        public static void to_returns_sequence_from_x_to_one_less_than_y(Test t)
        {
            var numbers = 10.To(12).ToList();
            t.Assert(3, numbers.Count);
            t.Assert(10, numbers[0]);
            t.Assert(11, numbers[1]);
            t.Assert(12, numbers[2]);
        }
    }
}
