using System.Threading.Tasks;

namespace BusterWood.Channels
{
    class Receiver<T> : TaskCompletionSource<T>, INext<Receiver<T>>
    {
        public Receiver<T> Next { get; set; } // linked list

        public Receiver()
             : base(TaskCreationOptions.RunContinuationsAsynchronously)
        {
        }
    }
}
