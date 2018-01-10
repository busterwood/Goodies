using System;
using System.Diagnostics;

namespace UnitTests
{
    class PerformaceMonitor
    {
        readonly Stopwatch sw = new Stopwatch();
        long starting;
        long allocated;
        long held;

        public PerformaceMonitor(bool start = false)
        {
            if (start)
                Start();
        }

        public void Start()
        {
            sw.Reset();
            starting = GC.GetTotalMemory(true);
            sw.Start();
        }

        public void Stop()
        {
            sw.Stop();
            allocated = GC.GetTotalMemory(false) - starting;
            held = GC.GetTotalMemory(true) - starting;
        }

        public string Report(int reads, int inCache)
        {
            return ($"Took {sw.ElapsedMilliseconds:N0} ms, {reads} reads, {inCache} items remain in the cache, allocated {allocated:N0} bytes, holding {held:N0} bytes, overhead per item {held / (double)inCache:N2} bytes");
        }
    }
}