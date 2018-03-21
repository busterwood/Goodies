using BusterWood.Testing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BusterWood.Json
{
    public class ParserTests
    {
        public static void can_read_empty_object(Test t)
        {
            var s = NewParser(" {} ");
            var map = s.ReadObject();
            t.Assert(0, map?.Count);
        }

        public static void cannot_read_empty_partial_object(Test t)
        {
            var s = NewParser(" {");
            var ex = t.AssertThrows<ParseException>(() => s.ReadObject());
            t.Assert("Expected a name or end of object but got end-of-file at 2", ex.Message);
        }

        public static void can_read_object_with_one_property(Test t)
        {
            var s = NewParser(@"{
    ""first"" : 123
}");
            var map = s.ReadObject();
            t.Assert(1, map?.Count);
            t.Assert("first", map.Keys.First());
            t.Assert(123, map["first"]);
        }

        public static void cannot_read_object_with_name_colon_but_no_value(Test t)
        {
            var s = NewParser(@"{
    ""first"" : 
}");
            var ex = t.AssertThrows<ParseException>(() => s.ReadObject());
            t.Assert("Unexpected EndObject '}' at 20", ex.Message);
        }

        public static void cannot_read_object_with_name_value_but_no_colon(Test t)
        {
            var s = NewParser(@"{
    ""first"" 123
}");
            var ex = t.AssertThrows<ParseException>(() => s.ReadObject());
            t.Assert("Expected a Colon but got Number '123' at 16", ex.Message);
        }

        public static void can_read_object_with_multiple_properties(Test t)
        {
            var s = NewParser(@"{
    ""first"" : 123,
    ""sec"" : ""hello"",
    ""3"" : null,
    ""4"" : true,
    ""5"" : false
}");
            var map = s.ReadObject();
            t.Assert(5, map?.Count);
            t.Assert(123, map["first"]);
            t.Assert("hello", map["sec"]);
            t.Assert(null, map["3"]);
            t.Assert(true, map["4"]);
            t.Assert(false, map["5"]);
        }

        public static void can_read_object_with_embedded_object(Test t)
        {
            var s = NewParser(@"{
    ""first"" : 123,
    ""sec"":{
        ""hello"": null
    }
}");
            var map = s.ReadObject();
            t.Assert(2, map?.Count);
            t.Assert(123, map["first"]);
            var embedded = map["sec"] as Dictionary<string,object>;
            t.Assert(1, embedded?.Count);
            t.Assert(null, embedded["hello"]);
        }

        public static void can_read_object_with_embedded_array(Test t)
        {
            var s = NewParser(@"{
    ""first"" : 123,
    ""sec"":[true, false ]
}");
            var map = s.ReadObject();
            t.Assert(2, map?.Count);
            t.Assert(123, map["first"]);
            var embedded = map["sec"] as List<object>;
            t.Assert(2, embedded?.Count);
            t.Assert(true, embedded[0]);
            t.Assert(false, embedded[1]);
        }

        public static void can_read_empty_array(Test t)
        {
            var s = NewParser(" [] ");
            var arr = s.ReadArray();
            t.Assert(0, arr?.Count);
        }

        public static void cannot_read_empty_array_with_no_end_bracket(Test t)
        {
            var s = NewParser(" [ ");
            var ex = t.AssertThrows<ParseException>(() => s.ReadArray());
            t.Assert("Expected a value or end of array but got end-of-file at 3", ex.Message);
        }

        public static void can_read_array_with_one_value(Test t)
        {
            var s = NewParser("[123]");
            var arr = s.ReadArray();
            t.Assert(1, arr?.Count);
            t.Assert(123, arr[0]);
        }

        public static void cannot_read_array_with_leading_comma(Test t)
        {
            var s = NewParser("[,123]");
            var ex = t.AssertThrows<ParseException>(() => s.ReadArray());
            t.Assert("Unexpected Comma ',' at 2", ex.Message);
        }

        public static void cannot_read_array_trailing_comma(Test t)
        {
            var s = NewParser("[123,]");
            var ex = t.AssertThrows<ParseException>(() => s.ReadArray());
            t.Assert("Unexpected EndArray ']' at 6", ex.Message);
        }

        public static void cannot_read_array_with_double_comma (Test t)
        {
            var s = NewParser("[1,,2]");
            var ex = t.AssertThrows<ParseException>(() => s.ReadArray());
            t.Assert("Unexpected Comma ',' at 4", ex.Message);
        }

        public static void can_read_array_with_multiple_values(Test t)
        {
            var s = NewParser(@"[123,null,true,false,""abc""]");
            var arr = s.ReadArray();
            t.Assert(5, arr?.Count);
            t.Assert(123, arr[0]);
            t.Assert(null, arr[1]);
            t.Assert(true, arr[2]);
            t.Assert(false, arr[3]);
            t.Assert("abc", arr[4]);
        }

        public static void can_read_null(Test t)
        {
            var s = NewParser("");
            var obj = s.Read();
            t.Assert(null, obj);
        }

        public static void can_read_object(Test t)
        {
            var s = NewParser("{}");
            var map = s.Read() as Dictionary<string, object>;
            t.Assert(0, map?.Count);
        }

        public static void can_read_array(Test t)
        {
            var s = NewParser("[]");
            var list = s.Read() as List<object>;
            t.Assert(0, list?.Count);
        }

        public static void cannot_read_other_when_only_object_or_array_expected(Test t)
        {
            foreach (var txt in new string[] { "null", "true", "false", "\"hello\"", "123", ",", ":" })
            {
                var s = NewParser(txt);
                var ex = t.AssertThrows<ParseException>(() => s.Read());
            }
        }

        static Parser NewParser(string text) => new Parser(new Parser.Scanner(new StringReader(text)));
    }
}
