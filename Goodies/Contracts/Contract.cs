using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BusterWood.Contracts
{
    /// <summary>
    /// Code contracts are not really supported in VS2017, it was an MS research project
    /// </summary>
    public static class Contract
    {
        private const string PreConditionFailed = "Pre-condition failed";
        private const string PostConditionFailed = "Post-condition failed";
        private const string AssertionFailed = "Assertion failed";

        public static void Requires(bool condition, string message = null, [CallerFilePath] string filePath = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        {
            if (!condition)
            {
                var msg = $"{message ?? PreConditionFailed} at {filePath}:{member}:{line}";
#if DEBUG
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif
                throw new ArgumentException(msg);
            }
        }

        public static void RequiresNotNull<T>(T obj, string message = null, [CallerFilePath] string filePath = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0) where T : class
        {
            if (obj == null)
            {
                var msg = $"{message ?? PreConditionFailed} at {filePath}:{member}:{line}";
#if DEBUG
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif
                throw new ArgumentNullException(msg, (Exception)null);
            }
        }

        public static void RequiresNotNull<T>(T? val, string message = null, [CallerFilePath] string filePath = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0) where T : struct
        {
            if (val == null)
            {
                var msg = $"{message ?? PreConditionFailed} at {filePath}:{member}:{line}";
#if DEBUG
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif
                throw new ArgumentNullException("Pre-condition failed: " + message, (Exception)null);
            }
        }

        // only for RELEASE builds
        [Conditional("RELEASE")]
        public static void Ensures(bool condition, string message = null, [CallerFilePath] string filePath = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        {
            if (!condition)
            {
                var msg = $"{message ?? PostConditionFailed} at {filePath}:{member}:{line}";
#if DEBUG
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif
                throw new InvalidOperationException(msg);
            }
        }

        public static void Assert(bool condition, string message = null, [CallerFilePath] string filePath = null, [CallerMemberName] string member = null, [CallerLineNumber] int line = 0)
        {
            if (!condition)
            {
                var msg = $"{message ?? AssertionFailed} at {filePath}:{member}:{line}";
#if DEBUG
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif
                throw new InvalidOperationException(msg);
            }
        }
    }
}
