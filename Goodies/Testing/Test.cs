using BusterWood.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BusterWood.Testing
{
    public class Test
    {
        internal Type type;
        internal MethodInfo method;
        internal readonly List<string> Messsages = new List<string>();
        internal readonly Action<string> LogMessage = Console.WriteLine;
        
        public string TypeName => type.Name;

        /// <summary>returns the name of the running test</summary>
        public string Name => method.Name;

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

        internal void Run()
        {
            var instance = Activator.CreateInstance(type);
            var action = (Action<Test>)method.CreateDelegate(typeof(Action<Test>), instance);
            using (instance as IDisposable)
            {
                action(this);
            }
        }

        /// <summary>Skip long tests?</summary>
        public static bool Short { get; set; }

        /// <summary>If set then all messages are logged, if not set then messages are only logged for failed tests</summary>
        public static bool Verbose { get; set; }

        static Test()
        {
            var args = Environment.GetCommandLineArgs().ToHashSet(StringComparer.OrdinalIgnoreCase);
            Verbose = args.Contains("--verbose");
            Short = args.Contains("--short");
        }
    }
}
