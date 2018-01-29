using System;

namespace BusterWood.Contracts
{
    /// <summary>
    /// Code contracts are not really supported in VS2017, it was an MS research project
    /// </summary>
    public static class Contract
    {
        public static void Requires(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new ArgumentException("Pre-condition failed: " + message);
            }
        }

        public static void RequiresNotNull<T>(T obj, string message = null) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException("Pre-condition failed: " + message, (Exception)null);
            }
        }

        public static void RequiresNotNull<T>(T? val, string message = null) where T : struct
        {
            if (val == null)
            {
                throw new ArgumentNullException("Pre-condition failed: " + message, (Exception)null);
            }
        }

        public static void Ensures(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Post-condition failed: " + message);
            }
        }

        public static void Assert(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Assertion failed: " + message);
            }
        }
    }
}
