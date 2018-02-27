using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace BusterWood.Testing
{
    public static class Tests
    {
        public static int Run(Assembly assembly)
        {
            int failCount = 0;
            foreach (var t in assembly.GetExportedTypes())
            {
                failCount += Run(t);
            }
            return failCount;
        }

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
                    WriteLine(ConsoleColor.Green, $"ok\t{type.Name}\t{sw.ElapsedMilliseconds:N0}MS");
                }
                else
                {
                    WriteLine(ConsoleColor.Red, $"FAILED \t{type.Name}\t{sw.ElapsedMilliseconds:N0}MS");
                    failCount++;
                }
            }
            return failCount;
        }

        public static IEnumerable<Test> DiscoverTests(Type type)
        {
            object instance = null;
            foreach (var m in type.GetMethods())
            {
                var p = m.GetParameters();
                if (p.Length != 1 || p[0].ParameterType != typeof(Test))
                    continue;
                if (instance == null)
                    instance = Activator.CreateInstance(type);
                var test = (Action<Test>)m.CreateDelegate(typeof(Action<Test>), instance);
                yield return new Test { method = test, TypeName = type.Name, Name = m.Name };
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
