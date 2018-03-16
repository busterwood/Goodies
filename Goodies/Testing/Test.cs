using BusterWood.Channels;
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
        internal ConstructorInfo ctor;
        internal readonly List<string> Messsages = new List<string>();
        internal readonly Action<string> LogMessage = Console.WriteLine;
        internal readonly Channel<bool> Finished = new Channel<bool>();

        public Type Type => type;

        /// <summary>returns the name of the running test</summary>
        public string Name => method.Name;

        /// <summary>reports whether the test has failed</summary>
        public bool Failed { get; private set; }

        /// <summary>reports whether the test was skipped</summary>
        public bool Skipped { get; private set; }

        public bool Verbose { get; set; }

        /// <summary>Marks the function as having <see cref="Failed"/> and stops its execution</summary>
        public void Fatal()
        {
            Error();
            throw new FailException();
        }

        /// <summary>Marks the function as having <see cref="Failed"/> but continues execution</summary>
        public void Error()
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
        public void Skip()
        {
            Skipped = true;
            throw new SkipException();
        }

        public override string ToString() => Name;

        internal bool IsAsync => method.ReturnType == typeof(Task);

        internal virtual async Task RunAsync()
        {
            await Task.Yield(); // force async execution

            try
            {
                var instance = CreateInstance();
                using (instance as IDisposable)
                {
                    if (IsAsync)
                        await RunAsync(instance);
                    else
                        Run(instance);
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
            catch (TargetInvocationException e) when (e.InnerException is SkipException)
            {
                Skipped = true;
            }
            catch (TargetInvocationException e) when (e.InnerException is FailException)
            {
                Failed = true;
            }
            catch (Exception ex)
            {
                this.Error(ex.ToString());
            }
            finally
            {
                Finished.Send(true); // signal we have finished
            }
        }

        private object CreateInstance()
        {
            if (method.IsStatic)
                return null;
            return ctor.GetParameters().Length == 0 ? ctor.Invoke(null) : ctor.Invoke(new object[] { this });
        }

        private async Task RunAsync(object instance)
        {
            var testAsync = (Func<Test, Task>)method.CreateDelegate(typeof(Func<Test, Task>), instance);
            await testAsync(this);
        }

        private void Run(object instance)
        {
            var test = (Action<Test>)method.CreateDelegate(typeof(Action<Test>), instance);
            test(this);
        }

    }
}
