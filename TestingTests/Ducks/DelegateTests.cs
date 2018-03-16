using BusterWood.Testing;
using System;

namespace BusterWood.Ducks
{
    public class DelegateTests
    {        
        public static void can_call_a_void_method_via_proxy(Test t)
        {
            int calls = 0;
            Action target = () => calls++;
            ISimplist proxy = Duck.Cast<ISimplist>(target);
            proxy.Execute();
            t.Assert(1, calls);
        }
        
        public static void can_call_a_void_method_with_parameter_via_proxy(Test t)
        {
            int number = 0;
            Action<int> target = n => number = n;
            IWithNumber proxy = Duck.Cast<IWithNumber>(target);
            proxy.Execute(2);
            t.Assert(2, number);
        }
        
        public static void can_call_a_void_method_with_parameter_that_returns_something(Test t)
        {
            Func<int, int> target = n => n + 1;
            var proxy = Duck.Cast<IAdder>(target);
            t.Assert(3, proxy.AddOne(2));
        }
        
        public static void can_cast_to_simple_generic_interface_method(Test t)
        {
            Func<int, int> target = n => n + 1;
            var proxy = Duck.Cast<IAdder<int>>(target);
            t.Assert(3, proxy.AddOne(2));
        }
        
        public static void can_cast_proxy_to_another_interface_supported_by_wrapped_object(Test t)
        {
            Func<int, int> target = n => n + 1;
            var proxy = Duck.Cast<IAdder>(target);
            var proxy2 = Duck.Cast<IAdder<int>>(proxy); // note: cast the proxy
            t.Assert(3, proxy2.AddOne(2));
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

        public interface IAdder<T>
        {
            T AddOne(T num);
        }

    }

}