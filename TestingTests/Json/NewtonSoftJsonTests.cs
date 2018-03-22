using BusterWood.Testing;
using System;
using System.IO;
using Newtonsoft.Json;

namespace BusterWood.Json
{
    public class NewtonSoftJsonTests
    {
        public static void can_read_large_file(Test t)
        {
            t.Verbose = true;
            var txt = File.ReadAllText(@"Json\canada.json");
            for (int i = 0; i < 10; i++)
            {
                var sr = new StringReader(txt);
                var pm = new PerformaceMonitor(true);
                var p = new JsonTextReader(sr);
                while (p.Read())
                {
                    GC.KeepAlive(p.Value);
                }
                t.Log("Reading 2MB Json file " + pm.Stop());
            }
            GC.KeepAlive(txt);
        }

        public static void can_read_twitter_file(Test t)
        {
            t.Verbose = true;
            var txt = File.ReadAllText(@"Json\twitter.json");
            for (int i = 0; i < 10; i++)
            {
                var sr = new StringReader(txt);
                var pm = new PerformaceMonitor(true);
                var p = new JsonTextReader(sr);
                while (p.Read())
                {
                    GC.KeepAlive(p.Value);
                }
                t.Log("Reading 617K Json file " + pm.Stop());
            }
            GC.KeepAlive(txt);
        }
    }
}
