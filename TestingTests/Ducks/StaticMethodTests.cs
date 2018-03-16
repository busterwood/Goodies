using BusterWood.Testing;
using System;

namespace BusterWood.Ducks
{
    public class StaticMethodTests
    {
        public static void can_call_a_void_method_via_proxy(Test t)
        {
            TargetSimplist.calls = 0;
            ISimplist proxy = Duck.Cast<ISimplist>(typeof(TargetSimplist));
            proxy.Execute();
            t.Assert(1, TargetSimplist.calls);
        }

        public static void can_call_a_void_method_with_parameter_via_proxy(Test t)
        {
            IWithNumber proxy = Duck.Cast<IWithNumber>(typeof(TargetWithParameter));
            proxy.Execute(2);
            t.Assert(2, TargetWithParameter.Number);
        }

        public static void can_call_a_void_method_with_parameter_that_returns_something(Test t)
        {
            var proxy = Duck.Cast<IAdder>(typeof(Adder));
            t.Assert(3, proxy.AddOne(2));
        }

        public static void can_proxy_system_io_file(Test t)
        {
            var proxy = Duck.Cast<IExister>(typeof(System.IO.File));
            t.Assert(true, proxy.Exists(@"c:\Windows\notepad.exe"));
        }

        public static void proxy_inherited_interface_types(Test t)
        {
            var proxy = Duck.Cast<IExistDeleter>(typeof(System.IO.File));
            t.Assert(true, proxy.Exists(@"c:\Windows\notepad.exe"));
        }

        public static void cannot_cast_if_a_target_method_is_missing(Test t)
        {
            t.AssertThrows<InvalidCastException>(() => Duck.Cast<ISimplist>(typeof(TargetBad)));
        }

        public static void can_cast_if_a_target_method_is_missing_but_throwing_not_implemented_is_desired(Test t)
        {
            var proxy = Mock.Cast<ISimplist>(typeof(TargetBad));
            t.AssertThrows<NotImplementedException>(() => proxy.Execute());
        }

        public static void can_duck_type_an_event(Test t)
        {
            var proxy = Duck.Cast<IEventer>(typeof(TargetEvent));
            EventHandler hander = (sender, args) => { };
            proxy.SimpleEvent += hander;
            t.Assert(1, TargetEvent.count);
            proxy.SimpleEvent -= hander;
            t.Assert(0, TargetEvent.count);
        }

        public static class TargetEvent
        {
            public static int count;

            public static event EventHandler SimpleEvent
            {
                add { count++; }
                remove { count--; }
            }
        }

        public interface IEventer
        {
            event EventHandler SimpleEvent;
        }

        public interface IExistDeleter : IExister, IDeleter
        {
        }
        public interface IExister
        {
            bool Exists(string path);
        }

        public interface IDeleter
        {
            void Delete(string path);
        }

        public interface ISimplist
        {
            void Execute();
        }

        public interface IWithNumber
        {
            void Execute(int num);
        }

        public interface IAdder
        {
            int AddOne(int num);
        }

        public class TargetSimplist
        {
            public static int calls;

            public static void Execute()
            {
                calls++;
            }
        }

        public class TargetWithParameter
        {
            public static int Number;

            public static void Execute(int num)
            {
                Number = num;
            }
        }

        public class Adder
        {
            public static int AddOne(int num) => num + 1;
        }

        public class TargetBad
        {
            public static void Fred2()
            {
            }
        }
    }
}