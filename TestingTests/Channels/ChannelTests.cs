using BusterWood.Testing;
using System;

namespace BusterWood.Channels
{
    public static class ChannelTests
    {
        public static void trying_to_receive_from_an_empty_channel_returns_false(Test t)
        {
            var ch = new Channel<int>();
            int val;
            if (ch.TryReceive(out val) != false)
                t.Fatal("returned true receiving from a new (empty) channel");
        }

        public static void trying_to_send_to_an_empty_channel_returns_false(Test t)
        {
            var ch = new Channel<int>();
            if (ch.TrySend(2) != false)
                t.Fatal("returned true sending to an new (empty) channel");
        }

        public static void sending_async_returns_a_waiting_task(Test t)
        {
            var ch = new Channel<int>();
            var i = ch.SendAsync(2);
            if (i == null)
                t.Fatal("returned task was null");
            if (i.IsCompleted)
                t.Fatal("returned task was complete");
        }

        public static void receiving_async_returns_a_waiting_task(Test t)
        {
            var ch = new Channel<int>();
            var i = ch.ReceiveAsync();
            if (i == null)
                t.Fatal("returned task was null");
            if (i.IsCompleted)
                t.Fatal("returned task was complete");
        }

        public static void receiving_async_returns_a_waiting_task_which_is_complete_by_trying_to_send_a_value(Test t)
        {
            var ch = new Channel<int>();
            var rt = ch.ReceiveAsync();
            if (rt == null)
                t.Fatal("returned task was null");
            if (rt.IsCompleted)
                t.Fatal("returned task was complete");
            if (ch.TrySend(2) != true)
                t.Fatal("failed to send when a receiver was waiting");
            if (rt.Wait(100) == false || rt.IsCompleted == false)
                t.Fatal("receiver task did not complete");
            if (rt.Result != 2)
                t.Fatal("received value (" + rt.Result + ") is not what we sent (2)");
        }

        public static void receiving_async_returns_a_waiting_task_which_is_complete_by_sending_a_value_async(Test t)
        {
            var ch = new Channel<int>();
            var rt = ch.ReceiveAsync();
            if (rt == null)
                t.Fatal("returned task was null");
            if (rt.IsCompleted)
                t.Fatal("returned task was complete");
            var st = ch.SendAsync(2);
            if (st == null)
                t.Fatal("SendAsync returned null");
            if (!st.IsCompleted)
                t.Fatal("SendAsync is not complete");
            if (rt.Wait(100) == false || rt.IsCompleted == false)
                t.Fatal("receiver task did not complete");
            if (rt.Result != 2)
                t.Fatal("received value (" + rt.Result + ") is not what we sent (2)");
        }

        public static void sending_async_returns_a_waiting_task_which_is_complete_by_trying_to_receive_a_value(Test t)
        {
            var ch = new Channel<int>();
            var st = ch.SendAsync(2);
            if (st == null)
                t.Fatal("returned task was null");
            if (st.IsCompleted)
                t.Fatal("returned task was complete");
            int val;
            if (ch.TryReceive(out val) != true)
                t.Fatal("TryReceive failed when a receiver was waiting");
            if (st.Wait(100) == false || st.IsCompleted == false)
                t.Fatal("sending task did not complete, state is " + st.Status);
            if (val != 2)
                t.Fatal("received value (" + val + ") is not what we sent (2)");
        }

        public static void sending_async_returns_a_waiting_task_which_is_complete_by_receive_a_value_async(Test t)
        {
            var ch = new Channel<int>();
            var st = ch.SendAsync(2);
            if (st == null)
                t.Fatal("returned task was null");
            if (st.IsCompleted)
                t.Fatal("returned task was complete");
            var rt = ch.ReceiveAsync();
            if (rt == null)
                t.Fatal("ReceiveAsync returned null");
            if (!rt.IsCompleted)
                t.Fatal("ReceiveAsync is not complete");
            if (st.Wait(100) == false || st.IsCompleted == false)
                t.Fatal("sending task did not complete, state is " + st.Status);
            if (rt.Result != 2)
                t.Fatal("received value (" + rt.Result + ") is not what we sent (2)");
        }

        public static void sendAsync_on_a_closed_channel_returns_a_cancelled_task(Test t)
        {
            var ch = new Channel<int>();
            ch.Close();
            var st = ch.SendAsync(2);
            if (st == null)
                t.Fatal("returned task was null");
            if (!st.IsCanceled)
                t.Fatal("returned task was not cancelled");
        }

        public static void trySend_on_a_closed_channel_returns_false(Test t)
        {
            var ch = new Channel<int>();
            ch.Close();
            if (ch.TrySend(2) != false)
                t.Fatal("was able to send on closed channel");
        }

        public static void try_receive_on_a_closed_channel_return_false(Test t)
        {
            var ch = new Channel<int>();
            ch.Close();
            int val;
            if (ch.TryReceive(out val) != false)
                t.Fatal("was able to receive on closed channel");
        }

        public static void ReceiveAsync_on_a_closed_channel_return_cancelled_task(Test t)
        {
            var ch = new Channel<int>();
            ch.Close();
            var rt = ch.ReceiveAsync();
            if (rt == null)
                t.Fatal("ReceiveAsync returned null");
            if (!rt.IsCanceled)
                t.Fatal("ReceiveAsync is not cancelled");
        }

        public static void closing_the_channel_with_a_waiting_sender_does_not_cancel_the_senders_task(Test t)
        {
            if (Tests.Short)
                t.SkipNow();


            var ch = new Channel<int>();
            var st = ch.SendAsync(2);
            if (st == null)
                t.Fatal("returned task was null");
            if (st.IsCompleted)
                t.Fatal("returned task was complete already!");
            ch.Close();
            if (st.Wait(100))
                t.Fatal("task completed, it should just wait");
        }

        public static void closing_the_channel_with_a_waiting_receiver_cancel_the_receivers_task(Test t)
        {
            var ch = new Channel<int>();
            var rt = ch.ReceiveAsync();
            if (rt == null)
                t.Fatal("returned task was null");
            if (rt.IsCompleted)
                t.Fatal("returned task was complete already!");
            ch.Close();
            try
            {
                if (!rt.Wait(100))
                    t.Fatal("task did not completed");
            }
            catch (AggregateException)
            {
                if (!rt.IsCanceled)
                    t.Fatal("task is not cancelled");
            }
        }

        public static void memory(Test t)
        {
            if (Tests.Short)
                t.SkipNow();

            var before = GC.GetTotalMemory(true);
            var chans = new Channel<int>[10000];
            for (int i = 0; i < chans.Length; i++)
            {
                chans[i] = new Channel<int>();
            }
            var after = GC.GetTotalMemory(true);
            GC.KeepAlive(chans);
            var diff = after - before - (chans.Length * 4);
            var kb = diff / 1024;
            t.Log("10,000 changes takes " + kb + "KB");
        }
    }
}