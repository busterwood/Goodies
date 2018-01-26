# BusterWood.Channels namespace

[CSP-like](https://en.wikipedia.org/wiki/Communicating_sequential_processes) channels for communicating between (logical) asychronous thread.

> As its name suggests, CSP allows the description of systems in terms of component processes that operate independently, 
> and interact with each other solely through message-passing communication.


## `Channel<T>` class 
The `Channel<T>`class is used for sending and receiving values between threads (or logical async threads) and has the following methods:

* `T Receive()` reads a value from the channel, blocking until a sender has sent a value.
* `Task<T> ReceiveAsync()` reads a value from the channel, the returned task will only complete when a sender has written the value to the channel.
* `bool TryReceive(out T)` attempts to read a value from the channel, returns FALSE is no sender is ready.
* `void Send(T)` writes a value to the channel, blocking until a receiver has got the value.
* `Task SendAsync(T)` writes a value to the channel, the returned task will only complete when a receiver has got the value.
* `bool TrySend(T)` attempts to write a value to the channel, returns FALSE is no receiver is ready.
* `void Close()` prevents any further attempts to send to the channel
* `bool IsClosed` has the channel been closed?

## `BufferedChannel<T>` class 
The `BufferedChannel<T>`class has exactly the same interface as the `Channel<T>` class but has a fixed size queue between senders and receivers.

## `MulticastChannel<T>` class 
The `MulticastChannel<T>` class allows a publisher to send a value to multiple destination channels.
Subscribers can call the following methods:
* `Subscribe(IReceiver<T>)` to register a channel to get value published in the future
* `Unsubscribe(IReceiver<T>)` to unregister a channel and stop further values being sent

## `Select` class 
The `Select `class is used for reading from one of many possible channels, has the following methods:

* `Select OnReceive<T>(Channel<T>, Action<T>)` builder method that adds an case to the select that executes a (synchronous) action when the channel is read.
* `Select OnReceiveAsync<T>(Channel<T>, Func<T, Task>)` builder method that adds an case to the select that executes an *asynchronous* action when the channel is read.
* `Task ExecuteAsync()` reads from one (and only one) of the channels and executes the associated action.
* `Task<bool> ExecuteAsync(TimeSpan)` tries to read from one of the channels within the timeout, returns FALSE if no channel could be read within the timeout.

## Closed channels

When `Close()` is called on a `Channel<T>` then:

* any outstanding sends are allowed to complete
* any *new* sends will throw an `OperationCanceledException`
* any outstanding recevies with matching sends are allowed to complete
* any outstanding recevies *without* matching sends will throw an `OperationCanceledException`
* any *new* recevies will throw an `OperationCanceledException`

The `bool TryReceive(out T)` and `bool TrySend(T)` methods will always return `false` when a channel `IsClosed`.
