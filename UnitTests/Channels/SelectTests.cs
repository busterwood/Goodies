using BusterWood.Channels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class SelectTests
    {
        [Test]
        public void executing_a_select_when_one_channel_is_ready_to_send_completes_immediately()
        {
            var ch1 = new Channel<int>();
            var ch2 = new Channel<int>();

            var got1 = new TaskCompletionSource<int>();
            var got2 = new TaskCompletionSource<int>();
            var select = new Select()
                .OnReceive(ch1, val => got1.TrySetResult(val))
                .OnReceive(ch2, val => got2.TrySetResult(val));

            var sendTask = ch2.SendAsync(2);

            var selTask = select.ExecuteAsync();
            if (!selTask.IsCompleted)
                Assert.Fail("Expected select to wait, status = " + selTask.Status);

            if (!got2.Task.Wait(100))
                Assert.Fail("did not get the value to the TCS");

            if (got2.Task.Result != 2)
                Assert.Fail("Expected ch2 to get 2 but got " + got2.Task.Result);

            if (!selTask.IsCompleted)
                Assert.Fail("Expected select to be compelte");

            if (!sendTask.IsCompleted)
                Assert.Fail("Expected select to be compelte");

            if (got1.Task.Wait(50))
                Assert.Fail("did not expect value in ch1");
        }

        [Test]
        public void executing_a_select_waits_for_one_case_to_succeed()
        {
            var ch1 = new Channel<int>();
            var ch2 = new Channel<int>();

            var got1 = new TaskCompletionSource<int>();
            var got2 = new TaskCompletionSource<int>();
            var select = new Select()
                .OnReceive(ch1, val => got1.TrySetResult(val))
                .OnReceive(ch2, val => got2.TrySetResult(val));

            var st = select.ExecuteAsync();
            if (st.IsCompleted)
                Assert.Fail("Expected select to wait, status = " + st.Status);

            while ((st.Status & TaskStatus.WaitingForActivation) != TaskStatus.WaitingForActivation)
                Thread.Sleep(10);

            Thread.Sleep(10);

            if (!ch1.SendAsync(2).Wait(200))
                Assert.Fail("Failed to SendAsync");

            if (!got1.Task.Wait(200))
                Assert.Fail("did not get the value to the TCS");

            if (!st.Wait(100) || !st.IsCompleted)
                Assert.Fail($"Expected select to be complete, but select is in {st.Status}");

            if (got2.Task.Wait(80))
                Assert.Fail("did not expect value in ch2");
        }

        [Test]
        public void if_more_that_once_case_is_ready_then_the_first_one_added_is_chose()
        {
            var ch1 = new Channel<int>();
            var ch2 = new Channel<int>();

            var got1 = new TaskCompletionSource<int>();
            var got2 = new TaskCompletionSource<int>();
            var select = new Select()
                .OnReceive(ch1, val => got1.TrySetResult(val))
                .OnReceive(ch2, val => got2.TrySetResult(val));

            var sendTask1 = ch1.SendAsync(1);
            var sendTask2 = ch2.SendAsync(2);

            var selTask = select.ExecuteAsync();
            if (!selTask.IsCompleted)
                Assert.Fail("Expected select to wait, status = " + selTask.Status);

            if (!got1.Task.Wait(100))
                Assert.Fail("did not get the value to the TCS");

            if (got1.Task.Result != 1)
                Assert.Fail("Expected ch1 to get 1 but got " + got1.Task.Result);

            if (!selTask.IsCompleted)
                Assert.Fail("Expected select to be compelte");

            if (!sendTask1.IsCompleted)
                Assert.Fail("Expected select to be compelte");

            if (got2.Task.Wait(50))
                Assert.Fail("did not expect value in ch2");

            if (sendTask2.IsCompleted)
                Assert.Fail("Expected send 2 to be waiting");
        }
    }
}
