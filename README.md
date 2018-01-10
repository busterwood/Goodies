[![Build status](https://ci.appveyor.com/api/projects/status/6ecfrrp2q2x0xvxm?svg=true)](https://ci.appveyor.com/project/busterwood/goodies/branch/master)

# BusterWood.Goodies
So much useful & free stuff for .NET, built as a library for .NET Standard 2.0.

### Selected highlights

* [BusterWood.Batching](https://github.com/busterwood/Goodies/blob/master/Goodies/Batching/README.md) namespace contains classes for batching together asynchronous method calls for efficiency.
* [BusterWood.Caching](https://github.com/busterwood/Goodies/blob/master/Goodies/Caching/README.md) namespace contains time and space effecient `Cache<,>` and `ReadThroughCache<,>`.
* [BusterWood.Channels](https://github.com/busterwood/Goodies/blob/master/Goodies/Channels/README.md) namespace contains CSP-like communication between (logical) asynchronous threads.
* [BusterWood.Collections](https://github.com/busterwood/Goodies/blob/master/Goodies/Collections/README.md) namespace contains the time and space effecient `UniqueList` which implements both `IList<>` and `ISet<>`.
* [BusterWood.Ducks](https://github.com/busterwood/Goodies/blob/master/Goodies/Ducks/README.md) namespace contains run-time duck typing.
* [BusterWood.Equality](https://github.com/busterwood/Goodies/blob/master/Goodies/Equality/README.md) namespace contains run-time creation of equality comparers using property names.
* `BusterWood.Goodies` namespace contains `Int32<>`, `Int64<>` and `Guid<>` structs to ensure you don't mix up your customer and order identifiers.
* `BusterWood.Logging` namespace contains `Log` static class for structured logging to `Console.Error` (StdErr).
* [BusterWood.Monies](https://github.com/busterwood/Goodies/blob/master/Goodies/Monies/README.md) namespace contains `Money` struct to ensure that you don't add acciendtly add GBP and USD together.
* `BusterWood.Restarting` namespace contains `RestartMonitoring` that monitor failures in asychrous processes and restart them on failure (with delay).
