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

        public new bool TrySetResult(T result)
        {
#if NET452
            System.Threading.Tasks.Task.Run(() => { base.TrySetResult(result); });
            return true; // fake it
#else
            return base.TrySetResult(result);
#endif
        }

        public bool TrySetCanceled(CancellationToken cancellationToken)
        {
#if NET452
            System.Threading.Tasks.Task.Run(() => { base.TrySetCanceled(); });
            return true; // fake it
#else
            return base.TrySetCanceled(cancellationToken);
#endif

        }
    }
}
