# Batched Linq

**20% Faster execution** of LINQ statements using array-batching of source data.
Modern CPUs like working with arrays of data, by using arrays we can execute 
faster than Linq, *faster than foreach loops*, **faster that for loops**, even though it allocates more 
(garbage is short lived so will tyically be collected in G0).

## Less virtual method calls

In LINQ uses enumerators, and specifically two virtual method calls `enumerator.MoveNext()` and read `enumerator.Current`.

For example when executing `source.Where().ToList()` for a source of 100 items:
* `Where` will make 2 virtual method calls for each item of source (200 VM calls)
* `ToList` will make 2 virtual method calls for each item returned by `Where` (e.g. 100 VM calls if `Where` filters out 50% of the source)

Using the same example, Batched LINQ will make way less calls (for a batch size of 100):
* `Batched` will make 1 method call to copy the source list into an Array
* `Where` will iterate the array (2 virtual method calls in total)
* `ToList` will iterate the array (2 virtual method calls in total)

## Benchmarks

For example, here is a comparison of doing Where().Select().ToList() over a list of string:

```csharp
List<string> _strings = ....;

[Benchmark(Baseline = true)]
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
    return _strings.Batched(100).Where(s => s.Contains("3")).Select(int.Parse).ToList();
}

```

We can see that with a batch size of 100, the batched version is **15-23% faster** than LINQ, 
depending on the size of the input list (N).
Batching was faster than `foreach` in all cases, and was faster and a `for` loop when the input list size was greater than 100!
```
BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i5-6500 CPU 3.20GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
Frequency=3117183 Hz, Resolution=320.8025 ns, Timer=TSC
  [Host]    : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3110.0
  RyuJitX64 : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3110.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64

  Method |    N |       Mean |     Error |    StdDev | Scaled |  Gen 0 | Allocated |
-------- |----- |-----------:|----------:|----------:|-------:|-------:|----------:|
    Linq |   20 |   1.847 us | 0.0142 us | 0.0133 us |   1.00 | 0.0935 |     296 B |
 Foreach |   20 |   1.574 us | 0.0061 us | 0.0057 us |   0.85 | 0.0248 |      80 B |
     For |   20 |   1.471 us | 0.0104 us | 0.0097 us |   0.80 | 0.0248 |      80 B |
 Batched |   20 |   1.541 us | 0.0072 us | 0.0068 us |   0.83 | 0.1850 |     584 B |
         |      |            |           |           |        |        |           |
    Linq |  120 |  11.058 us | 0.0641 us | 0.0599 us |   1.00 | 0.1831 |     592 B |
 Foreach |  120 |  10.218 us | 0.0438 us | 0.0410 us |   0.92 | 0.1068 |     376 B |
     For |  120 |   9.771 us | 0.0822 us | 0.0769 us |   0.88 | 0.1068 |     376 B |
 Batched |  120 |   8.382 us | 0.0719 us | 0.0673 us |   0.76 | 0.6409 |    2048 B |
         |      |            |           |           |        |        |           |
    Linq | 1020 | 103.987 us | 0.4600 us | 0.4303 us |   1.00 | 1.3428 |    4529 B |
 Foreach | 1020 |  97.301 us | 0.8166 us | 0.7638 us |   0.94 | 1.3428 |    4313 B |
     For | 1020 |  93.435 us | 0.3137 us | 0.2780 us |   0.90 | 1.3428 |    4313 B |
 Batched | 1020 |  79.896 us | 0.7070 us | 0.6613 us |   0.77 | 4.1504 |   13210 B |


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