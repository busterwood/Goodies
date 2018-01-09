using BusterWood.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusterWood.Goodies
{
    public static class TextExtensions
    {
        public static bool IsLetter(this char c) => char.IsLetter(c);
        public static bool IsWhiteSpace(this char c) => char.IsWhiteSpace(c);
        public static bool IsUpper(this char c) => char.IsUpper(c);
        public static bool IsLower(this char c) => char.IsLower(c);
        public static char ToUpper(this char c) => char.ToUpper(c);
        public static char ToLower(this char c) => char.ToLower(c);

        public static IEnumerable<char> ToUpper(this IEnumerable<char> chars) => chars.Select(c => c.ToUpper());
        public static IEnumerable<char> ToLower(this IEnumerable<char> chars) => chars.Select(c => c.ToLower());

        /// <summary>Turns a sequnce of characters into a string</summary>
        public static string String(this IEnumerable<char> items) => items is string ? (string)items : new string(items.ToArray());

        /// <summary>Turns a sequnce of objects into a string with a <param name="separator"/> between each</summary>
        public static string String(this IEnumerable items, string separator = " ") => string.Join(separator, items.Cast<object>().Select(i => i?.ToString()));

        /// <summary>Turns a sequnce of strings into a big string with a <param name="separator"/> between each</summary>
        public static string String(this IEnumerable<string> items, string separator = " ") => string.Join(separator, items);

        /// <summary>Separate "TitleCase" text into "Title Case"</summary>
        public static string ToWords(this string text) => text?.SplitOn(c => char.IsUpper(c)).Select(String).String();

        /// <summary>Formats "title case" text into "TitleCase"</summary>
        public static string ToTitleCase(this string text) => text
            .SplitOn(c => !c.IsLetter())
            .Select(word => word.First().ToUpper().Concat(word.Rest().ToLower()).String())
            .String("");

        public delegate bool TryParser<T>(string text, out T result);

        public static T? Parse<T>(this string text, TryParser<T> parser, T? @default = null) where T : struct
        {
            T temp;
            return parser(text, out temp) ? temp : @default;
        }

        public static int? ParseInt(this string text, int? @default = null) => text.Parse(int.TryParse, @default);

        public static long? ParseLong(this string text, long? @default = null) => text.Parse(long.TryParse, @default);

        public static DateTime? ParseDateTime(this string text, DateTime? @default = null) => text.Parse(DateTime.TryParse, @default);

        public static StringBuilder ToBuilder(this string text) => new StringBuilder(text);

        public static StringBuilder ToBuilder(this IEnumerable items, string separator = null) => new StringBuilder().Append(separator, items);

        public static StringBuilder Append(this StringBuilder sb, string separator, params object[] items) => Append(sb, separator, ((IEnumerable)items));

        public static StringBuilder Append(this StringBuilder sb, string separator, IEnumerable items)
        {
            var startLen = sb.Length;
            foreach (var i in items)
                sb.Append(i).Append(separator);
            if (sb.Length > startLen)
                sb.Length -= separator.Length; // remove last separator
            return sb;
        }
    }
}