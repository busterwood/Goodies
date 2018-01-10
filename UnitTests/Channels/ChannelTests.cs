using BusterWood.Channels;
using NUnit.Framework;
using System;

namespace UnitTests
{
    [TestFixture]
    public class ChannelTests
    {
        [Test]
        public void trying_to_receive_from_an_empty_channel_returns_false()
        {
            var ch = new Channel<int>();
            int val;
            if (ch.TryReceive(out val) != false)
                Assert.Fail("returned true receiving from a new (empty) channel");
        }

        [Test]
        public void trying_to_send_to_an_empty_channel_returns_false()
        {
            var ch = new Channel<int>();
            if (ch.TrySend(2) != false)
                Assert.Fail("returned true sending to an new (empty) channel");
        }

        [Test]
        public void sending_async_returns_a_waiting_task()
        {
            var ch = new Channel<int>();
            var t = ch.SendAsync(2);
            if (t == null)
                Assert.Fail("returned task was null");
            if (t.IsCompleted)
                Assert.Fail("returned task was complete");
        }

        [Test]
        public void receiving_async_returns_a_waiting_task()
        {
            var ch = new Channel<int>();
            var t = ch.ReceiveAsync();
            if (t == null)
                Assert.Fail("returned task was null");
            if (t.IsCompleted)
                Assert.Fail("returned task was complete");
        }

        [Test]
        public void receiving_async_returns_a_waiting_task_which_is_complete_by_trying_to_send_a_value()
        {
            var ch = new Channel<int>();
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
            var ch = new Channel<int>();
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
            var ch = new Channel<int>();
            var st = ch.SendAsync(2);
            if (st == null)
                Assert.Fail("returned task was null");
            if (st.IsCompleted)
                Assert.Fail("returned task was complete");
            int val;
            if (ch.TryReceive(out val) != true)
                Assert.Fail("TryReceive failed when a receiver was waiting");
            if (st.Wait(100) == false || st.IsCompleted == false)
                Assert.Fail("sending task did not complete, state is " + st.Status);
            if (val != 2)
                Assert.Fail("received value (" + val + ") is not what we sent (2)");
        }

        [Test]
        public void sending_async_returns_a_waiting_task_which_is_complete_by_receive_a_value_async()
        {
            var ch = new Channel<int>();
            var st = ch.SendAsync(2);
            if (st == null)
                Assert.Fail("returned task was null");
            if (st.IsCompleted)
                Assert.Fail("returned task was complete");
            var rt = ch.ReceiveAsync();
            if (rt == null)
                Assert.Fail("ReceiveAsync returned null");
            if (!rt.IsCompleted)
                Assert.Fail("ReceiveAsync is not complete");
            if (st.Wait(100) == false || st.IsCompleted == false)
                Assert.Fail("sending task did not complete, state is " + st.Status);
            if (rt.Result != 2)
                Assert.Fail("received value (" + rt.Result + ") is not what we sent (2)");
        }

        [Test]
        public void sendAsync_on_a_closed_channel_returns_a_cancelled_task()
        {
            var ch = new Channel<int>();
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
            var ch = new Channel<int>();
            ch.Close();
            if (ch.TrySend(2) != false)
                Assert.Fail("was able to send on closed channel");
        }

        [Test]
        public void try_receive_on_a_closed_channel_return_false()
        {
            var ch = new Channel<int>();
            ch.Close();
            int val;
            if (ch.TryReceive(out val) != false)
                Assert.Fail("was able to receive on closed channel");
        }

        [Test]
        public void ReceiveAsync_on_a_closed_channel_return_cancelled_task()
        {
            var ch = new Channel<int>();
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
            var ch = new Channel<int>();
            var st = ch.SendAsync(2);
            if (st == null)
                Assert.Fail("returned task was null");
            if (st.IsCompleted)
                Assert.Fail("returned task was complete already!");
            ch.Close();
            if (st.Wait(100))
                Assert.Fail("task completed, it should just wait");
        }

        [Test]
        public void closing_the_channel_with_a_waiting_receiver_cancel_the_receivers_task()
        {
            var ch = new Channel<int>();
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
            var chans = new Channel<int>[10000];
            for (int i = 0; i < chans.Length; i++)
            {
                chans[i] = new Channel<int>();
            }
            var after = GC.GetTotalMemory(true);
            GC.KeepAlive(chans);
            var diff = after - before - (chans.Length * 4);
            var kb = diff / 1024;
            Console.WriteLine("10,000 changes takes " + kb + "KB");
        }
    }
}