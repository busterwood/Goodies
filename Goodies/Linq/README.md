# Batched Linq

**Up to 25% Faster execution** of LINQ statements using array-batching of source data.
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

We can see that with a batch size of 100, the batched version is between **9 and 24% faster** than LINQ, 
depending on the size of the input list (N).
Batching was faster than `foreach` in all cases, and was faster and a `for` loop when the input list size was greater than 1000!
```
BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i5-6500 CPU 3.20GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
Frequency=3117183 Hz, Resolution=320.8025 ns, Timer=TSC
  [Host]    : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3110.0
  RyuJitX64 : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3110.0

Job=RyuJitX64  Jit=RyuJit  Platform=X64

  Method |    N |      Mean |     Error |    StdDev | Scaled |  Gen 0 | Allocated |
-------- |----- |----------:|----------:|----------:|-------:|-------:|----------:|
    Linq |  126 | 10.309 us | 0.0262 us | 0.0232 us |   1.00 | 0.1831 |     592 B |
 Foreach |  126 |  9.529 us | 0.0395 us | 0.0309 us |   0.92 | 0.1068 |     376 B |
     For |  126 |  9.093 us | 0.0170 us | 0.0133 us |   0.88 | 0.1068 |     376 B |
 Batched |  126 |  9.404 us | 0.0156 us | 0.0122 us |   0.91 | 0.7935 |    2504 B |
         |      |           |           |           |        |        |           |
    Linq | 1026 | 91.234 us | 0.4703 us | 0.3400 us |   1.00 | 1.3428 |    4529 B |
 Foreach | 1026 | 85.109 us | 0.1343 us | 0.1190 us |   0.93 | 1.3428 |    4313 B |
     For | 1026 | 81.371 us | 0.1475 us | 0.1380 us |   0.89 | 1.3428 |    4313 B |
 Batched | 1026 | 69.673 us | 0.0650 us | 0.0542 us |   0.76 | 1.3428 |    4481 B |

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