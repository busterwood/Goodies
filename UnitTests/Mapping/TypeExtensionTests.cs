using NUnit.Framework;
using System.Text;

namespace BusterWood.Mapping
{
    [TestFixture]
    public class TypeExtensionTests
    {
        [Test]
        public void immutable_when_type_has_no_fields()
        {
            Assert.IsTrue(typeof(Emtpy).IsImmutable());
        }

        [Test]
        public void immutable_when_type_only_has_readonly_fields()
        {
            Assert.IsTrue(typeof(Readonly).IsImmutable());
        }

        [Test]
        public void type_with_one_mutable_field_is_not_immutable()
        {
            Assert.IsFalse(typeof(Mutable).IsImmutable());
        }

        [Test]
        public void immutable_when_type_only_has_readonly_auto_property()
        {
            Assert.IsTrue(typeof(InitAutoProp).IsImmutable());
        }

        [Test]
        public void immutable_when_type_and_base_class_immutable()
        {
            Assert.IsTrue(typeof(Derived).IsImmutable());
        }

        [Test]
        public void not_immutable_when_base_class_not_immutable()
        {
            Assert.IsFalse(typeof(DerivedFromMutable).IsImmutable());
        }

        [Test]
        public void not_immutable_field_type_is_mutable()
        {
            Assert.IsFalse(typeof(MutableFieldType).IsImmutable());
        }

        [Test]
        [Timeout(100)]
        public void can_handle_directly_recursive_types()
        {
            Assert.IsTrue(typeof(Recursive).IsImmutable());
        }

        class Emtpy { }
        class Readonly { readonly int One; }
        class Mutable { readonly int One; int Two; }
        class InitAutoProp { int One { get; }}
        class Derived : Readonly { }
        class DerivedFromMutable : Mutable { }
        class MutableFieldType { readonly StringBuilder One;}
        class Recursive { readonly Recursive rec; }
    }
}
