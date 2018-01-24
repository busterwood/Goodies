using System.Threading.Tasks;

namespace BusterWood.Channels
{
    /// <summary>
    /// A waiting select
    /// </summary>
    public class Waiter : TaskCompletionSource<bool>, INext<Waiter>
    {
        public Waiter Next { get; set; } // linked list

        public Waiter()
            : base(TaskCreationOptions.RunContinuationsAsynchronously)
        {
        }
    }
}
