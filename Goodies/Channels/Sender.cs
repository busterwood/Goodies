using System.Threading.Tasks;

namespace BusterWood.Channels
{
    class Sender<T> : TaskCompletionSource<bool>, INext<Sender<T>>
    {
        public Sender<T> Next { get; set; } // linked list
        public readonly T Value;

        public Sender(T value)
            : base(TaskCreationOptions.RunContinuationsAsynchronously)
        {
            Value = value;
        }
    }
}
