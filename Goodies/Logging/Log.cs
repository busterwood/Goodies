using System;
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
            _logContext.Value = new LogContext(value, LogContext.Current);
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
                Pop();
            }
        }
    }
}