using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    class Tasks
    {
#if NET452
        public static readonly Task CompletedTask = Task.FromResult(true);
#else
        public static readonly Task CompletedTask = Task.CompletedTask;
#endif

        public static Task<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
        {
#if NET452
            var tcs = new TaskCompletionSource<TResult>();
            tcs.TrySetCanceled();
            return tcs.Task;
#else
            return Task.FromCanceled<TResult>(cancellationToken);
#endif
        }

        public static Task FromCanceled(CancellationToken cancellationToken)
        {
#if NET452
            var tcs = new TaskCompletionSource<object>();
            tcs.TrySetCanceled();
            return tcs.Task;
#else
            return Task.FromCanceled(cancellationToken);
#endif
        }
    }
}
