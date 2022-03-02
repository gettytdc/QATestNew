using BluePrism.Utilities.Testing;
using NUnit.Framework;
using System;
using System.Threading;

namespace BluePrism.Caching.UnitTests
{
    [TestFixture]
    public class AutoExpireInMemoryCacheTests : UnitTestBase<AutoExpireInMemoryCache>
    {
        public override void Setup() => base.Setup();

        protected override AutoExpireInMemoryCache TestClassConstructor() =>
            new AutoExpireInMemoryCache(Guid.NewGuid().ToString());

        public override void TearDown()
        {
            base.TearDown();
            ClassUnderTest.Dispose();
        }

        [Test]
        public void SetValueNullKeyException() =>
            Assert.Throws<ArgumentNullException>(() => ClassUnderTest.SetValue(null, null));

        [Test]
        public void SetValueNegativeExpirySeconds() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => ClassUnderTest.SetValue("key1", "value1", -1));

        [Test]
        public void SetValueNeverExpire()
        {
            ClassUnderTest.SetValue("key2", "value2", 0);
            Assert.AreEqual("value2", ClassUnderTest.GetValue("key2", null));
            Thread.Sleep(1000);
            Assert.AreEqual("value2", ClassUnderTest.GetValue("key2", null));
        }

        [Test]
        public void SetValueExpireValue()
        {
            var called = false;
            ClassUnderTest.SetValue("key3", "value3", 1);
            Assert.AreEqual("value3", ClassUnderTest.GetValue("key3", null));
            Thread.Sleep(2000);
            var value = ClassUnderTest.GetValue("key3", () =>
                {
                    called = true;
                    return "updatedValue";
                });
            Assert.AreEqual("updatedValue", value);
            Assert.IsTrue(called);
        }

        [Test]
        public void GetValueNullKeyException() =>
            Assert.Throws<ArgumentNullException>(() => ClassUnderTest.GetValue(null, null));

        [Test]
        public void GetValueNegativeExpirySeconds() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => ClassUnderTest.GetValue("key4", null, -1));
    }
}
