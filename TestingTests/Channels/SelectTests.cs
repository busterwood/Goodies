using BusterWood.Testing;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    public class SelectTests
    {        
        public static void executing_a_select_when_one_channel_is_ready_to_send_completes_immediately(Test t)
        {
            if (Test.Short)
                t.SkipNow();

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
                t.Fatal("Expected select to wait, status = " + selTask.Status);

            if (!got2.Task.Wait(100))
                t.Fatal("did not get the value to the TCS");

            if (got2.Task.Result != 2)
                t.Fatal("Expected ch2 to get 2 but got " + got2.Task.Result);

            if (!selTask.IsCompleted)
                t.Fatal("Expected select to be compelte");

            if (!sendTask.IsCompleted)
                t.Fatal("Expected select to be compelte");

            if (got1.Task.Wait(50))
                t.Fatal("did not expect value in ch1");
        }
        
        public static void executing_a_select_waits_for_one_case_to_succeed(Test t)
        {
            if (Test.Short)
                t.SkipNow();

            var ch1 = new Channel<int>();
            var ch2 = new Channel<int>();

            var got1 = new TaskCompletionSource<int>();
            var got2 = new TaskCompletionSource<int>();
            var select = new Select()
                .OnReceive(ch1, val => got1.TrySetResult(val))
                .OnReceive(ch2, val => got2.TrySetResult(val));

            var st = select.ExecuteAsync();
            if (st.IsCompleted)
                t.Fatal("Expected select to wait, status = " + st.Status);

            while ((st.Status & TaskStatus.WaitingForActivation) != TaskStatus.WaitingForActivation)
                Thread.Sleep(10);

            Thread.Sleep(10);

            if (!ch1.SendAsync(2).Wait(200))
                t.Fatal("Failed to SendAsync");

            if (!got1.Task.Wait(200))
                t.Fatal("did not get the value to the TCS");

            if (!st.Wait(100) || !st.IsCompleted)
                t.Fatal($"Expected select to be complete, but select is in {st.Status}");

            if (got2.Task.Wait(80))
                t.Fatal("did not expect value in ch2");
        }
        
        public static void if_more_that_once_case_is_ready_then_the_first_one_added_is_chose(Test t)
        {
            if (Test.Short)
                t.SkipNow();

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
                t.Fatal("Expected select to wait, status = " + selTask.Status);

            if (!got1.Task.Wait(100))
                t.Fatal("did not get the value to the TCS");

            if (got1.Task.Result != 1)
                t.Fatal("Expected ch1 to get 1 but got " + got1.Task.Result);

            if (!selTask.IsCompleted)
                t.Fatal("Expected select to be compelte");

            if (!sendTask1.IsCompleted)
                t.Fatal("Expected select to be compelte");

            if (got2.Task.Wait(50))
                t.Fatal("did not expect value in ch2");

            if (sendTask2.IsCompleted)
                t.Fatal("Expected send 2 to be waiting");
        }
    }
}
