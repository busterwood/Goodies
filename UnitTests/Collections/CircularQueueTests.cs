using NUnit.Framework;

namespace BusterWood.Collections
{
    public class CircularQueueTests
    {
        [Test]
        public void zero_capacity_queue_is_empty()
        {
            var q = new CircularQueue<int>(0);
            Assert.IsTrue(q.IsEmpty);
        }

        [Test]
        public void zero_capacity_queue_is_full()
        {
            var q = new CircularQueue<int>(0);
            Assert.IsTrue(q.IsFull);
        }
    }
}
