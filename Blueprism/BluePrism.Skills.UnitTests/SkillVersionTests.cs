#if UNITTESTS
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Drawing;
using System.IO;

namespace BluePrism.Skills.UnitTests
{
    class SkillVersionTests
    {
        [Test]
        public void Constructor_ValidData_InstantiatesCorrectly()
        {
            var id = Guid.NewGuid();
            var classUnderTest = new TestSkillVersion(
                                    "TestSkill",
                                    SkillCategory.VisualPerception,
                                    "1.1", 
                                    "This is a description",
                                    new Bitmap(10,10),
                                    "1.2.3",
                                    "1.2.3",
                                    new DateTime(2012, 1, 1));

            classUnderTest.Should().NotBeNull();
            classUnderTest.ImportedAt.Should().Be(new DateTime(2012, 1, 1));
        }

        [TestCase("", "valid", "valid", SkillCategory.VisualPerception, "valid", "valid")]
        [TestCase(null, "valid", "valid", SkillCategory.VisualPerception, "valid", "valid")]
        [TestCase("valid", "", "valid", SkillCategory.VisualPerception, "valid", "valid")]
        [TestCase("valid", null, "valid", SkillCategory.VisualPerception, "valid", "valid")]
        [TestCase("valid", "valid", "", SkillCategory.VisualPerception, "valid", "valid")]
        [TestCase("valid", "valid", null, SkillCategory.VisualPerception, "valid", "valid")]
        [TestCase("valid", "valid", "valid", SkillCategory.Unknown, "valid", "valid")]
        [TestCase("valid", "valid", "valid", SkillCategory.VisualPerception, "", "valid")]
        [TestCase("valid", "valid", "valid", SkillCategory.VisualPerception, null, "valid")]
        [TestCase("valid", "valid", "valid", SkillCategory.VisualPerception, "valid", "")]
        [TestCase("valid", "valid", "valid", SkillCategory.VisualPerception, "valid", null)]

        public void Constructor_InvalidProvider_Throws(
                                        string version,
                                        string name,
                                        string description,
                                        SkillCategory category,
                                        string bpCreatedVersion,
                                        string bpTestedVersion
            )
        {
            Action constructs = () => new TestSkillVersion(
                                            name,
                                            category, 
                                            version,
                                            description,
                                            new Bitmap(10, 10),
                                            bpCreatedVersion,
                                            bpTestedVersion,
                                            new DateTime(2012,1,1));

            constructs.ShouldThrow<ArgumentNullException>();
        }

        private class TestSkillVersion : SkillVersion
        {
            public TestSkillVersion(string name, SkillCategory category, string versionNumber, string description, Image icon, string bpVersionCreated, string bpVersionTested, DateTime importedAt) :
                base(name, category, versionNumber, description, GetBytesFromImage(icon), bpVersionCreated, bpVersionTested, importedAt)
            {
            }

            private static byte[] GetBytesFromImage(Image image)
            {
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                    return stream.ToArray();
                }
            }
        }
    }

       
}
            

#endif