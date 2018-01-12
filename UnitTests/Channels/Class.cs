using BusterWood.Channels;
using NUnit.Framework;
using System;

namespace UnitTests
{
    public class TimeoutTests
    {
        [Test, NUnit.Framework.Timeout(500)]
        public void can_do_something()
        {
            var chan = Timeout.After(TimeSpan.FromMilliseconds(10));
            var got = chan.Receive();
            Assert.IsTrue(DateTime.UtcNow >= got);
        }
    }
}
