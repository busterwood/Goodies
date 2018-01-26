[![Build status](https://ci.appveyor.com/api/projects/status/6ecfrrp2q2x0xvxm?svg=true)](https://ci.appveyor.com/project/busterwood/goodies/branch/master)

# BusterWood.Goodies
So much useful & free stuff for .NET, built as a library for .NET Standard 2.0.

### Selected highlights

* [BusterWood.Batching](https://github.com/busterwood/Goodies/blob/master/Goodies/Batching/README.md) namespace contains classes for batching together asynchronous method calls for efficiency.
* [BusterWood.Caching](https://github.com/busterwood/Goodies/blob/master/Goodies/Caching/README.md) namespace contains time and space effecient `Cache<,>` and `ReadThroughCache<,>`.
* [BusterWood.Channels](https://github.com/busterwood/Goodies/blob/master/Goodies/Channels/README.md) namespace contains CSP-like communication between (logical) asynchronous threads, including buffered and multicast channels.
* [BusterWood.Collections](https://github.com/busterwood/Goodies/blob/master/Goodies/Collections/README.md) namespace contains the time and space effecient `UniqueList<>` which implements both `IList<>` and `ISet<>`. Additionally `HashLookup<,>` and `CircularQueue<>` classes are provided.
* [BusterWood.Ducks](https://github.com/busterwood/Goodies/blob/master/Goodies/Ducks/README.md) namespace contains run-time duck typing.
* [BusterWood.Equality](https://github.com/busterwood/Goodies/blob/master/Goodies/Equality/README.md) namespace contains run-time creation of equality comparers using property names.
* [BusterWood.Goodies](https://github.com/busterwood/Goodies/blob/master/Goodies/Goodies/README.md) namespace contains structs to ensure you don't mix up your customer and order identifiers, and extension methods for arrays, enums, TimeSpans and strings.
* `BusterWood.Linq` namespace contains additional LINQ methods and asynchronous enumerables.
* `BusterWood.Logging` namespace contains `Log` static class for structured logging to `Console.Error` (StdErr).
* [BusterWood.Mapping](https://github.com/busterwood/Goodies/blob/master/Goodies/Mapping/README.md) namespace contains extension methods for copying objects with rules for name and type conversion.
* [BusterWood.Monies](https://github.com/busterwood/Goodies/blob/master/Goodies/Monies/README.md) namespace contains `Money` struct to ensure that you don't add accidentally add GBP and USD together.
* `BusterWood.Reflect.Emit` namespace contains extension methods for emiting IL via `ILGenerator`.
* `BusterWood.Restarting` namespace contains `RestartMonitoring` that monitor failures in asychrous processes and restarts them on failure (with delay).
