using System;
using System.Diagnostics;
using System.Runtime;
using BusterWood.Testing;

namespace TestingTests
{
    public class Program
    {
        public static int Main()
        {
            // enable multi-core JIT for faster testing
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ProfileOptimization.SetProfileRoot(Environment.ExpandEnvironmentVariables("%APPDATA%"));
            ProfileOptimization.StartProfile(Process.GetCurrentProcess().ProcessName + ".profile");
            int result = Tests.Run();
            Console.Error.WriteLine($"TOTAL time {sw.Elapsed.TotalSeconds:N2} secs");
            return result;
        }
    }
}
