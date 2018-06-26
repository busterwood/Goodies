Batched Linq
============

Faster execution of common LINQ expressions using batching of source data.
Modern CPUs like working with arrays of data, by using arrays we can execute 
faster than Linq, *faster than foreach loops* even though it allocates more 
(garbage is short lived so will tyically be collected in G0).

For example, here is a comparison of doing Where().Select().ToList() over a list of string:

```csharp
[Benchmark]
[Arguments(100)]
public object Batched(int batchSize)
{
    return _strings.Batched(batchSize).Where(s => s.Contains("3")).Select(int.Parse).ToList();
}

[Benchmark]
public object Linq()
{
    return _strings.Where(s => s.Contains("3")).Select(int.Parse).ToList();
}

[Benchmark]
public object Foreach()
{
    var result = new List<int>();
    foreach (var s in _strings)
    {
        if (s.Contains("3"))
            result.Add(int.Parse(s));
    }
    return result;
}

```

We can see that the batched version is consitantly 10%-20% faster depending on the size of the input list (N):
```
BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i5-6500 CPU 3.20GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
Frequency=3117183 Hz, Resolution=320.8025 ns, Timer=TSC
  [Host]    : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3110.0
  RyuJitX64 : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3110.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64

  Method |    N | batchSize |       Mean |     Error |    StdDev |  Gen 0 | Allocated |
-------- |----- |---------- |-----------:|----------:|----------:|-------:|----------:|
 Batched |   25 |       100 |   1.760 us | 0.0345 us | 0.0473 us | 0.4005 |    1264 B |
    Linq |   25 |         ? |   2.136 us | 0.0426 us | 0.0473 us | 0.0916 |     296 B |
 Foreach |   25 |         ? |   1.953 us | 0.0390 us | 0.0640 us | 0.0248 |      80 B |
 Batched |   50 |       100 |   3.231 us | 0.0582 us | 0.0598 us | 0.4616 |    1464 B |
    Linq |   50 |         ? |   5.878 us | 0.0240 us | 0.0213 us | 0.1373 |     440 B |
 Foreach |   50 |         ? |   5.331 us | 0.0271 us | 0.0240 us | 0.0687 |     224 B |
 Batched |  100 |       100 |   7.062 us | 0.0569 us | 0.0532 us | 0.5875 |    1864 B |
    Linq |  100 |         ? |  10.271 us | 0.0949 us | 0.0887 us | 0.1831 |     592 B |
 Foreach |  100 |         ? |   9.525 us | 0.0721 us | 0.0602 us | 0.1068 |     376 B |
 Batched | 1000 |       100 |  84.827 us | 1.0216 us | 0.9556 us | 4.0283 |   13026 B |
    Linq | 1000 |         ? | 111.345 us | 0.4644 us | 0.4344 us | 1.3428 |    4529 B |
 Foreach | 1000 |         ? | 103.134 us | 0.5181 us | 0.4593 us | 1.3428 |    4313 B |


// * Legends *
  N         : Value of the 'N' parameter
  batchSize : Value of the 'batchSize' parameter
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Gen 0     : GC Generation 0 collects per 1k Operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us      : 1 Microsecond (0.000001 sec)
  ```