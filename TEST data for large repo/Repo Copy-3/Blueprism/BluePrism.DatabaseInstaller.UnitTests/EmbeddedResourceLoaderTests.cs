#if UNITTESTS
using BluePrism.Utilities.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace BluePrism.DatabaseInstaller.UnitTests
{
    [TestFixture]
    class EmbeddedResourceLoaderTests : UnitTestBase<EmbeddedResourceLoader>
    {
        [Test]
        public void GetResourceContent_FileInRootFolder_ReadsContent()
        {
            var content = ClassUnderTest.GetResourceContent("BluePrism.DatabaseInstaller.Scripts.DescribeDB.sql");
            content.TrimStart().Should().StartWith("IF OBJECT_ID('desc_table') IS NOT NULL");
            content.TrimEnd().Should().EndWith("--END");
        }

        [Test]
        public void GetResourceContent_FileInSubFolder_ReadsContent()
        {
            var content = ClassUnderTest.GetResourceContent("BluePrism.DatabaseInstaller.Scripts.db_upgradeR112.sql");
            content.TrimStart().Should().StartWith("/*" + Environment.NewLine + "SCRIPT         : 112");
            content.TrimEnd().Should().EndWith("'Increased the permitted length of work queue item tags'" + Environment.NewLine + ");");
        }

        [Test]
        public void GetResourceContent_UnknownFile_ReturnsEmpty()
        {
            ClassUnderTest.GetResourceContent("BluePrism.DatabaseInstaller.Scripts.UNKNOWN.sql").Should().BeNull();
        }

        [Test]
        public void GetResourceNames_IncludesExpectedFiles()
        {
            var resources = ClassUnderTest.GetResourceNames();
                
            resources.Should().Contain("BluePrism.DatabaseInstaller.Scripts.DescribeDB.sql");
            resources.Should().Contain("BluePrism.DatabaseInstaller.Scripts.db_upgradeR112.sql");
        }
    }
}
#endif