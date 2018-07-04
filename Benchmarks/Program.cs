using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using BusterWood.Linq;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Linq
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            var b = new WhereToListBenchmark() { N = 126 };
            b.Setup();
            var res = b.Batched();
#else
            BenchmarkRunner.Run<WhereToListBenchmark>();
#endif
        }
    }

    //[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
    [RyuJitX64Job, /*LegacyJitX86Job,*/ MemoryDiagnoser]
    public class WhereToListBenchmark
    {
        List<string> _strings;

        [Params(45, 159, 345, 526, 1811)]
        public int N { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _strings = new List<string>(N);
            for (int i = 0; i < N; i++)
            {
                _strings.Add(i.ToString());
            }
        }

        [Benchmark(Baseline = true)]
        public object Linq()
        {
            return _strings.Where(s => s.Contains("3")).Select(int.Parse).ToList();
        }

        //[Benchmark]
        //public object Foreach()
        //{
        //    var result = new List<int>();
        //    foreach (var s in _strings)
        //    {
        //        if (s.Contains("3"))
        //            result.Add(int.Parse(s));
        //    }
        //    return result;
        //}

        [Benchmark]
        public object For()
        {
            var result = new List<int>();
            for (int i = 0; i < _strings.Count; i++)
            {
                if (_strings[i].Contains("3"))
                    result.Add(int.Parse(_strings[i]));
            }
            return result;
        }

        [Benchmark]
        public object Batched()
        {
            return _strings.Batched().Where(s => s.Contains("3")).Select(int.Parse).ToList();
        }
    }

    [RyuJitX64Job, /*LegacyJitX86Job,*/ MemoryDiagnoser]
    public class AggregateBenchmark
    {
        List<int> values;

        [Params(126, 1062)]
        public int N { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            values = new List<int>(N);
            for (int i = 0; i < N; i++)
            {
                values.Add(i);
            }
        }

        [Benchmark(Baseline =true)]
        public int Foreach()
        {
            var result = 0;
            foreach (var v in values)
            {
                result += v;
            }
            return result;
        }

        [Benchmark]
        public int Linq()
        {
            return values.Aggregate(0, (v, acc) => v + acc);
        }

        [Benchmark]
        public int Batched()
        {
            return values.Batched().Aggregate(0, (v, acc) => v + acc);
        }

        [Benchmark]
        public int For()
        {
            var result = 0;
            for (int i = 0; i < values.Count; i++)
            {
                result += values[i];

            }
            return result;
        }

    }
}
