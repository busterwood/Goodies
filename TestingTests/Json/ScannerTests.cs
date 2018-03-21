using BusterWood.Testing;
using System.IO;

namespace BusterWood.Json
{
    public class ScannerTests
    {
        public static void can_read_empty_string(Test t)
        {
            var s = NewScanner(" \"\"");
            t.Assert(true, s.MoveNext());
            var cur = s.Current;
            t.Assert(Parser.Type.String, cur.Type);
            t.Assert("", cur.Text);
            t.Assert(2, cur.Index);
        }

        public static void can_read_string(Test t)
        {
            var s = NewScanner(" \"abc123\"");
            AssertString(t, s, "abc123");
        }

        public static void can_read_string_with_embedded_double_quote(Test t)
        {
            var s = NewScanner(" \"abc\\\"123\"");
            AssertString(t, s, "abc\"123");
        }

        public static void can_read_string_with_embedded_backslash(Test t)
        {
            var s = NewScanner(" \"abc\\\\123\"");
            AssertString(t, s, "abc\\123");
        }

        public static void can_read_string_with_embedded_slash(Test t)
        {
            var s = NewScanner(" \"abc\\/123\"");
            AssertString(t, s, "abc/123");
        }

        public static void can_read_string_with_embedded_ff(Test t)
        {
            var s = NewScanner(" \"abc\\f123\"");
            AssertString(t, s, "abc\f123");
        }

        public static void can_read_string_with_embedded_cr(Test t)
        {
            var s = NewScanner(" \"abc\\r123\"");
            AssertString(t, s, "abc\r123");
        }

        public static void can_read_string_with_embedded_lf(Test t)
        {
            var s = NewScanner(" \"abc\\n123\"");
            AssertString(t, s, "abc\n123");
        }

        public static void can_read_string_with_embedded_tab(Test t)
        {
            var s = NewScanner(" \"abc\\t123\"");
            AssertString(t, s, "abc\t123");
        }

        public static void can_read_string_with_embedded_backspace(Test t)
        {
            var s = NewScanner(" \"abc\\b123\"");
            AssertString(t, s, "abc\b123");
        }

        public static void can_read_integer(Test t)
        {
            for (int i = -100; i < 100; i++)
            {
                var s = NewScanner(" " + i);
                AssertToken(t, s, new Parser.Token(2, i.ToString(), Parser.Type.Number));
            }
        }

        public static void can_read_number(Test t)
        {
            for (double i = -10.0; i < 10.0; i+= 0.1)
            {
                var s = NewScanner($" {i:N1}");
                AssertToken(t, s, new Parser.Token(2, i.ToString("N1"), Parser.Type.Number));
            }
        }

        public static void can_read_null(Test t)
        {
            var s = NewScanner(" null ");
            AssertToken(t, s, new Parser.Token(2, "null", Parser.Type.Null));
        }

        public static void can_read_true(Test t)
        {
            var s = NewScanner(" true ");
            AssertToken(t, s, new Parser.Token(2, "true", Parser.Type.True));
        }

        public static void can_read_false(Test t)
        {
            var s = NewScanner(" false ");
            AssertToken(t, s, new Parser.Token(2, "false", Parser.Type.False));
        }

        public static void can_read_start_of_object(Test t)
        {
            var s = NewScanner(" { ");
            AssertToken(t, s, new Parser.Token(2, "{", Parser.Type.StartObject));
        }

        public static void can_read_end_of_object(Test t)
        {
            var s = NewScanner(" } ");
            AssertToken(t, s, new Parser.Token(2, "}", Parser.Type.EndObject));
        }

        public static void can_read_start_of_array(Test t)
        {
            var s = NewScanner(" [ ");
            AssertToken(t, s, new Parser.Token(2, "[", Parser.Type.StartArray));
        }

        public static void can_read_end_of_array(Test t)
        {
            var s = NewScanner(" ] ");
            AssertToken(t, s, new Parser.Token(2, "]", Parser.Type.EndArray));
        }

        public static void can_read_comma(Test t)
        {
            var s = NewScanner(" , ");
            AssertToken(t, s, new Parser.Token(2, ",", Parser.Type.Comma));
        }

        public static void can_read_colon(Test t)
        {
            var s = NewScanner(" : ");
            AssertToken(t, s, new Parser.Token(2, ":", Parser.Type.Colon));
        }


        static Parser.Scanner NewScanner(string text) => new Parser.Scanner(new StringReader(text));

        static void AssertString(Test t, Parser.Scanner s, string expectedText, int expectedIndex = 2) => AssertToken(t, s, new Parser.Token(expectedIndex, expectedText, Parser.Type.String));

        private static void AssertToken(Test t, Parser.Scanner s, Parser.Token expected)
        {
            t.Assert(true, s.MoveNext());
            var cur = s.Current;
            t.Assert(expected.Type, cur.Type);
            t.Assert(expected.Text, cur.Text);
            t.Assert(expected.Index, cur.Index);
        }
    }
}
