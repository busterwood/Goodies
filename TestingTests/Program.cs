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
            //int result = Tests.Run(predicate: t => t.type.Name.Contains("Scanner") && t.Name.Contains("tuning"));
            //int result = Tests.Run(predicate: t => t.type.Namespace.Contains("Json"));
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
