namespace BusterWood.Channels
{
    interface INext<T>
    {
        T Next { get; set; }
    }

    struct LinkedQueue<T> where T : class, INext<T>
    {
        public T Head;
        public T Tail;
    }

    static class Queue
    {
        public static void Enqueue<T>(ref LinkedQueue<T> q, T item) where T : class, INext<T>
        {
            if (q.Tail == null)
            {
                q.Head = q.Tail = item;
            }
            else
            {
                q.Tail.Next = item;
                q.Tail = item;
            }
        }

        public static T Dequeue<T>(ref LinkedQueue<T> q) where T : class, INext<T>
        {
            var item = q.Head;
            if (item != null)
            {
                q.Head = item.Next;
                if (q.Head == null)
                    q.Tail = null;
                else
                    item.Next = null;
            }
            return item;
        }

        public static void Remove<T>(ref LinkedQueue<T> q, T item) where T : class, INext<T>
        {
            if (q.Head == item)
            {
                q.Head = item.Next;
                if (q.Head == null)
                    q.Tail = null;
            }
            else
            {
                for (var val = q.Head; val != null; val = val.Next)
                {
                    if (val.Next == item)
                    {
                        val.Next = item.Next;
                        item.Next = null;
                        if (q.Tail == item)
                            q.Tail = val;
                        break;
                    }
                }
            }
        }
    }
}