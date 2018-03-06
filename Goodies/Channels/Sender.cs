using System.Threading.Tasks;

namespace BusterWood.Channels
{
    class Sender<T> : TaskCompletionSource<bool>, INext<Sender<T>>
    {
        public Sender<T> Next { get; set; } // linked list
        public readonly T Value;

        public Sender(T value)
#if !NET452
            : base(TaskCreationOptions.RunContinuationsAsynchronously)
#endif
        {
            Value = value;
        }

        public new bool TrySetResult(bool result)
        {
#if NET452
            System.Threading.Tasks.Task.Run(() => { base.TrySetResult(result); });
            return true; // fake it
#else
            return base.TrySetResult(result);
#endif
        }
    }
}
