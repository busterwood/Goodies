# BusterWood.Collections

## UniqueList<T>

`UniqueList` is a generic collection that is both a set and a list:
* implements `ISet<T>` and has hash table like performance for `Contains` and `IndexOf` operations
* implements `IList<T>` and gives constant time access to items via index
* stores values in an array, so enumeration yields items in list order, i.e. *in the order they were added*
* does not allow null to be added - calling `Add()` with a null value always returns FALSE and does not add to the underlying list of values

The design of `UniqueList` was inspired by [Python 3.6's new dict ](https://mail.python.org/pipermail/python-dev/2012-December/123028.html) which uses hashcode to access an array of indexes which gives an index into the array of values.

### Add Performance

`Add` performance is similar to a adding to a `HashSet<T>` and not that much slower than `List<T>`, for example adding a number strings:
```
BenchmarkDotNet=v0.10.8, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=AMD A4-6210 APU with AMD Radeon R3 Graphics, ProcessorCount=4
Frequency=1754521 Hz, Resolution=569.9561 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 64bit RyuJIT-v4.7.2101.1
  DefaultJob : Clr 4.0.30319.42000, 64bit RyuJIT-v4.7.2101.1


     Method | Size |        Mean |       Error |      StdDev |
----------- |----- |------------:|------------:|------------:|
 UniqueList |   50 |    29.79 us |   0.5882 us |   0.5777 us |
  ArrayList |   50 |    22.06 us |   0.4055 us |   0.3793 us |
    HashSet |   50 |    28.81 us |   0.5722 us |   0.5353 us |
 UniqueList |  500 |   327.05 us |   6.4500 us |  11.1260 us |
  ArrayList |  500 |   234.22 us |   4.6396 us |   5.6979 us |
    HashSet |  500 |   304.02 us |   5.9085 us |   9.3715 us |
 UniqueList | 5000 | 3,350.63 us |  70.5560 us |  58.9174 us |
  ArrayList | 5000 | 2,478.81 us |  56.2821 us |  64.8145 us |
    HashSet | 5000 | 5,107.88 us | 100.1957 us | 143.6977 us |
```
### Contains / IndexOf performance

`Contains` performance is similar to a `HashSet<T>` and *much* faster than `List<T>`, for example lookup up all the strings in a list or set of different sizes:

```
BenchmarkDotNet=v0.10.8, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=AMD A4-6210 APU with AMD Radeon R3 Graphics, ProcessorCount=4
Frequency=1754521 Hz, Resolution=569.9561 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 64bit RyuJIT-v4.7.2101.1
  DefaultJob : Clr 4.0.30319.42000, 64bit RyuJIT-v4.7.2101.1


     Method | Size |          Mean |         Error |        StdDev |
----------- |----- |--------------:|--------------:|--------------:|
 UniqueList |   50 |      23.60 us |     0.4546 us |     0.4668 us |
  ArrayList |   50 |      56.86 us |     1.1189 us |     1.6401 us |
    HashSet |   50 |      24.01 us |     0.4411 us |     0.4126 us |
 UniqueList |  500 |     243.04 us |     4.6906 us |     5.0189 us |
  ArrayList |  500 |   3,697.55 us |    73.0742 us |    68.3536 us |
    HashSet |  500 |     246.22 us |     4.4495 us |     3.7155 us |
 UniqueList | 5000 |   2,716.73 us |    52.8352 us |    56.5331 us |
  ArrayList | 5000 | 350,595.46 us | 6,076.2343 us | 5,386.4229 us |
    HashSet | 5000 |   2,750.42 us |    16.3910 us |    11.8517 us |
```

### Memory usage

`UniqueList<T>` uses more memory than `List<T>` but less than `HashSet<T>`, for example on a x64 system, where T is `string`:

```
Data structure | Size | Memory Held |
-------------- |----- |------------ |  
UniqueList     |   50 |       1.0 K |
 ArrayList     |   50 |       0.6 K |
   HashSet     |   50 |       1.7 K |
UniqueList     |  500 |       8.6 K |
 ArrayList     |  500 |       4.5 K |
   HashSet     |  500 |      18.3 K |
UniqueList     | 5000 |       139 K |
 ArrayList     | 5000 |        73 K |
   HashSet     | 5000 |       168 K |
```
