using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace BusterWood.Logging
{

    /// <summary>Immutable context information used for logging, use the <see cref="Log.Push"/> and <see cref="Log.Pop()"/> methods to add and remove context information</summary>
    /// <remarks>Uses the LogicalCallContext so it works with async methods as well as normal methods</remarks>
    public class LogContext : IEnumerable<LogContext>
    {
        static readonly LogContext Empty = new LogContext();

        static LogContext()
        {
            Empty.Next = Empty;
        }

        public object Value { get; }

        internal LogContext Next { get; private set; }

        public LogContext() { }

        internal LogContext(object value, LogContext next)
        {
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
            this.Next = next;
        }

        public static LogContext Current => Log._logContext.Value ?? Empty;

        public LogContext Add(LogContext other) => new LogContext(other.Value, this);

        /// <summary>Add an anonymous type</summary>
        public LogContext Add(object value) => new LogContext(value, this);

        public override string ToString()
        {
            var withThread = Add(new { thread = Thread.CurrentThread.ManagedThreadId });
            return string.Join(", ", withThread.Reverse().Where(x => x.Value != null).Select(TrimmedContextValue));
        }

        static string TrimmedContextValue(LogContext ctx) => ctx.Value.ToString().Trim(' ', '{', '}');

        public IEnumerator<LogContext> GetEnumerator()
        {
            for (var ctx = this; ctx != null && ctx != Empty; ctx = ctx.Next)
                yield return ctx;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}