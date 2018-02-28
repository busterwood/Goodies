using BusterWood.Testing;

namespace BusterWood.Collections
{
    public class CircularQueueTests
    {
        public static void zero_capacity_queue_is_empty(Test t)
        {
            var q = new CircularQueue<int>(0);
            t.Assert(() => q.IsEmpty);
        }

        public static void zero_capacity_queue_is_full(Test t)
        {
            var q = new CircularQueue<int>(0);
            t.Assert(() => q.IsFull);
        }
    }
}
