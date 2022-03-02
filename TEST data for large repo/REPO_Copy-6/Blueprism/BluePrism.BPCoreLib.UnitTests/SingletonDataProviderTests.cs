#if UNITTESTS

using System;
using BluePrism.BPCoreLib.Data;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class SingletonDataProviderTests
    {
        [Test]
        public void TestNullNameThrowsException()
        {
            Assert.That(() => new SingletonDataProvider(null, "Anything").ToString(), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void TestNullValueIsValid()
        {
            var sp = new SingletonDataProvider("Nada", null);
            Assert.That(sp.GetGuid("Nada"), Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void TestNamedValueReturnsCorrectly()
        {
            var sp = new SingletonDataProvider("Thing", "This is a thing");
            Assert.That(sp.GetString("Not a thing"), Is.Null);
            Assert.That(sp.GetString("Thing"), Is.EqualTo("This is a thing"));
        }
    }
}

#endif
