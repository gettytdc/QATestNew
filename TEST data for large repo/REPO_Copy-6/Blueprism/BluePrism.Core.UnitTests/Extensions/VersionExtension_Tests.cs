using BluePrism.Core.Extensions;
using NUnit.Framework;
using System;

namespace BluePrism.Core.UnitTests.Extensions
{
    [TestFixture]
    public class VersionExtension_Tests
    {
        [Test]
        public void ValidVersionStringReturnsValidVersion()
        {
            const string versionString = "1.1.1.1";

            var expectedVersion = new Version(1, 1, 1, 1);
            
            Assert.IsTrue(VersionExtensions.TryParseVersionString(versionString, out var actualVersion));
            Assert.AreEqual(expectedVersion, actualVersion);
        }

        [Test]
        public void ValidFireFoxVersionStringReturnsValidVersion()
        {
            const string versionString = "1.1.1.1sdfgfsd";

            var expectedVersion = new Version(1, 1, 1, 1);

            Assert.IsTrue(VersionExtensions.TryParseVersionString(versionString, out var actualVersion));
            Assert.AreEqual(expectedVersion, actualVersion);
        }

        [Test]
        public void InValidVersionStringReturnsFalse()
        {
            const string versionString = "1.a.1.1sdfgfsd";

            Assert.IsFalse(VersionExtensions.TryParseVersionString(versionString, out var actualVersion));
            Assert.IsNull(actualVersion);
        }

        [Test]
        public void EmptyVersionStringReturnsFalse()
        {
            var versionString = string.Empty;

            Assert.IsFalse(VersionExtensions.TryParseVersionString(versionString, out var actualVersion));
            Assert.IsNull(actualVersion);
        }
    }
}
