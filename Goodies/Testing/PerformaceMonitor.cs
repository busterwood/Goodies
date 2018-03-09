using System;
using System.Diagnostics;
using System.Text;

namespace BusterWood.Testing
{
    public class PerformaceMonitor
    {
        readonly Stopwatch sw = new Stopwatch();
        long startingBytes;
        int[] startingCollections;

        public PerformaceMonitor(bool start = false)
        {
            if (start)
                Start();
        }

        public void Start()
        {
            sw.Reset();
            startingCollections = CountCollections();
            startingBytes = GC.GetTotalMemory(true);
            sw.Start();
        }

        private int[] CountCollections()
        {
            var colls = new int[GC.MaxGeneration+1];
            for (int i = 0; i < colls.Length; i++)
            {
                colls[i] = GC.CollectionCount(i);
            }
            return colls;
        }

        public PerformanceResult Stop()
        {
            sw.Stop();
            var allocatedBytes = GC.GetTotalMemory(false) - startingBytes;
            var heldBytes = GC.GetTotalMemory(true) - startingBytes;
            var collections = CountCollections();
            SubstractStartingCollections(collections);
            return new PerformanceResult(collections, allocatedBytes, heldBytes, sw.Elapsed);
        }

        private void SubstractStartingCollections(int[] collections)
        {
            for (int i = 0; i < collections.Length; i++)
            {
                collections[i] -= startingCollections[i];
            }
        }
    }

    public class PerformanceResult
    {
        public int[] Collections { get; }
        public long BytesAllocated { get; }
        public long BytesHeld { get; }
        public TimeSpan Elapsed { get; }

        public PerformanceResult(int[] collections, long bytesAllocated, long bytesHeld, TimeSpan elapsed)
        {
            Collections = collections;
            BytesAllocated = bytesAllocated;
            BytesHeld = bytesHeld;
            Elapsed = elapsed;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Collections.Length; i++)
            {
                sb.Append(", Gen").Append(i).Append(": ").Append(Collections[i]);
            }
            return ($"Took {Elapsed.TotalMilliseconds:N0} ms, allocated {BytesAllocated:N0} bytes, holding {BytesHeld:N0} bytes{sb}");
        }
    }
}