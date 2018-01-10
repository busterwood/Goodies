using BusterWood.Ducks;
using NUnit.Framework;
using System;

namespace UnitTests
{
    [TestFixture]
    public class DelegateTests
    {
        [Test]
        public void can_call_a_void_method_via_proxy()
        {
            int calls = 0;
            Action target = () => calls++;
            ISimplist proxy = Duck.Cast<ISimplist>(target);
            proxy.Execute();
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void can_call_a_void_method_with_parameter_via_proxy()
        {
            int number = 0;
            Action<int> target = n => number = n;
            IWithNumber proxy = Duck.Cast<IWithNumber>(target);
            proxy.Execute(2);
            Assert.AreEqual(2, number);
        }

        [Test]
        public void can_call_a_void_method_with_parameter_that_returns_something()
        {
            Func<int, int> target = n => n + 1;
            var proxy = Duck.Cast<IAdder>(target);
            Assert.AreEqual(3, proxy.AddOne(2));
        }

        [Test]
        public void can_cast_to_simple_generic_interface_method()
        {
            Func<int, int> target = n => n + 1;
            var proxy = Duck.Cast<IAdder<int>>(target);
            Assert.AreEqual(3, proxy.AddOne(2));
        }

        [Test]
        public void can_cast_proxy_to_another_interface_supported_by_wrapped_object()
        {
            Func<int, int> target = n => n + 1;
            var proxy = Duck.Cast<IAdder>(target);
            var proxy2 = Duck.Cast<IAdder<int>>(proxy); // note: cast the proxy
            Assert.AreEqual(3, proxy2.AddOne(2));
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