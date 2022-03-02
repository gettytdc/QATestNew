#if UNITTESTS

using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;


namespace BluePrism.Skills.UnitTests
{
    [TestFixture]
    public class SkillsTests
    {
        [Test]
        public void Constructor_ValidData_InstantiatesCorrectly()
        {
            var id = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var classUnderTest = new Skill(id, "Provider", true, new List<SkillVersion>());
            classUnderTest.Should().NotBeNull();

            classUnderTest.Id.Should().Be(id);
            classUnderTest.Provider.Should().Be("Provider");
            classUnderTest.Enabled.Should().BeTrue();
        }

        [TestCase("")]
        [TestCase(null)]
        public void Constructor_InvalidProvider_Throws(string provider)
        {
            Action constructs = () => new Skill(Guid.NewGuid(), provider, true, new List<SkillVersion>());
            constructs.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void LatestVersion_MultipleVersions_IsCorrect()
        {
            var version1 = CreateVersion(new DateTime(2011, 1, 1));
            var version2 = CreateVersion(new DateTime(2012, 1, 1));
            var version3 = CreateVersion(new DateTime(2013, 1, 1));

            var versions = new[] {version1, version3, version2};
            var classUnderTest = new Skill(Guid.NewGuid(), "Provider", true, versions);

            classUnderTest.Versions.ShouldBeEquivalentTo(versions);
            classUnderTest.LatestVersion.ShouldBeEquivalentTo(version3);
        }

        private WebSkillVersion CreateVersion(DateTime importedAt)
        {
            return new WebSkillVersion(
                Guid.NewGuid(),
                "webapi name",
                true,
                "name",
                SkillCategory.Collaboration,
                "1.1",
                "description",
                null,
                "1.1",
                "1.2",
                importedAt,
                new List<string>());
        }
    }
}
#endif