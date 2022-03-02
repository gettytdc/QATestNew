#if UNITTESTS
using System;
using BluePrism.AutomateProcessCore.Compilation;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests.Compilation
{
    public class ObjectCacheTests
    {
        [Test]
        public void Add_StoresValueType() => TestAddAndGet(1);

        [Test]
        public void Add_StoresReferenceType() => TestAddAndGet(new object());

        [Test]
        public void Get_WithIncorrectType_ShouldThrow()
        {
            var cache = CreateCache();
            cache.Add("key1", 1);
            Action action = () => cache.Get<string>("key1");
            action.ShouldThrow<InvalidCastException>();
        }

        private static ObjectCache CreateCache() => new ObjectCache(new InMemoryCacheStore());

        private static void TestAddAndGet<T>(T value)
        {
            var cache = CreateCache();
            cache.Add("key1", value);
            var retrievedValue = cache.Get<T>("key1");
            retrievedValue.Should().Be(value);
        }
    }
}
#endif
