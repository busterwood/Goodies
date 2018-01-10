using BusterWood.Ducks;
using NUnit.Framework;
using System;

namespace UnitTests
{
    [TestFixture]
    public class InstanceMethodTests
    {
        [Test]
        public void can_call_a_void_method_via_proxy()
        {
            var target = new TargetSimplist();
            ISimplist proxy = Duck.Cast<ISimplist>(target);
            proxy.Execute();
            Assert.AreEqual(1, target.calls);
        }

        [Test]
        public void can_call_a_void_method_with_parameter_via_proxy()
        {
            var target = new TargetWithParameter();
            IWithNumber proxy = Duck.Cast<IWithNumber>(target);
            proxy.Execute(2);
            Assert.AreEqual(2, target.Number);
        }

        [Test]
        public void can_call_a_void_method_with_parameter_that_returns_something()
        {
            var target = new Adder();
            var proxy = Duck.Cast<IAdder>(target);
            Assert.AreEqual(3, proxy.AddOne(2));
        }

        [Test]
        public void can_create_proxy_for_interface_that_inherits_other_interfaces()
        {
            var target = new Combined();
            var proxy = Duck.Cast<ISimplistWithAdder>(target);
            Assert.AreEqual(3, proxy.AddOne(2));
        }

        [Test]
        public void can_cast_proxy_to_another_interface_supported_by_wrapped_object()
        {
            var target = new Combined();
            var proxy = Duck.Cast<ISimplist>(target);
            Duck.Cast<IAdder>(proxy);
        }

        [Test]
        public void can_cast_to_simple_property()
        {
            var target = new TargetSimplistProperty();
            var proxy = Duck.Cast<ISimplistProperty>(target);
            proxy.Value = 2;
            Assert.AreEqual(2, target.Value);
        }

        [Test]
        public void can_cast_to_simple_generic_interface_method()
        {
            var target = new Adder();
            var proxy = Duck.Cast<IAdder<int>>(target);
            Assert.AreEqual(3, proxy.AddOne(2));
        }

        [Test]
        public void can_cast_to_simple_generic_interface_with_property()
        {
            var target = new TargetSimplistProperty();
            var proxy = Duck.Cast<ISimplistProperty<int>>(target);
            proxy.Value = 2;
            Assert.AreEqual(2, target.Value);
        }

        [Test]
        public void can_cast_to_method_with_out_parameter()
        {
            var target = new TargetWithOutMethod();
            var proxy = Duck.Cast<IMethodWithOut>(target);
            int val;
            proxy.Execute(out val);
            Assert.AreEqual(1, val);
        }

        [Test]
        public void can_cast_to_method_with_ref_parameter()
        {
            var target = new TargetWithOutMethod();
            var proxy = Duck.Cast<IMethodWithRef>(target);
            int val = 1;
            proxy.AddOne(ref val);
            Assert.AreEqual(2, val);
        }

        [Test]
        public void cannot_cast_if_a_target_method_is_missing()
        {
            var target = new TargetBad();
            Assert.Throws<InvalidCastException>(() => Duck.Cast<ISimplist>(target));
        }

        [Test]
        public void can_cast_if_a_target_method_is_missing_but_throwing_not_implemented_exception_is_required()
        {
            var target = new TargetBad();
            var proxy = Mock.Cast<ISimplist>(target);
            Assert.Throws<NotImplementedException>(() => proxy.Execute());
        }

        [Test]
        public void can_duck_type_an_event()
        {
            var target = new TargetEvent();
            var proxy = Duck.Cast<IEventer>(target);
            EventHandler hander = (sender, args) => { };
            proxy.SimpleEvent += hander;
            Assert.AreEqual(1, target.count);
            proxy.SimpleEvent -= hander;
            Assert.AreEqual(0, target.count);
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