using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Testing
{
    public static class Tests
    {
        /// <summary>
        /// Runs all the tests in the <paramref name="assembly"/>, returning the number of failed tests.
        /// If <paramref name="assembly"/> is null, or not provided, then all tests in the entry assembly (exe) are run.
        /// </summary>
        public static int Run(Assembly assembly = null)
        {
            int failCount = 0;
            foreach (var t in (assembly ?? Assembly.GetEntryAssembly()).GetExportedTypes())
            {
                failCount += Run(t);
            }
            return failCount;
        }

        /// <summary>Runs all the tests in the <paramref name="type"/>, returning the number of failed tests</summary>
        public static int Run(Type type)
        {
            var tests = DiscoverTests(type).ToList();
            int failCount = 0;
            if (tests.Any())
            {
                var sw = new Stopwatch();
                sw.Start();
                bool ok = Run(tests);
                sw.Stop();
                if (ok)
                {
                    WriteLine(ConsoleColor.Green, $"ok\t{sw.ElapsedMilliseconds:N0}ms\t{type.Name}");
                }
                else
                {
                    WriteLine(ConsoleColor.Red, $"FAILED\t{sw.ElapsedMilliseconds:N0}ms\t{type.Name}");
                    failCount++;
                }
            }
            return failCount;
        }

        //TODO: async test methods with a CancellationToken
        //TODO: timeout of test
        //TODO: way of skipping all tests via the constructor, e.g. take a Test parameter into the ctor

        /// <summary>Finds all the tests in the <paramref name="type"/></summary>
        public static IEnumerable<Test> DiscoverTests(Type type)
        {
            foreach (var m in type.GetMethods())
            {
                var p = m.GetParameters();
                if (p.Length == 1 && p[0].ParameterType == typeof(Test))
                {
                    if (m.ReturnType == typeof(void) || m.ReturnType == typeof(Task))
                        yield return new Test { method = m, type = type };
                }
            }
        }

        /// <summary>Runs all the <paramref name="tests"/>, returning a flag indicating that the test passed</summary>
        public static bool Run(IEnumerable<Test> tests)
        {
            int failed = 0;
            using (var finished = new AutoResetEvent(false))
            {
                foreach (var t in tests)
                {
                    if (t.IsAsync)
                    {
                        RunViaTask(t);
                    }
                    else
                    {
                        RunViaThreadPool(finished, t);
                    }

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

        private static void RunViaThreadPool(AutoResetEvent finished, Test t)
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
        }

        private static void RunViaTask(Test at)
        {
            try
            {
                at.RunAsync().Wait();
            }
            catch (AggregateException e) when (e.InnerException != null)
            {
                throw e.InnerException;
            }
        }

        static void WriteLine(ConsoleColor color, string message)
        {
            if (Console.IsOutputRedirected)
            {
                Console.WriteLine(message);
                return;
            }
            var before = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = before;
        }
    }
}
