using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Testing
{
    public class Test
    {
        internal Action<Test> method;

        internal readonly List<string> Messsages = new List<string>();

        internal readonly Action<string> LogMessage = Console.WriteLine;

        internal void Run() => method(this);

        public string TypeName { get; internal set; }

        /// <summary>returns the name of the running test</summary>
        public string Name { get; internal set; }

        /// <summary>reports whether the test has failed</summary>
        public bool Failed { get; private set; }

        /// <summary>reports whether the test was skipped</summary>
        public bool Skipped { get; private set; }

        public void Error(string message)
        {
            Log(message);
            Fail();
        }

        /// <summary>Fail marks the function as having <see cref="Failed"/> but continues execution</summary>
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

        /// <summary>FailNow marks the function as having <see cref="Failed"/> and stops its execution</summary>
        public void FailNow()
        {
            Fail();
            throw new FailException();
        }

        /// <summary>Fatal is equivalent to <see cref="Log"/> followed by <see cref="FailNow"/></summary>
        public void Fatal(string message)
        {
            Log(message);
            FailNow();
        }

        /// <summary>Records the text in the error log. For tests, the text will be printed only if the test fails or the verbose logging flag is set.</summary>
        public void Log(string message)
        {
            if (Verbose)
                LogMessage(message);
            Messsages.Add(message);
        }

        /// <summary>Skip is equivalent to <see cref="Log"/> followed by <see cref="SkipNow"/></summary>
        public void Skip(string message)
        {
            Log(message);
            SkipNow();
        }

        /// <summary>Marks the test as having been <see cref="Skipped"/> and stops its execution</summary>
        public void SkipNow()
        {
            Skipped = true;
            throw new SkipException();
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>Skip log tests?</summary>
        public static bool Short { get; set; }

        /// <summary>If set then all messages are logged, if not set then messages are only logged for failed tests</summary>
        public static bool Verbose { get; set; }
    }

    public static class Extensions
    {
        /// <summary>Checks the expression returns true, or reports the expression an an error</summary>
        public static void Assert(this Test t, Expression<Func<bool>> expression)
        {
            var func = expression.Compile();
            if (!func())
                t.Error(expression.ToString());
        }

        /// <summary>Checks the expression returns true, or reports the expression an an error</summary>
        public static void AssertFalse(this Test t, Expression<Func<bool>> expression)
        {
            var func = expression.Compile();
            if (func())
                t.Error(expression.ToString());
        }
    }

    public static class Tests
    {
        public static void Run(Assembly assembly)
        {
            foreach (var t in assembly.GetExportedTypes())
            {
                Run(t);
            }
        }

        public static int Run(Type type)
        {
            var tests = DiscoverTests(type).ToList();
            bool allOkay = true;
            if (tests.Any())
            {
                var sw = new Stopwatch();
                sw.Start();
                bool ok = Run(tests);
                sw.Stop();
                if (ok)
                    WriteLine(ConsoleColor.Green, $"ok\t{type.Name}\t{sw.ElapsedMilliseconds:N0}MS");
                else
                    WriteLine(ConsoleColor.Red, $"FAILED \t{type.Name}\t{sw.ElapsedMilliseconds:N0}MS");
                allOkay |= ok;
            }
            return allOkay ? 0 : 1;
        }

        public static IEnumerable<Test> DiscoverTests(Type type)
        {
            object instance = null;
            foreach (var m in type.GetMethods().Where(mi => mi.Name.StartsWith("test", StringComparison.OrdinalIgnoreCase)))
            {
                var p = m.GetParameters();
                if (p.Length != 1 || p[0].ParameterType != typeof(Test))
                    continue;
                if (instance == null)
                    instance = Activator.CreateInstance(type);
                var test = (Action<Test>)m.CreateDelegate(typeof(Action<Test>), instance);
                yield return new Test { method = test,  TypeName = type.Name, Name = m.Name };
            }
        }

        public static bool Run(IEnumerable<Test> tests)
        {
            int failed = 0;
            using (var finished = new AutoResetEvent(false))
            {
                foreach (var t in tests)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            t.Run();
                        }
                        catch (SkipException)
                        {
                        }
                        catch (FailException)
                        {
                        }
                        catch (Exception ex)
                        {
                            t.Log(ex.ToString());
                            t.Fail();
                        }
                        finally
                        {
                            finished.Set();
                        }
                    });

                    finished.WaitOne();

                    if (t.Failed)
                    {
                        WriteLine(ConsoleColor.Red, "Failed " + t);
                        failed++;
                    }
                    else if (t.Skipped)
                    {
                        WriteLine(ConsoleColor.Cyan, "Skipped " + t);
                    }
                    else if (Test.Verbose)
                    {
                        Console.WriteLine(t.ToString());
                    }
                }
            }
            return failed == 0;
        }

        static void WriteLine(ConsoleColor color, string message)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = before;
        }
    }
}
