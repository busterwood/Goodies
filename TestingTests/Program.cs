using BusterWood.Monies;
using BusterWood.Testing;
using System;

namespace TestingTests
{
    public class Program   
    {
        public static int Main()
        {
            return Tests.Run(typeof(MoneyTests));
        }
    }
}
