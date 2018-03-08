using BusterWood.Collections;
using BusterWood.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BusterWood.Testing
{
    public static class Tests
    {
        /// <summary>Skip long tests?</summary>
        public static bool Short { get; set; }

        /// <summary>If set then all messages are logged, if not set then messages are only logged for failed tests</summary>
        public static bool Verbose { get; set; }

        /// <summary>Stop on first failed test?</summary>
        public static bool FailFast{ get; set; }

        static Tests()
        {
            var args = Environment.GetCommandLineArgs().ToHashSet(StringComparer.OrdinalIgnoreCase);
            Verbose = args.Contains("--verbose");
            Short = args.Contains("--short");
            FailFast = args.Contains("--fail.fast");
        }

        /// <summary>
        /// Runs all the tests in the <paramref name="assembly"/>, returning the number of failed tests.
        /// If <paramref name="assembly"/> is null, or not provided, then all tests in the entry assembly (exe) are run.
        /// </summary>
        public static int Run(Assembly assembly = null, Func<Test, bool> predicate = null)
        {
            int failCount = 0;
            Assembly assm = (assembly ?? Assembly.GetEntryAssembly());
            foreach (var t in assm.GetExportedTypes())
            {
                failCount += Run(t, predicate);
                if (FailFast && failCount > 0)
                    break;
            }
            return failCount;
        }

        /// <summary>Runs all the tests in the <paramref name="type"/>, returning the number of failed tests</summary>
        public static int Run(Type type, Func<Test, bool> predicate = null)
        {
            if (predicate == null)
                predicate = _ => true;

            var tests = DiscoverTests(type).Where(predicate).ToList();

            int failed = 0;
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
                    failed++;
                }
            }
            return failed;
        }

        //TODO: async test methods with a CancellationToken
        //TODO: timeout of test

        /// <summary>Finds all the tests in the <paramref name="type"/></summary>
        public static IEnumerable<Test> DiscoverTests(Type type)
        {
            var ctor = type.GetConstructor(new[] { typeof(Test) }) ?? type.GetConstructor(Type.EmptyTypes);
            foreach (var m in type.GetMethods())
            {
                var p = m.GetParameters();
                if (p.Length == 1 && p[0].ParameterType == typeof(Test))
                {
                    if (m.ReturnType == typeof(void) || m.ReturnType == typeof(Task))
                        yield return new Test { method = m, type = type, ctor = ctor };
                }
            }
        }

        /// <summary>Runs all the <paramref name="tests"/>, returning a flag indicating that the test passed</summary>
        public static bool Run(IEnumerable<Test> tests)
        {
            int failed = 0;
            foreach (var t in tests)
            {
                t.RunAsync().DontWait();
                t.Finished.Receive();

                if (t.Failed)
                {
                    WriteLine(ConsoleColor.Red, "Failed " + t);
                    failed++;
                }
                else if (t.Skipped)
                {
                    WriteLine(ConsoleColor.Cyan, "Skipped " + t);
                }
                else if (Verbose)
                {
                    Console.WriteLine(t.ToString());
                }

                if (FailFast && failed > 0)
                    break;
            }
            return failed == 0;
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
