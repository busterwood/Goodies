using BusterWood.Channels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
