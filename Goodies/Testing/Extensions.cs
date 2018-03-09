using System;
using System.Linq.Expressions;

namespace BusterWood.Testing
{
    public static class Extensions
    {
        /// <summary>Equivalent to <see cref="Test.Log"/> followed by <see cref="Test.Error"/></summary>
        public static void Error(this Test t, string message)
        {
            t.Log(message);
            t.Error();
        }

        /// <summary>Fatal is equivalent to <see cref="Test.Log"/> followed by <see cref="Test.Fatal"/></summary>
        public static void Fatal(this Test t, string message)
        {
            t.Log(message);
            t.Fatal();
        }

        /// <summary>Skip is equivalent to <see cref="Test.Log"/> followed by <see cref="Test.Skip"/></summary>
        public static void Skip(this Test t, string message)
        {
            t.Log(message);
            t.Skip();
        }

        /// <summary>Checks the <paramref name="expression"/> returns true, or reports the <paramref name="expression"/> as an error</summary>
        /// <remarks>This method is easy to use, but much slower than checking a condition with an if statement then calling <see cref="Error(Test, string)"/>/// </remarks>
        public static void Assert(this Test t, Expression<Func<bool>> expression)
        {
            var func = expression.Compile();
            if (!func())
                t.Error(expression.ToString());
        }

        /// <summary>Checks the <paramref name="expression"/> returns false, or reports the <paramref name="expression"/> as an error</summary>
        /// <remarks>This method is easy to use, but much slower than checking a condition with an if statement then calling <see cref="Error(Test, string)"/>/// </remarks>
        public static void AssertNot(this Test t, Expression<Func<bool>> expression)
        {
            var func = expression.Compile();
            if (func())
                t.Error(expression.ToString());
        }

        /// <summary>Check the <paramref name="expression"/> throw an exception of type <typeparamref name="T"/></summary>
        public static T AssertThrows<T>(this Test t, Expression<Func<object>> expression) where T : Exception
        {
            var act = expression.Compile();
            try
            {
                act();
                t.Error($"Expected {typeof(T).Name} to be thrown: {expression}");
            }
            catch (T e)
            {
                return e;
            }
            catch (Exception e)
            {
                t.Error($"Expected {typeof(T).Name} but {e.GetType()} was thrown: {expression}");
                t.Error(e.ToString());
            }
            throw new NotImplementedException();
        }

        /// <summary>Check the <paramref name="expression"/> throw an exception of type <typeparamref name="T"/></summary>
        public static T AssertThrows<T>(this Test t, Action act, string message) where T : Exception
        {
            try
            {
                act();
                t.Error($"Expected {typeof(T).Name} to be thrown: {message}");
            }
            catch (T e)
            {
                return e;
            }
            catch (Exception e)
            {
                t.Error($"Expected {typeof(T).Name} but {e.GetType()} was thrown: {message}");
                t.Error(e.ToString());
            }
            throw new NotImplementedException();
        }
    }
}
