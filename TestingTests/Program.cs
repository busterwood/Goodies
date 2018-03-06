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
            var sw = new Stopwatch();
            sw.Start();
            EnableMultiCoreJit();
            int result = Tests.Run();
            Console.Error.WriteLine($"TOTAL time {sw.Elapsed.TotalSeconds:N2} secs");
            return result;
        }

        private static void EnableMultiCoreJit()
        {
            ProfileOptimization.SetProfileRoot(Environment.ExpandEnvironmentVariables("%APPDATA%"));
            ProfileOptimization.StartProfile(Process.GetCurrentProcess().ProcessName + ".profile");
        }
    }
}
