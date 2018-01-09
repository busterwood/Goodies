using System;
using System.Threading.Tasks;

namespace BusterWood.Channels
{
    public static class Timeout
    {
        public static Channel<DateTime> After(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "must be more than zero");
            var c = new Channel<DateTime>();
            Task.Delay(timeout).ContinueWith(_ => c.SendAsync(DateTime.UtcNow));
            return c;
        }
    }
}
