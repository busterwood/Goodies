using BusterWood.Testing;

namespace BusterWood.Channels
{
    public class LinkedQueueTests
    {        
        public static void head_and_tail_not_set_on_new_queue(Test t)
        {
            var q = new LinkedQueue<Waiter>();
            if (q.Head != null)
                t.Fatal("Head is " + q.Head);
            if (q.Tail != null)
                t.Fatal("Tail is " + q.Tail);
        }
        
        public static void can_enqueue_into_empty_queue(Test t)
        {
            var q = new LinkedQueue<Waiter>();
            var w = new Waiter();
            Queue.Enqueue(ref q, w);
            if (q.Head != w)
                t.Fatal("Head is " + q.Head + " but expected it to be w");
            if (q.Tail != w)
                t.Fatal("Tail is " + q.Head + " but expected it to be w");
        }
        
        public static void second_enqueued_item_becomes_the_tail(Test t)
        {
            var q = new LinkedQueue<Waiter>();
            var w1 = new Waiter();
            var w2 = new Waiter();
            Queue.Enqueue(ref q, w1);
            Queue.Enqueue(ref q, w2);
            if (q.Head != w1)
                t.Fatal("Head is " + q.Head + " but expected it to be w1");
            if (q.Tail != w2)
                t.Fatal("Tail is " + q.Tail + " but expected it to be w2");
        }
        
        public static void dequeuing_the_only_item_in_the_queue_empties_the_queue(Test t)
        {
            var q = new LinkedQueue<Waiter>();
            var win = new Waiter();
            Queue.Enqueue(ref q, win);
            var wout = Queue.Dequeue(ref q);
            if (win != wout)
                t.Fatal("dequeue item is wrong");
            if (q.Head != null)
                t.Fatal("Head is " + q.Head);
            if (q.Tail != null)
                t.Fatal("Tail is " + q.Tail);
            if (win.Next != null)
                t.Fatal("win1.Next is " + win.Next);
        }
        
        public static void dequeuing_one_item_from_a_queue_of_two_sets_the_head_and_tail_to_the_remaining_item(Test t)
        {
            var q = new LinkedQueue<Waiter>();
            var win1 = new Waiter();
            var win2 = new Waiter();
            Queue.Enqueue(ref q, win1);
            Queue.Enqueue(ref q, win2);
            var wout = Queue.Dequeue(ref q);
            if (win1 != wout)
                t.Fatal("dequeue item is wrong");
            if (q.Head != win2)
                t.Fatal("Head is " + q.Head);
            if (q.Tail != win2)
                t.Fatal("Tail is " + q.Tail);
            if (win1.Next != null)
                t.Fatal("win1.Next is " + win1.Next);
        }
        
        public static void removing_the_only_item_from_a_queue_sets_the_head_and_tail_to_null(Test t)
        {
            var q = new LinkedQueue<Waiter>();
            var win1 = new Waiter();
            Queue.Enqueue(ref q, win1);
            Queue.Remove(ref q, win1);
            if (q.Head != null)
                t.Fatal("Head is " + q.Head);
            if (q.Tail != null)
                t.Fatal("Tail is " + q.Tail);
            if (win1.Next != null)
                t.Fatal("win1.Next is " + win1.Next);
        }
        
        public static void removing_first_item_from_a_queue_of_two_sets_the_head_and_tail_to_the_remaining_item(Test t)
        {
            var q = new LinkedQueue<Waiter>();
            var win1 = new Waiter();
            var win2 = new Waiter();
            Queue.Enqueue(ref q, win1);
            Queue.Enqueue(ref q, win2);
            Queue.Remove(ref q, win1);
            if (q.Head != win2)
                t.Fatal("Head is " + q.Head);
            if (q.Tail != win2)
                t.Fatal("Tail is " + q.Tail);
        }
        
        public static void removing_last_item_from_a_queue_of_two_sets_the_head_and_tail_to_the_remaining_item(Test t)
        {
            var q = new LinkedQueue<Waiter>();
            var win1 = new Waiter();
            var win2 = new Waiter();
            Queue.Enqueue(ref q, win1);
            Queue.Enqueue(ref q, win2);
            Queue.Remove(ref q, win2);
            if (q.Head != win1)
                t.Fatal("Head is " + q.Head);
            if (q.Tail != win1)
                t.Fatal("Tail is " + q.Tail);
        }

    }
}
