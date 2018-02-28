using BusterWood.Testing;
using System;

namespace BusterWood.Channels
{
    public class BufferedChannelTests
    {
        public static void trying_to_receive_from_an_empty_channel_returns_false(Test t)
        {
            var ch = new BufferedChannel<int>(3);
            int val;
            if (ch.TryReceive(out val) != false)
                t.Fatal("returned true receiving from a new (empty) channel");
        }

        public static void trying_to_send_to_an_empty_channel_returns_true(Test t)
        {
            var ch = new BufferedChannel<int>(3);
            if (ch.TrySend(2) != true)
                t.Fatal("returned true sending to an new (empty) channel");
        }

        public static void sending_async_returns_a_waiting_task(Test t)
        {
            var ch = new BufferedChannel<int>(1);

            // first value will be buffered
            var i1 = ch.SendAsync(1);
            if (i1 == null)
                t.Fatal("returned task was null");
            if (!i1.IsCompleted)
                t.Fatal("returned task was complete");

            // second value must wait
            var i2 = ch.SendAsync(2);
            if (i2 == null)
                t.Fatal("returned task was null");
            if (i2.IsCompleted)
                t.Fatal("returned task was complete");
        }

        public static void receiving_async_returns_a_waiting_task(Test t)
        {
            var ch = new BufferedChannel<int>(3);
            var i = ch.ReceiveAsync();
            if (i == null)
                t.Fatal("returned task was null");
            if (i.IsCompleted)
                t.Fatal("returned task was complete");
        }

        public static void receiving_async_returns_a_waiting_task_which_is_complete_by_trying_to_send_a_value(Test t)
        {
            var ch = new BufferedChannel<int>(3);
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
            var ch = new BufferedChannel<int>(3);
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
            var ch = new BufferedChannel<int>(1);

            var st1 = ch.SendAsync(1);
            var st2 = ch.SendAsync(2);
            if (st2.IsCompleted)
                t.Fatal("second send should wait for a recevier");

            int val;
            if (ch.TryReceive(out val) != true)
                t.Fatal("TryReceive failed when a receiver was waiting");
            if (ch.TryReceive(out val) != true)
                t.Fatal("TryReceive failed when a receiver was waiting");
            if (st2.Wait(100) == false || st2.IsCompleted == false)
                t.Fatal("sending task did not complete, state is " + st2.Status);
            if (val != 2)
                t.Fatal("received value (" + val + ") is not what we sent (2)");
        }

        public static void sending_async_returns_a_waiting_task_which_is_complete_by_receive_a_value_async(Test t)
        {
            var ch = new BufferedChannel<int>(1);
            var st1 = ch.SendAsync(1);
            var st2 = ch.SendAsync(2);
            if (st2.IsCompleted)
                t.Fatal("second send should wait for a recevier");
            var rt = ch.ReceiveAsync();
            var rt2 = ch.ReceiveAsync();
            if (rt == null)
                t.Fatal("ReceiveAsync returned null");
            if (!rt2.IsCompleted)
                t.Fatal("ReceiveAsync is not complete");
            if (st2.Wait(100) == false || st2.IsCompleted == false)
                t.Fatal("sending task did not complete, state is " + st2.Status);
            if (rt2.Result != 2)
                t.Fatal("received value (" + rt2.Result + ") is not what we sent (2)");
        }

        public static void sendAsync_on_a_closed_channel_returns_a_cancelled_task(Test t)
        {
            var ch = new BufferedChannel<int>(3);
            ch.Close();
            var st = ch.SendAsync(2);
            if (st == null)
                t.Fatal("returned task was null");
            if (!st.IsCanceled)
                t.Fatal("returned task was not cancelled");
        }

        public static void trySend_on_a_closed_channel_returns_false(Test t)
        {
            var ch = new BufferedChannel<int>(3);
            ch.Close();
            if (ch.TrySend(2) != false)
                t.Fatal("was able to send on closed channel");
        }

        public static void try_receive_on_a_closed_channel_return_false(Test t)
        {
            var ch = new BufferedChannel<int>(3);
            ch.Close();
            int val;
            if (ch.TryReceive(out val) != false)
                t.Fatal("was able to receive on closed channel");
        }

        public static void ReceiveAsync_on_a_closed_channel_return_cancelled_task(Test t)
        {
            var ch = new BufferedChannel<int>(3);
            ch.Close();
            var rt = ch.ReceiveAsync();
            if (rt == null)
                t.Fatal("ReceiveAsync returned null");
            if (!rt.IsCanceled)
                t.Fatal("ReceiveAsync is not cancelled");
        }

        public static void closing_the_channel_with_a_waiting_sender_does_not_cancel_the_senders_task(Test t)
        {
            var ch = new BufferedChannel<int>(1);
            var st1 = ch.SendAsync(1);
            var st2 = ch.SendAsync(2);
            if (st2 == null)
                t.Fatal("returned task was null");
            if (st2.IsCompleted)
                t.Fatal("returned task was complete already!");
            ch.Close();
            if (st2.Wait(100))
                t.Fatal("task completed, it should just wait");
        }

        public static void closing_the_channel_with_a_waiting_receiver_cancel_the_receivers_task(Test t)
        {
            var ch = new BufferedChannel<int>(3);
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
            var before = GC.GetTotalMemory(true);
            var chans = new BufferedChannel<int>[10000];
            for (int i = 0; i < chans.Length; i++)
            {
                chans[i] = new BufferedChannel<int>(3);
            }
            var after = GC.GetTotalMemory(true);
            GC.KeepAlive(chans);
            var diff = after - before - (chans.Length * 4);
            var kb = diff / 1024;
            Console.WriteLine("10,000 buffered channels of size 3 takes " + kb + "KB");
        }
    }
}