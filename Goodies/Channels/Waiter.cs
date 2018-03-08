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
#if !NET452
            : base(TaskCreationOptions.RunContinuationsAsynchronously)
#endif
        {
        }

        public new void SetResult(bool result)
        {
#if NET452
            System.Threading.Tasks.Task.Run(() => { base.TrySetResult(result); });
#else
            base.TrySetResult(result);
#endif
        }
    }
}
