#if UNITTESTS
using System;
using System.Linq;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BluePrism.DatabaseInstaller.UnitTests
{
    [TestFixture]
    class DatabaseScriptLoaderTests : UnitTestBase<DatabaseScriptLoader>
    {
        [Test]
        public void GetLatestUpgradeVersion_NoUpgradesHaveOccurred_GetsSomething()
        {
            GetMock<IEmbeddedResourceLoader>()
               .Setup(m => m.GetResourceNames())
               .Returns(new string[] {});

            Action getVersion = () => ClassUnderTest.GetLatestUpgradeVersion();
            getVersion.ShouldThrow<InvalidOperationException>();
        }
        
        [Test]
        public void GetLatestUpgradeVersion_SinlgeUpgradeHasOccurred_GetsThatVersion()
        {
            GetMock<IEmbeddedResourceLoader>()
               .Setup(m => m.GetResourceNames())
               .Returns(new[] { "BluePrism.DatabaseInstaller.Scripts.db_upgradeR99.sql" });

            ClassUnderTest.GetLatestUpgradeVersion().Should().Be(99);
        }

        [Test]
        public void GetLatestUpgradeVersion_MultipleUpgradesHaveOccurred_GetCorrectVersion()
        {
            GetMock<IEmbeddedResourceLoader>()
               .Setup(m => m.GetResourceNames())
               .Returns(new[] {
                "BluePrism.DatabaseInstaller.Scripts.db_upgradeR1.sql",
                                  "BluePrism.DatabaseInstaller.Scripts.db_upgradeR99.sql",
                                  "BluePrism.DatabaseInstaller.Scripts.db_upgradeR26.sql",
                                  "BluePrism.DatabaseInstaller.Scripts.db_upgradeR247.sql",
                                  "BluePrism.DatabaseInstaller.Scripts.db_upgradeR6.sql" });

            ClassUnderTest.GetLatestUpgradeVersion().Should().Be(247);
        }

        [Test]
        public void GetUpgradeSql_UnknownFile_IsEmpty()
        {
            GetMock<IEmbeddedResourceLoader>()
               .Setup(m => m.GetResourceContent(It.IsAny<string>()))
               .Returns(string.Empty);

            ClassUnderTest.GetUpgradeScript(1).Should().BeNull();
        }

        [Test]
        public void GetUpgradeSql_KnownFile_GetsCorrectContent()
        {
            var fileContent = "Some content for the file";

            GetMock<IEmbeddedResourceLoader>()
               .Setup(m => m.GetResourceContent("BluePrism.DatabaseInstaller.Scripts.db_upgradeR99.sql"))
               .Returns(fileContent);

            ClassUnderTest.GetUpgradeScript(99).SqlStatements.Single().Should().Be(fileContent);
        }

        [Test]
        public void GetDescribeSql_GetsCorrentContent()
        {
            var fileContent = "Some content for the file";

            GetMock<IEmbeddedResourceLoader>()
               .Setup(m => m.GetResourceContent("BluePrism.DatabaseInstaller.Scripts.DescribeDB.sql"))
               .Returns(fileContent);

            ClassUnderTest.GetDescribeDbScript().SqlStatements.Single().Should().Be(fileContent);
        }

        [Test]
        public void GetUpgradeScriptName_HasCorrectFormat()
        {            
            GetMock<IEmbeddedResourceLoader>()
               .Setup(m => m.GetResourceContent("BluePrism.DatabaseInstaller.Scripts.db_upgradeR3.sql"))
               .Returns("content");

            ClassUnderTest.GetUpgradeScript(3).Name.Should().Be("db_upgradeR3.sql");
        }

        [Test]
        public void GetResetInstallMarkerSql_GetsCorrentSql()
        {
            ClassUnderTest.GetResetInstallMarkerSql().Should().Be("if (select InstallInProgress from BPVScriptEnvironment) =  1 "
            + "alter table BPASysConfig drop column InstallInProgress;");
        }

        [Test]
        public void GetCreateScript()
        {
            var fileContent = "Some content for the file";

            GetMock<IEmbeddedResourceLoader>()
               .Setup(m => m.GetResourceContent("BluePrism.DatabaseInstaller.Scripts.db_createR10.sql"))
               .Returns(fileContent);

            ClassUnderTest.GetCreateScript().Name.Should().Be("db_createR10.sql");
            ClassUnderTest.GetCreateScript().SqlStatements.Single().Should().Be(fileContent);
        }
    }
}
#endif