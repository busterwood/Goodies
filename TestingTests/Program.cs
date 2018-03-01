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
            ProfileOptimization.SetProfileRoot(Environment.ExpandEnvironmentVariables("%APPDATA%"));
            ProfileOptimization.StartProfile(Process.GetCurrentProcess().ProcessName + ".profile");
            return Tests.Run();
        }
    }
}
