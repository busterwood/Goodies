using BusterWood.Ducks;
using NUnit.Framework;
using System;

namespace UnitTests
{
    [TestFixture]
    public class StaticMethodTests
    {
        [Test]
        public void can_call_a_void_method_via_proxy()
        {
            TargetSimplist.calls = 0;
            ISimplist proxy = Duck.Cast<ISimplist>(typeof(TargetSimplist));
            proxy.Execute();
            Assert.AreEqual(1, TargetSimplist.calls);
        }

        [Test]
        public void can_call_a_void_method_with_parameter_via_proxy()
        {
            IWithNumber proxy = Duck.Cast<IWithNumber>(typeof(TargetWithParameter));
            proxy.Execute(2);
            Assert.AreEqual(2, TargetWithParameter.Number);
        }

        [Test]
        public void can_call_a_void_method_with_parameter_that_returns_something()
        {
            var proxy = Duck.Cast<IAdder>(typeof(Adder));
            Assert.AreEqual(3, proxy.AddOne(2));
        }

        [Test]
        public void can_proxy_system_io_file()
        {
            var proxy = Duck.Cast<IExister>(typeof(System.IO.File));
            Assert.AreEqual(true, proxy.Exists(@"c:\Windows\notepad.exe"));
        }

        [Test]
        public void proxy_inherited_interface_types()
        {
            var proxy = Duck.Cast<IExistDeleter>(typeof(System.IO.File));
            Assert.AreEqual(true, proxy.Exists(@"c:\Windows\notepad.exe"));
        }

        [Test]
        public void cannot_cast_if_a_target_method_is_missing()
        {
            Assert.Throws<InvalidCastException>(() => Duck.Cast<ISimplist>(typeof(TargetBad)));
        }

        [Test]
        public void can_cast_if_a_target_method_is_missing_but_throwing_not_implemented_is_desired()
        {
            var proxy = Mock.Cast<ISimplist>(typeof(TargetBad));
            Assert.Throws<NotImplementedException>(() => proxy.Execute());
        }

        [Test]
        public void can_duck_type_an_event()
        {
            var proxy = Duck.Cast<IEventer>(typeof(TargetEvent));
            EventHandler hander = (sender, args) => { };
            proxy.SimpleEvent += hander;
            Assert.AreEqual(1, TargetEvent.count);
            proxy.SimpleEvent -= hander;
            Assert.AreEqual(0, TargetEvent.count);
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