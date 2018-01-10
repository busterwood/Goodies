using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusterWood.Batching
{
    [TestFixture]
    [Timeout(2000)]
    public class SingleResultAsyncBatcherTests
    {
        [Test]
        public async Task can_query_one_value_in_one_batch()
        {
            var values = new Dictionary<long, int> { { 1, 2 } };
            int calls = 0;
            var b = new AsyncFuncBatcher<long, int>(keys => { calls++; return Task.FromResult(values); }, TimeSpan.FromMilliseconds(10));
            var result = await b.QueryAsync(1);
            Assert.AreEqual(2, result);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public async Task can_query_many_values_in_one_batch()
        {
            var values = new Dictionary<long, int> { { 1, 2 }, { 2, 3 } };
            int calls = 0;
            var b = new AsyncFuncBatcher<long, int>(keys => { calls++; return Task.FromResult(values); }, TimeSpan.FromMilliseconds(10));
            var tasks = new[] { b.QueryAsync(1), b.QueryAsync(2) };
            await Task.WhenAll(tasks);
            Assert.AreEqual(2, tasks[0].Result);
            Assert.AreEqual(3, tasks[1].Result);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public async Task can_query_again_after_first_batch_query()
        {
            var values = new[] {
                new Dictionary<long, int> { { 1, 2 } },
                new Dictionary<long, int> { { 2, 3 } },
            };
            int calls = 0;
            var b = new AsyncFuncBatcher<long, int>(keys => Task.FromResult(values[calls++]), TimeSpan.FromMilliseconds(10));
            Assert.AreEqual(2, await b.QueryAsync(1));
            Assert.AreEqual(3, await b.QueryAsync(2));
            Assert.AreEqual(2, calls);
        }

        [Test]
        public void any_exception_caught_by_the_batch_query_is_returned_to_all_query_tasks()
        {
            var exception = new Exception("whoops");
            var b = new AsyncFuncBatcher<long, int>(keys => { throw exception; }, TimeSpan.FromMilliseconds(50));
            var tasks = new[] { b.QueryAsync(1), b.QueryAsync(2) };
            foreach (var t in tasks)
            {
                var task = t;
                var ae = Assert.Throws<AggregateException>(() => task.Wait(100));
                Assert.AreSame(exception, ae.InnerException);
            }
        }
    }

}
