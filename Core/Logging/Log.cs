using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO;

namespace BusterWood.Logging
{
    /// <summary>
    /// Structured logging to <see cref="Console.Error"/> (StdErr).  
    /// If you want logging to a file then redirect StdErr using the shell or another program.
    /// </summary>
    public static class Log
    {
        static readonly string Program = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

        internal static readonly AsyncLocal<LogContext> _logContext = new AsyncLocal<LogContext>();

        static readonly Dictionary<string, ConsoleColor> levelColours = new Dictionary<string, ConsoleColor>
        {
            ["DEBUG"] = ConsoleColor.DarkGray,
            ["INFO"] = ConsoleColor.Gray,
            ["WARN"] = ConsoleColor.Yellow,
            ["ERROR"] = ConsoleColor.Red,
        };

        public static void Debug(string message, object extraCtx = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int sourceLine = 0, [CallerMemberName] string sourceMember = null)
        {
            var ctx = AddSourceContext(extraCtx, sourceFile, sourceLine, sourceMember);
            WriteLine("DEBUG", ctx, message);
        }

        public static void Error(string message, object extraCtx = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int sourceLine = 0, [CallerMemberName] string sourceMember = null)
        {
            var ctx = AddSourceContext(extraCtx, sourceFile, sourceLine, sourceMember);
            WriteLine("ERROR", ctx, message);
        }

        public static void Info(string message, object extraCtx = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int sourceLine = 0, [CallerMemberName] string sourceMember = null)
        {
            var ctx = AddSourceContext(extraCtx, sourceFile, sourceLine, sourceMember);
            WriteLine("INFO", ctx, message);
        }

        public static void Warning(string message, object extraCtx = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int sourceLine = 0, [CallerMemberName] string sourceMember = null)
        {
            var ctx = AddSourceContext(extraCtx, sourceFile, sourceLine, sourceMember);
            WriteLine("WARN", ctx, message);
        }

        private static LogContext AddSourceContext(object extraContext, string sourceFile, int sourceLine, string sourceMember)
        {
            var ctx = extraContext == null ? LogContext.Current : new LogContext(extraContext, LogContext.Current);
            return ctx.Add(new { src = $"{Path.GetFileName(sourceFile)};{sourceLine};{sourceMember}" });
        }

        static void WriteLine(string level, LogContext context, string message)
        {
            string fullMessage = $"{Program}: {level,-5}: {DateTime.UtcNow:o}: {context}: {message}";
            if (Console.IsErrorRedirected)
            {
                Console.Error.WriteLine(fullMessage);
            }
            else
            {
                var next = levelColours[level];
                lock (levelColours)
                {
                    var prev = Console.ForegroundColor;
                    Console.ForegroundColor = next;
                    Console.Error.WriteLine(fullMessage);
                    Console.ForegroundColor = prev;
                }
            }
        }

        public static Popper Push(object value)
        {
            var newCtx = new LogContext(value, LogContext.Current);
            _logContext.Value = newCtx;
            return new Popper();
        }

        public static LogContext Pop()
        {
            var newCtx = LogContext.Current.Next;
            _logContext.Value = LogContext.Current.Next;
            return newCtx;
        }

        public struct Popper : IDisposable
        {
            public void Dispose()
            {
                Log.Pop();
            }
        }
    }

    /// <summary>Immutable context information used for logging, use the <see cref="Push"/> and <see cref="Pop()"/> methods to add and remove context information</summary>
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