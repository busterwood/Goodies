using BusterWood.Channels;
using NUnit.Framework;
using System;

namespace UnitTests
{
    [TestFixture]
    public class BufferedChannelTests
    {
        [Test]
        public void trying_to_receive_from_an_empty_channel_returns_false()
        {
            var ch = new BufferedChannel<int>(3);
            int val;
            if (ch.TryReceive(out val) != false)
                Assert.Fail("returned true receiving from a new (empty) channel");
        }

        [Test]
        public void trying_to_send_to_an_empty_channel_returns_true()
        {
            var ch = new BufferedChannel<int>(3);
            if (ch.TrySend(2) != true)
                Assert.Fail("returned true sending to an new (empty) channel");
        }

        [Test]
        public void sending_async_returns_a_waiting_task()
        {
            var ch = new BufferedChannel<int>(1);

            // first value will be buffered
            var t1 = ch.SendAsync(1);
            if (t1 == null)
                Assert.Fail("returned task was null");
            if (!t1.IsCompleted)
                Assert.Fail("returned task was complete");

            // second value must wait
            var t2 = ch.SendAsync(2);
            if (t2 == null)
                Assert.Fail("returned task was null");
            if (t2.IsCompleted)
                Assert.Fail("returned task was complete");
        }

        [Test]
        public void receiving_async_returns_a_waiting_task()
        {
            var ch = new BufferedChannel<int>(3);
            var t = ch.ReceiveAsync();
            if (t == null)
                Assert.Fail("returned task was null");
            if (t.IsCompleted)
                Assert.Fail("returned task was complete");
        }

        [Test]
        public void receiving_async_returns_a_waiting_task_which_is_complete_by_trying_to_send_a_value()
        {
            var ch = new BufferedChannel<int>(3);
            var rt = ch.ReceiveAsync();
            if (rt == null)
                Assert.Fail("returned task was null");
            if (rt.IsCompleted)
                Assert.Fail("returned task was complete");
            if (ch.TrySend(2) != true)
                Assert.Fail("failed to send when a receiver was waiting");
            if (rt.Wait(100) == false || rt.IsCompleted == false)
                Assert.Fail("receiver task did not complete");
            if (rt.Result != 2)
                Assert.Fail("received value (" + rt.Result + ") is not what we sent (2)");
        }

        [Test]
        public void receiving_async_returns_a_waiting_task_which_is_complete_by_sending_a_value_async()
        {
            var ch = new BufferedChannel<int>(3);
            var rt = ch.ReceiveAsync();
            if (rt == null)
                Assert.Fail("returned task was null");
            if (rt.IsCompleted)
                Assert.Fail("returned task was complete");
            var st = ch.SendAsync(2);
            if (st == null)
                Assert.Fail("SendAsync returned null");
            if (!st.IsCompleted)
                Assert.Fail("SendAsync is not complete");
            if (rt.Wait(100) == false || rt.IsCompleted == false)
                Assert.Fail("receiver task did not complete");
            if (rt.Result != 2)
                Assert.Fail("received value (" + rt.Result + ") is not what we sent (2)");
        }

        [Test]
        public void sending_async_returns_a_waiting_task_which_is_complete_by_trying_to_receive_a_value()
        {
            var ch = new BufferedChannel<int>(1);

            var st1 = ch.SendAsync(1);
            var st2 = ch.SendAsync(2);
            if (st2.IsCompleted)
                Assert.Fail("second send should wait for a recevier");

            int val;
            if (ch.TryReceive(out val) != true)
                Assert.Fail("TryReceive failed when a receiver was waiting");
            if (ch.TryReceive(out val) != true)
                Assert.Fail("TryReceive failed when a receiver was waiting");
            if (st2.Wait(100) == false || st2.IsCompleted == false)
                Assert.Fail("sending task did not complete, state is " + st2.Status);
            if (val != 2)
                Assert.Fail("received value (" + val + ") is not what we sent (2)");
        }

        [Test]
        public void sending_async_returns_a_waiting_task_which_is_complete_by_receive_a_value_async()
        {
            var ch = new BufferedChannel<int>(1);
            var st1 = ch.SendAsync(1);
            var st2 = ch.SendAsync(2);
            if (st2.IsCompleted)
                Assert.Fail("second send should wait for a recevier");
            var rt = ch.ReceiveAsync();
            var rt2 = ch.ReceiveAsync();
            if (rt == null)
                Assert.Fail("ReceiveAsync returned null");
            if (!rt2.IsCompleted)
                Assert.Fail("ReceiveAsync is not complete");
            if (st2.Wait(100) == false || st2.IsCompleted == false)
                Assert.Fail("sending task did not complete, state is " + st2.Status);
            if (rt2.Result != 2)
                Assert.Fail("received value (" + rt2.Result + ") is not what we sent (2)");
        }

        [Test]
        public void sendAsync_on_a_closed_channel_returns_a_cancelled_task()
        {
            var ch = new BufferedChannel<int>(3);
            ch.Close();
            var st = ch.SendAsync(2);
            if (st == null)
                Assert.Fail("returned task was null");
            if (!st.IsCanceled)
                Assert.Fail("returned task was not cancelled");
        }

        [Test]
        public void trySend_on_a_closed_channel_returns_false()
        {
            var ch = new BufferedChannel<int>(3);
            ch.Close();
            if (ch.TrySend(2) != false)
                Assert.Fail("was able to send on closed channel");
        }

        [Test]
        public void try_receive_on_a_closed_channel_return_false()
        {
            var ch = new BufferedChannel<int>(3);
            ch.Close();
            int val;
            if (ch.TryReceive(out val) != false)
                Assert.Fail("was able to receive on closed channel");
        }

        [Test]
        public void ReceiveAsync_on_a_closed_channel_return_cancelled_task()
        {
            var ch = new BufferedChannel<int>(3);
            ch.Close();
            var rt = ch.ReceiveAsync();
            if (rt == null)
                Assert.Fail("ReceiveAsync returned null");
            if (!rt.IsCanceled)
                Assert.Fail("ReceiveAsync is not cancelled");
        }

        [Test]
        public void closing_the_channel_with_a_waiting_sender_does_not_cancel_the_senders_task()
        {
            var ch = new BufferedChannel<int>(1);
            var st1 = ch.SendAsync(1);
            var st2 = ch.SendAsync(2);
            if (st2 == null)
                Assert.Fail("returned task was null");
            if (st2.IsCompleted)
                Assert.Fail("returned task was complete already!");
            ch.Close();
            if (st2.Wait(100))
                Assert.Fail("task completed, it should just wait");
        }

        [Test]
        public void closing_the_channel_with_a_waiting_receiver_cancel_the_receivers_task()
        {
            var ch = new BufferedChannel<int>(3);
            var rt = ch.ReceiveAsync();
            if (rt == null)
                Assert.Fail("returned task was null");
            if (rt.IsCompleted)
                Assert.Fail("returned task was complete already!");
            ch.Close();
            try
            {
                if (!rt.Wait(100))
                    Assert.Fail("task did not completed");
            }
            catch (AggregateException)
            {
                if (!rt.IsCanceled)
                    Assert.Fail("task is not cancelled");
            }
        }

        [Test]
        public void memory()
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