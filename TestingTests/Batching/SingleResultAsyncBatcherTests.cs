using BusterWood.Testing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusterWood.Batching
{
    public class SingleResultAsyncBatcherTests
    {
        public async Task can_query_one_value_in_one_batch(Test t)
        {
            var values = new Dictionary<long, int> { { 1, 2 } };
            int calls = 0;
            var b = new AsyncFuncBatcher<long, int>(keys => { calls++; return Task.FromResult(values); }, TimeSpan.FromMilliseconds(10));
            var result = await b.QueryAsync(1);
            t.Assert(2, result);
            t.Assert(1, calls);
        }

        public async Task can_query_many_values_in_one_batch(Test t)
        {
            var values = new Dictionary<long, int> { { 1, 2 }, { 2, 3 } };
            int calls = 0;
            var b = new AsyncFuncBatcher<long, int>(keys => { calls++; return Task.FromResult(values); }, TimeSpan.FromMilliseconds(10));
            var tasks = new[] { b.QueryAsync(1), b.QueryAsync(2) };
            await Task.WhenAll(tasks);
            t.Assert(2, tasks[0].Result);
            t.Assert(3, tasks[1].Result);
            t.Assert(1, calls);
        }

        public async Task can_query_again_after_first_batch_query(Test t)
        {
            var values = new[] {
                new Dictionary<long, int> { { 1, 2 } },
                new Dictionary<long, int> { { 2, 3 } },
            };
            int calls = 0;
            var b = new AsyncFuncBatcher<long, int>(keys => Task.FromResult(values[calls++]), TimeSpan.FromMilliseconds(10));
            int valuesforKey1 = await b.QueryAsync(1);
            t.Assert(2, valuesforKey1);
            int valuesforKey2 = await b.QueryAsync(2);
            t.Assert(3, valuesforKey2);
            t.Assert(2, calls);
        }

        public void any_exception_caught_by_the_batch_query_is_returned_to_all_query_tasks(Test t)
        {
            var exception = new Exception("whoops");
            var b = new AsyncFuncBatcher<long, int>(keys => { throw exception; }, TimeSpan.FromMilliseconds(50));
            var tasks = new[] { b.QueryAsync(1), b.QueryAsync(2) };
            foreach (var ta in tasks)
            {
                var task = ta;
                var ae = t.AssertThrows<AggregateException>(() => task.Wait(100));
                t.Assert(() => ReferenceEquals(exception, ae.InnerException));
            }
        }
    }

}
