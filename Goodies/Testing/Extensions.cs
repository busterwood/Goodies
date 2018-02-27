using System;
using System.Linq.Expressions;

namespace BusterWood.Testing
{
    /// <summary>
    /// These extension methods are easy to use, but much slower than checking a condition with an if statement then calling <see cref="Test.Error(string)"/>
    /// </summary>
    public static class Extensions
    {
        /// <summary>Checks the <paramref name="expression"/> returns true, or reports the <paramref name="expression"/> as an error</summary>
        public static void Assert(this Test t, Expression<Func<bool>> expression)
        {
            var func = expression.Compile();
            if (!func())
                t.Error(expression.ToString());
        }

        /// <summary>Checks the <paramref name="expression"/> returns false, or reports the <paramref name="expression"/> as an error</summary>
        public static void AssertNot(this Test t, Expression<Func<bool>> expression)
        {
            var func = expression.Compile();
            if (func())
                t.Error(expression.ToString());
        }

        /// <summary>Check the <paramref name="expression"/> throw an exception of type <typeparamref name="T"/></summary>
        public static void AssertThrows<T>(this Test t, Expression<Func<object>> expression) where T : Exception
        {
            var act = expression.Compile();
            try
            {
                act();
                t.Error($"Expected {typeof(T).Name} to be thrown: {expression}");
            }
            catch (T)
            {
            }
            catch (Exception e)
            {
                t.Error($"Expected {typeof(T).Name} but {e.GetType()} was thrown: {expression}");
                t.Error(e.ToString());
            }
        }
    }
}
