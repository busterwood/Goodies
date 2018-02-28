# BusterWood.Testing

Small simple library for testing, based on Go's `Testing` package.  No need for test external test runners, just create a console application and put your tests in that.

## How to write tests

Tests are public methods that take a single parameter of type `BusterWood.Testing.Test`, for example:
```
public static void can_send_to_a_buffered_channel(Test t)
{
	...
}
```

Static test methods are run in isolation.

Instance methods are run on a new object instance per test.

### How to set-up a test?

If all tests in a class need common setup code then put this code in the constructor and use non-static test methods.

### How to clean up a test after it has finished?

If the test class implements `IDisposable` then the `Dispose()` method is called after each non-static test method is run.

### How to run setup-code before any test is run?

One-off setup code can be placed in a static constructor, or in the `Main()` method of the program.

## How to run tests

Create a console application and add the following class, which will run all the tests found in the console application assembly:

```
using BusterWood.Testing;

namespace Sample
{
    public class Program
    {
        public static int Main()
        {
            return Tests.Run();
        }
    }
}
```

Note that `Tests.Run()` returns the number of tests that failed, so can be used as the return value of the executable, for easy integration with build tools and scripts.

### Command Line Options

The `Tests.Run()` method recognises the following parameter:
* `--verbose` to list all the test names and all test messages.  Normally only failed test names and messages are shown.
* `--short` which sets the `Test.Short` to TRUE, and can be used by your test code to slow tests.

# Test Class

The `Test` class have the following methods:
* `void Error(string message)` is used to log a message then fail the test (although execution of the test continues)
* `void Fail()` marks the test as having Failed but continues execution
* `void FailNow()` marks the test as having Failed and stops its execution
...