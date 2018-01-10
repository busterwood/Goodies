using BusterWood.Caching;
using NUnit.Framework;
using System;

namespace UnitTests
{
    [TestFixture]
    public class CacheTests
    {
        [Test]
        public void empty_cache_has_count_of_zero()
        {
            var c = NewCache();
            Assert.AreEqual(0, c.Count);
        }

        [Test]
        public void can_add_to_cache_and_read_back()
        {
            var c = NewCache();
            c[2] = "hello";
            Assert.AreEqual("hello", c[2]);
            Assert.AreEqual(1, c.Count, "Count");
        }

        [Test]
        public void can_replace_existing_value()
        {
            var c = NewCache();
            c[2] = "hello";
            c[2] = "world";
            Assert.AreEqual("world", c[2]);
            Assert.AreEqual(1, c.Count, "Count");
        }

        [Test]
        public void can_invalidate()
        {
            var c = NewCache();
            c[2] = "hello";
            Assert.AreEqual(1, c.Count, "Count");
            c.Remove(2);
            Assert.AreEqual(0, c.Count, "Count");
            Assert.AreEqual(null, c[2]);
        }

        [Test]
        public void can_add_and_readback_after_first_collection()
        {
            var c = NewCache();
            c[2] = "hello";
            c.ForceCollect();
            Assert.AreEqual("hello", c[2]);
            Assert.AreEqual(1, c.Count, "Count");
        }

        [Test]
        public void evicted_after_two_collections()
        {
            var c = NewCache();
            c[2] = "hello";
            c.ForceCollect();
            c.ForceCollect();
            Assert.AreEqual(null, c[2]);
            Assert.AreEqual(0, c.Count, "Count");
        }

        static Cache<int, string> NewCache(int gen0 = 10, TimeSpan? ttl = null) => new Cache<int, string>(gen0, ttl);
    }
}
