using BusterWood.Channels;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class LinkedQueueTests
    {
        [Test]
        public void head_and_tail_not_set_on_new_queue()
        {
            var q = new LinkedQueue<Waiter>();
            if (q.Head != null)
                Assert.Fail("Head is " + q.Head);
            if (q.Tail != null)
                Assert.Fail("Tail is " + q.Tail);
        }

        [Test]
        public void can_enqueue_into_empty_queue()
        {
            var q = new LinkedQueue<Waiter>();
            var w = new Waiter();
            Queue.Enqueue(ref q, w);
            if (q.Head != w)
                Assert.Fail("Head is " + q.Head + " but expected it to be w");
            if (q.Tail != w)
                Assert.Fail("Tail is " + q.Head + " but expected it to be w");
        }

        [Test]
        public void second_enqueued_item_becomes_the_tail()
        {
            var q = new LinkedQueue<Waiter>();
            var w1 = new Waiter();
            var w2 = new Waiter();
            Queue.Enqueue(ref q, w1);
            Queue.Enqueue(ref q, w2);
            if (q.Head != w1)
                Assert.Fail("Head is " + q.Head + " but expected it to be w1");
            if (q.Tail != w2)
                Assert.Fail("Tail is " + q.Tail + " but expected it to be w2");
        }

        [Test]
        public void dequeuing_the_only_item_in_the_queue_empties_the_queue()
        {
            var q = new LinkedQueue<Waiter>();
            var win = new Waiter();
            Queue.Enqueue(ref q, win);
            var wout = Queue.Dequeue(ref q);
            if (win != wout)
                Assert.Fail("dequeue item is wrong");
            if (q.Head != null)
                Assert.Fail("Head is " + q.Head);
            if (q.Tail != null)
                Assert.Fail("Tail is " + q.Tail);
            if (win.Next != null)
                Assert.Fail("win1.Next is " + win.Next);
        }

        [Test]
        public void dequeuing_one_item_from_a_queue_of_two_sets_the_head_and_tail_to_the_remaining_item()
        {
            var q = new LinkedQueue<Waiter>();
            var win1 = new Waiter();
            var win2 = new Waiter();
            Queue.Enqueue(ref q, win1);
            Queue.Enqueue(ref q, win2);
            var wout = Queue.Dequeue(ref q);
            if (win1 != wout)
                Assert.Fail("dequeue item is wrong");
            if (q.Head != win2)
                Assert.Fail("Head is " + q.Head);
            if (q.Tail != win2)
                Assert.Fail("Tail is " + q.Tail);
            if (win1.Next != null)
                Assert.Fail("win1.Next is " + win1.Next);
        }

        [Test]
        public void removing_the_only_item_from_a_queue_sets_the_head_and_tail_to_null()
        {
            var q = new LinkedQueue<Waiter>();
            var win1 = new Waiter();
            Queue.Enqueue(ref q, win1);
            Queue.Remove(ref q, win1);
            if (q.Head != null)
                Assert.Fail("Head is " + q.Head);
            if (q.Tail != null)
                Assert.Fail("Tail is " + q.Tail);
            if (win1.Next != null)
                Assert.Fail("win1.Next is " + win1.Next);
        }

        [Test]
        public void removing_first_item_from_a_queue_of_two_sets_the_head_and_tail_to_the_remaining_item()
        {
            var q = new LinkedQueue<Waiter>();
            var win1 = new Waiter();
            var win2 = new Waiter();
            Queue.Enqueue(ref q, win1);
            Queue.Enqueue(ref q, win2);
            Queue.Remove(ref q, win1);
            if (q.Head != win2)
                Assert.Fail("Head is " + q.Head);
            if (q.Tail != win2)
                Assert.Fail("Tail is " + q.Tail);
        }

        [Test]
        public void removing_last_item_from_a_queue_of_two_sets_the_head_and_tail_to_the_remaining_item()
        {
            var q = new LinkedQueue<Waiter>();
            var win1 = new Waiter();
            var win2 = new Waiter();
            Queue.Enqueue(ref q, win1);
            Queue.Enqueue(ref q, win2);
            Queue.Remove(ref q, win2);
            if (q.Head != win1)
                Assert.Fail("Head is " + q.Head);
            if (q.Tail != win1)
                Assert.Fail("Tail is " + q.Tail);
        }

    }
}
