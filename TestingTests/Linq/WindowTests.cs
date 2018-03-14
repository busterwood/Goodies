using BusterWood.Linq;
using BusterWood.Testing;
using System;
using System.Linq;

namespace BusterWood.Linq
{

    public static class WindowTests
    {
        public static void argument_exception_thrown_when_input_sequence_is_null(Test t)
        {
            t.AssertThrows<ArgumentException>(() => new int[0].Window(3).ToList());
        }

        public static void single_window_returned_when_number_of_items_less_than_or_equal_to_window_size(Test t)
        {

            for (int i = 1; i <= 3; i++)
            {
                var list = Enumerable.Range(1, i).Window(3).ToList();
                if (list.Count != 1)
                    t.Error($"Expected 1 window but got {list.Count} for input size {i}");
                var first = list[0];
                t.Assert(() => Enumerable.SequenceEqual(Enumerable.Range(1, i), first));

            }
        }


    }
}
