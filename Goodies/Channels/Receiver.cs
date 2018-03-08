using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    class Receiver<T> : TaskCompletionSource<T>, INext<Receiver<T>>
    {
        public Receiver<T> Next { get; set; } // linked list

        public Receiver()
#if !NET452
            : base(TaskCreationOptions.RunContinuationsAsynchronously)
#endif
        {
        }

        public new void SetResult(T result)
        {
#if NET452
            System.Threading.Tasks.Task.Run(() => { base.TrySetResult(result); });
#else
            base.TrySetResult(result);
#endif
        }

        public new void SetCanceled(CancellationToken cancellationToken)
        {
#if NET452
            System.Threading.Tasks.Task.Run(() => { base.TrySetCanceled(); });
#else
            base.TrySetCanceled(cancellationToken);
#endif

        }
    }
}
