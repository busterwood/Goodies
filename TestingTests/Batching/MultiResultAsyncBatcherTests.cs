using BusterWood.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusterWood.Batching
{
    public class MultiResultAsyncBatcherTests
    {
        public static async Task can_query_one_value_in_one_batch(Test t)
        {
            var values = new [] { new KeyValuePair<long, int>(1, 2), new KeyValuePair<long, int>( 1, 3 ) }.ToLookup(pair => pair.Key, pair => pair.Value);
            int calls = 0;
            var b = new AsyncFuncManyBatcher<long, int>(keys => { calls++; return Task.FromResult(values); }, TimeSpan.FromMilliseconds(10));
            IEnumerable<int> result = await b.QueryAsync(1);
            t.Assert(() => Enumerable.SequenceEqual(new[] { 2, 3 }, result));
            t.Assert(1, calls);
        }
        
        public static async Task can_query_many_values_in_one_batch(Test t)
        {
            var values = new Dictionary<long, int> { { 1, 2 }, { 2, 3 } }.ToLookup(pair => pair.Key, pair => pair.Value);
            int calls = 0;
            var b = new AsyncFuncManyBatcher<long, int>(keys => { calls++; return Task.FromResult(values); }, TimeSpan.FromMilliseconds(10));
            var tasks = new[] { b.QueryAsync(1), b.QueryAsync(2) };
            await Task.WhenAll(tasks);
            t.Assert(() => Enumerable.SequenceEqual(new[] { 2 }, tasks[0].Result));
            t.Assert(() => Enumerable.SequenceEqual(new[] { 3 }, tasks[1].Result));
            t.Assert(1, calls);
        }
        
        public static async Task can_query_again_after_first_batch_query(Test t)
        {
            var values = new[] {
                new Dictionary<long, int> { { 1, 2 } }.ToLookup(pair => pair.Key, pair => pair.Value),
                new Dictionary<long, int> { { 2, 3 } }.ToLookup(pair => pair.Key, pair => pair.Value),
            };
            int calls = 0;
            var b = new AsyncFuncManyBatcher<long, int>(keys => Task.FromResult(values[calls++]), TimeSpan.FromMilliseconds(10));
            IEnumerable<int> valuesForKey1 = await b.QueryAsync(1);
            t.Assert(() => Enumerable.SequenceEqual(new[] { 2 }, valuesForKey1));
            IEnumerable<int> valuesForKey2 = await b.QueryAsync(2);
            t.Assert(() => Enumerable.SequenceEqual(new[] { 3 }, valuesForKey2));
            t.Assert(2, calls);
        }
        
        public static void any_exception_caught_by_the_batch_query_is_returned_to_all_query_tasks(Test t)
        {
            var exception = new Exception("whoops");
            var b = new AsyncFuncManyBatcher<long, int>(keys => { throw exception; }, TimeSpan.FromMilliseconds(50));
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
