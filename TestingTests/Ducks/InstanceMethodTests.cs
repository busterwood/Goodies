using BusterWood.Testing;
using System;

namespace BusterWood.Ducks
{
    public class InstanceMethodTests
    {
        public static void can_call_a_void_method_via_proxy(Test t)
        {
            var target = new TargetSimplist();
            ISimplist proxy = Duck.Cast<ISimplist>(target);
            proxy.Execute();
            t.Assert(1, target.calls);
        }

        public static void can_call_a_void_method_with_parameter_via_proxy(Test t)
        {
            var target = new TargetWithParameter();
            IWithNumber proxy = Duck.Cast<IWithNumber>(target);
            proxy.Execute(2);
            t.Assert(2, target.Number);
        }

        public static void can_call_a_void_method_with_parameter_that_returns_something(Test t)
        {
            var target = new Adder();
            var proxy = Duck.Cast<IAdder>(target);
            t.Assert(3, proxy.AddOne(2));
        }

        public static void can_create_proxy_for_interface_that_inherits_other_interfaces(Test t)
        {
            var target = new Combined();
            var proxy = Duck.Cast<ISimplistWithAdder>(target);
            t.Assert(3, proxy.AddOne(2));
        }

        public static void can_cast_proxy_to_another_interface_supported_by_wrapped_object(Test t)
        {
            var target = new Combined();
            var proxy = Duck.Cast<ISimplist>(target);
            Duck.Cast<IAdder>(proxy);
        }

        public static void can_cast_to_simple_property(Test t)
        {
            var target = new TargetSimplistProperty();
            var proxy = Duck.Cast<ISimplistProperty>(target);
            proxy.Value = 2;
            t.Assert(2, target.Value);
        }

        public static void can_cast_to_simple_generic_interface_method(Test t)
        {
            var target = new Adder();
            var proxy = Duck.Cast<IAdder<int>>(target);
            t.Assert(3, proxy.AddOne(2));
        }

        public static void can_cast_to_simple_generic_interface_with_property(Test t)
        {
            var target = new TargetSimplistProperty();
            var proxy = Duck.Cast<ISimplistProperty<int>>(target);
            proxy.Value = 2;
            t.Assert(2, target.Value);
        }

        public static void can_cast_to_method_with_out_parameter(Test t)
        {
            var target = new TargetWithOutMethod();
            var proxy = Duck.Cast<IMethodWithOut>(target);
            int val;
            proxy.Execute(out val);
            t.Assert(1, val);
        }

        public static void can_cast_to_method_with_ref_parameter(Test t)
        {
            var target = new TargetWithOutMethod();
            var proxy = Duck.Cast<IMethodWithRef>(target);
            int val = 1;
            proxy.AddOne(ref val);
            t.Assert(2, val);
        }

        public static void cannot_cast_if_a_target_method_is_missing(Test t)
        {
            var target = new TargetBad();
            t.AssertThrows<InvalidCastException>(() => Duck.Cast<ISimplist>(target));
        }

        public static void can_cast_if_a_target_method_is_missing_but_throwing_not_implemented_exception_is_required(Test t)
        {
            var target = new TargetBad();
            var proxy = Mock.Cast<ISimplist>(target);
            t.AssertThrows<NotImplementedException>(() => proxy.Execute());
        }

        public static void can_duck_type_an_event(Test t)
        {
            var target = new TargetEvent();
            var proxy = Duck.Cast<IEventer>(target);
            EventHandler hander = (sender, args) => { };
            proxy.SimpleEvent += hander;
            t.Assert(1, target.count);
            proxy.SimpleEvent -= hander;
            t.Assert(0, target.count);
        }

        public class TargetEvent
        {
            public int count;

            public event EventHandler SimpleEvent
            {
                add { count++; }
                remove { count--; }
            }
        }

        public interface IEventer
        {
            event EventHandler SimpleEvent;
        }

        public interface ISimplistWithAdder : ISimplist, IAdder
        {
        }

        public interface ISimplist
        {
            void Execute();
        }


        public interface IMethodWithOut
        {
            void Execute(out int val);
        }

        public interface IMethodWithRef
        {
            void AddOne(ref int val);
        }

        public class TargetWithOutMethod
        {
            public void Execute(out int val)
            {
                val = 1;
            }

            public void AddOne(ref int val)
            {
                val += 1;
            }
        }

        public interface ISimplistProperty
        {
            int Value { get; set; }
        }

        public interface ISimplistProperty<T>
        {
            T Value { get; set; }
        }

        public interface IWithNumber
        {
            void Execute(int num);
        }

        public interface IAdder
        {
            int AddOne(int num);
        }

        public interface IAdder<T>
        {
            T AddOne(T num);
        }

        public class AdderStatic
        {
            public static explicit operator Adder(AdderStatic fred)
            {
                return new Adder();
            }
        }

        public class TargetSimplistProperty
        {
            public int Value { get; set; }
        }

        public class TargetSimplist
        {
            public int calls;

            public void Execute()
            {
                calls++;
            }
        }

        public class TargetWithParameter
        {
            public int Number;

            public void Execute(int num)
            {
                Number = num;
            }
        }

        public class Adder
        {
            public int AddOne(int num) => num + 1;
        }

        public class TargetBad
        {
            public void Fred2()
            {
            }
        }

        public class Combined
        {
            public int calls;

            public void Execute()
            {
                calls++;
            }

            public int AddOne(int num) => num + 1;
        }
    }

}