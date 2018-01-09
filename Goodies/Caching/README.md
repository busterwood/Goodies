# Caching
A low memory overhead read-through cache implemented using generations.

The design is layered so you can choose the functionality that you require:
* `Cache<TKey, TValue>` is a thread-safe generational cache-aside class that you populated with data.
* `ConcurrentCache<TKey, TValue>` extends `Cache<TKey, TValue>`to support concurrent reading and writing
* `ReadThroughCache<TKey, TValue>` is a thread-safe generational cache that populates itself from an underlying data source.
* `BatchReadThroughCache<TKey, TValue>` extends `ReadThroughCache<TKey, TValue>` adding support for reading multiple values for multiple keys in one call.
* `ThunderingHerdProtection<TKey, TValue>` prevents concurrent read-through for the same key (see below)
* `BatchThunderingHerdProtection<TKey, TValue>` prevents concurrent read-through for the multiple keys (see below)

## Usage

All caches implementes the basic the `ICache<TKey, TValue>` interface, which has methods such as:
* `TValue this[TKey] { get; set; }` sets or tries to get a value for a key (returns `default(TValue)` is not found, i.e. null)
* `Task<TValue> GetAsync(TKey key)` tries to get a value for a key asynchronusly
* `int Count()` returns the number of items currently in the cache
* `void Clear()` removes all items from the cache
* `void Remove(TKey key)` evicts a specific key from the cache

Batch loading classes implement the `IBatchCache<TKey, TValue>` interface, which extends `ICache<TKey, TValue>` adding the following methods:
* `TValue[] GetBatch(IReadOnlyCollection<TKey> keys)` tries to get the values associated with some keys
* `Task<TValue[]> GetBatchAsync(IReadOnlyCollection<TKey> keys)` tries to get the values associated with the keys asynchronusly

You can also compose caches with the following extension methods:
* `WithGenerationalCache(int? gen0Limit, TimeSpan? timeToLive)` create a new read-through cache that has a Gen0 size limit and/or a periodic collection time
* `WithThunderingHerdProtection()` adds ThunderingHerdProtection to a cache which prevents calling the data source concurrently *for the same key*.

## Thundering Herd Protection

`ThunderingHerdProtection<TKey, TValue>` can be used to prevent multiple threads calling an underlying database, or remote service, to load the value for the *same* key.  
Different keys are handled concurrently, but indiviual keys are read by only one thread.  This can be used on the client side, before calling a remote service or database, 
and it can be used on the the server side.

## How does it work?

The design is insipred by "generational garbage collection" in that:

* the cache as two generations `Gen0` and `Gen1`
* as items are read from the underlying data source they are added to `Gen0`
* when a collection happens `Gen1` is thrown away and `Gen0` is moved to `Gen1`
* when an item is read from `Gen1` it is promted back to `Gen0`

Internally, `ReadThroughCache<TKey, TValue>` uses a lock (Monitor) to protect it's data structures, but the lock is released if a read to the underlying data source is required.

### When does a collection happen?

The `ReadThroughCache<TKey, TValue>` contructor takes two arguments that control collection:

* if a `gen0Limit` is set then a collection will occur when `Gen0` reaches that limit
* if `timeToLive` is set then a an un-read entry will be evicted some time after this (unless the `gen0Limit` was reached since the last collection)

One or both parameters neeed to be set, i.e.

* you can just specify a `gen0Limit`, but then an the cache will never be cleared, even if it is not used again for a long time
* you can just specify a `timeToLive` which will let the cache grow to any size but will ensure items not used for "a long time" are evicted
* you can specify both `gen0Limit` and `halfLife` to combine the attributes of both

### What is cached?

`ReadThroughCache<TKey, TValue>` remembers the results of all read-though operations, so it records the value for a key or it records that a *key does not have a value*.

### Performance and Memory

The following tests compare a generation cache with a Bit-Pseduo LRU cache.

#### 4 threads reading-through a total of 100,000 items
| Test | Elapsed | Items in cache | Bytes allocated | Bytes held | Bytes held per key |
| ---- | ------- | -------------- | --------------- | ---------- | ------------------ |
| BitPseudoLru 50% | 118 ms | 50,000 | 2,973,144 | 1,519,864 | 30.40 |
| Generational 25% Gen0 Limit| 34 ms | 33,332 | 3,782,700 | 895,764 | 26.87 |
| Generational 50ms Half-life | 25 ms | 100,000 | 6,050,996 | 3,129,228 | 31.29 |
| Concurrent Dictionary | 11 ms | 100,000 | 5,651,692 | 2,994,008 | 29.94 |

#### 4 threads reading-through a total of 500,000 items
| Test | Elapsed | Items in cache | Bytes allocated | Bytes held | Bytes held per key |
| ---- | ------- | -------------- | --------------- | ---------- | ------------------ |
| BitPseudoLru 50% | 2,062 ms | 250,000 | 9,686,520 | 6,529,452 | 26.12 |
| Generational 25% Gen0 Limit | 183 ms | 166,664 | 11,926,556 | 4,637,388 | 27.82 |
| Generational 50ms Half-life | 159 ms | 331,819 | 31,131,248 | 9,617,896 | 28.99 |
| Concurrent Dictionary | 205 ms | 500,000 | 37,960,620 | 16,607,692 | 33.22 |

