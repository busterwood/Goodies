using BusterWood.Channels;
using BusterWood.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace BusterWood.Testing
{
    public class Test
    {
        internal Type type;
        internal MethodInfo method;
        internal readonly List<string> Messsages = new List<string>();
        internal readonly Action<string> LogMessage = Console.WriteLine;
        internal readonly Channel<bool> Finished = new Channel<bool>();

        /// <summary>Skip long tests?</summary>
        public static bool Short { get; set; }

        /// <summary>If set then all messages are logged, if not set then messages are only logged for failed tests</summary>
        public static bool Verbose { get; set; }

        static Test()
        {
            var args = Environment.GetCommandLineArgs().ToHashSet(StringComparer.OrdinalIgnoreCase);
            Verbose = args.Contains("--verbose");
            Short = args.Contains("--short");
        }

        public Type Type => type;

        /// <summary>returns the name of the running test</summary>
        public string Name => method.Name;

        /// <summary>reports whether the test has failed</summary>
        public bool Failed { get; private set; }

        /// <summary>reports whether the test was skipped</summary>
        public bool Skipped { get; private set; }

        /// <summary>Marks the function as having <see cref="Failed"/> and stops its execution</summary>
        public void FailNow()
        {
            Fail();
            throw new FailException();
        }

        /// <summary>Marks the function as having <see cref="Failed"/> but continues execution</summary>
        public void Fail()
        {
            Failed = true;
            if (!Verbose) // already logged when in verbose mode
                ReportAllMessages();
        }

        private void ReportAllMessages()
        {
            foreach (var line in Messsages)
            {
                LogMessage(line);
            }
        }

        /// <summary>Records the text in the error log. For tests, the text will be printed only if the test fails or the verbose logging flag is set.</summary>
        public void Log(string message)
        {
            if (Verbose)
                LogMessage(message);
            Messsages.Add(message);
        }

        /// <summary>Marks the test as having been <see cref="Skipped"/> and stops its execution</summary>
        public void SkipNow()
        {
            Skipped = true;
            throw new SkipException();
        }

        public override string ToString() => Name;

        internal bool IsAsync => method.ReturnType == typeof(Task);

        internal async Task RunAsync()
        {
            await Task.Yield(); // force async execution

            try
            {
                if (IsAsync)
                {
                    if (method.IsStatic)
                        await RunStaticAsync();
                    else
                        await RunInstanceAsync();
                }
                else
                {
                    if (method.IsStatic)
                        RunStatic();
                    else
                        RunInstance();
                }
            }
            catch(SkipException)
            {
                Skipped = true;
            }
            catch (FailException)
            {
                Failed = true;
            }
            catch (Exception ex)
            {
                Failed = true;
                Log(ex.ToString());
            }
            finally
            {
                Finished.Send(true); // signal we have finished
            }
        }

        private Task RunStaticAsync()
        {
            var testAsync = (Func<Test, Task>)method.CreateDelegate(typeof(Func<Test, Task>));
            return testAsync(this);
        }

        private async Task RunInstanceAsync()
        {
            var instance = Activator.CreateInstance(type); // create a new instance per test, i.e. setup the test
            var testAsync = (Func<Test, Task>)method.CreateDelegate(typeof(Func<Test, Task>), instance);
            using (instance as IDisposable) // tear down the test, if required
            {
                await testAsync(this);
            }
        }

        private void RunStatic()
        {
            var test = (Action<Test>)method.CreateDelegate(typeof(Action<Test>));
            test(this);
        }

        private void RunInstance()
        {
            var instance = Activator.CreateInstance(type); // create a new instance per test, i.e. setup the test
            var test = (Action<Test>)method.CreateDelegate(typeof(Action<Test>), instance);
            using (instance as IDisposable) // tear down the test, if required
            {
                test(this);
            }
        }

    }
}
