using BusterWood.Testing;
using System;

namespace BusterWood.Channels
{
    public class TimeoutTests
    {
        public static void can_do_something(Test t)
        {
            var chan = Timeout.After(TimeSpan.FromMilliseconds(10));
            var got = chan.Receive();
            t.Assert(() => DateTime.UtcNow >= got);
        }
    }
}
