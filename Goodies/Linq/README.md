# Batched Linq

**Up to 30% Faster execution** of LINQ statements using array-batching of source data.
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
    return _strings.Batched().Where(s => s.Contains("3")).Select(int.Parse).ToList();
}

```

We can see that with a batch size of 80 (the default), the batched version is between **9 and 30% faster** than LINQ, 
depending on the size of the input list (N).
Batching was faster than `foreach` in all cases, and was faster and a `for` loop when the input list size was greater than a few hundred!
```
BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i5-6500 CPU 3.20GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
Frequency=3117183 Hz, Resolution=320.8025 ns, Timer=TSC
  [Host]    : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3110.0
  RyuJitX64 : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3110.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64

  Method |    N |       Mean |     Error |    StdDev | Scaled |  Gen 0 | Allocated |
-------- |----- |-----------:|----------:|----------:|-------:|-------:|----------:|
    Linq |   45 |   4.385 us | 0.0150 us | 0.0117 us |   1.00 | 0.1373 |     440 B |
     For |   45 |   3.782 us | 0.0068 us | 0.0049 us |   0.86 | 0.0687 |     224 B |
 Batched |   45 |   3.977 us | 0.0537 us | 0.0502 us |   0.91 | 0.4730 |    1504 B |
         |      |            |           |           |        |        |           |
    Linq |  159 |  13.654 us | 0.0224 us | 0.0175 us |   1.00 | 0.2747 |     872 B |
     For |  159 |  11.832 us | 0.1898 us | 0.1775 us |   0.87 | 0.1984 |     656 B |
 Batched |  159 |  12.238 us | 0.0208 us | 0.0162 us |   0.90 | 0.7324 |    2336 B |
         |      |            |           |           |        |        |           |
    Linq |  345 |  31.747 us | 0.0625 us | 0.0488 us |   1.00 | 0.4272 |    1408 B |
     For |  345 |  27.847 us | 0.4435 us | 0.4149 us |   0.88 | 0.3662 |    1192 B |
 Batched |  345 |  25.363 us | 0.0349 us | 0.0272 us |   0.80 | 0.7324 |    2336 B |
         |      |            |           |           |        |        |           |
    Linq |  526 |  49.408 us | 0.7290 us | 0.6819 us |   1.00 | 0.7324 |    2456 B |
     For |  526 |  44.735 us | 0.0379 us | 0.0355 us |   0.91 | 0.6714 |    2241 B |
 Batched |  526 |  34.717 us | 0.0467 us | 0.0365 us |   0.70 | 0.9155 |    3001 B |
         |      |            |           |           |        |        |           |
    Linq | 1811 | 163.329 us | 0.1469 us | 0.1374 us |   1.00 | 1.2207 |    4530 B |
     For | 1811 | 145.704 us | 0.3669 us | 0.3064 us |   0.89 | 1.2207 |    4313 B |
 Batched | 1811 | 119.788 us | 0.1696 us | 0.1587 us |   0.73 | 2.0752 |    6889 B |


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