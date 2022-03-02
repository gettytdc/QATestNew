#if UNITTESTS

using BluePrism.Utilities.Testing;
using NUnit.Framework;
using System;
using Moq;
using System.Text;
using FluentAssertions;
using System.Collections.Generic;
using BluePrism.DatabaseInstaller.Data;

namespace BluePrism.DatabaseInstaller.UnitTests
{
    [TestFixture]
    public class InstallerTests : UnitTestBase<Installer>
    {
        protected override Installer TestClassConstructor()
        {
            return new Installer(
                GetMock<ISqlDatabaseConnectionSetting>().Object,
                TimeSpan.FromSeconds(5),
                "APPLICATION NAME",
                "SSO_EVENT_CODE",
                (_, __) => GetMock<IDatabase>().Object,
                GetMock<IDatabaseScriptGenerator>().Object,
                GetMock<ISqlConnectionWrapper>().Object,
                GetMock<IDatabaseScriptLoader>().Object);
        }

        public override void Setup()
        {
            base.Setup();

            GetMock<ISqlDatabaseConnectionSetting>()
                .Setup(s => s.ConnectionString)
                .Returns("");

            GetMock<ISqlDatabaseConnectionSetting>()
                .Setup(s => s.MasterConnectionString)
                .Returns("");

            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Returns(true);

            GetMock<IDatabase>()
                .Setup(l => l.ValidVersionTableExists())
                .Returns(true);
        }

        [Test]
        public void AnnotateDatabase_ExecutesDescribeDB()
        {
            ClassUnderTest.AnnotateDatabase();
            GetMock<IDatabase>()
                .Verify(p => p.AnnotateDatabase());
        }

        [Test]
        public void AnnotateDatabase_ExceptionIsThrown_ThrowsException()
        {
            GetMock<IDatabase>()
                .Setup(f => f.AnnotateDatabase())
                .Throws(new Exception());

            Action annotate = () => ClassUnderTest.AnnotateDatabase();
            annotate.ShouldThrow<Exception>();
        }

        [Test]
        public void CheckDatabaseExists_CannotCheckMasterDB_ChecksLocalDBCanBeOpened()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Throws(new Exception());

           ClassUnderTest.CheckDatabaseExists();

            GetMock<IDatabase>()
                .Verify(q => q.CheckCanOpenDatabase());
        }

        [Test]
        public void CheckDatabaseExists_ErrorWithConnectionString_Throws()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Throws(new ArgumentException());

            Action check = () => ClassUnderTest.CheckDatabaseExists();

            check.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CheckDatabaseExists_ErrorWithOpeningLocalDB_Throws()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Throws(new Exception());

            GetMock<IDatabase>()
                .Setup(q => q.CheckCanOpenDatabase())
                .Throws(new Exception());

            Action check = () => ClassUnderTest.CheckDatabaseExists();

            check.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CheckDatabaseExists_DoesExist_IsTrue()
            => ClassUnderTest.CheckDatabaseExists().Should().BeTrue();

        [Test]
        public void CheckDatabaseExists_DoesNotExist_IsFalse()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Returns(false);

            ClassUnderTest.CheckDatabaseExists().Should().BeFalse();
        }

        [Test]
        public void CheckDatabaseExists_StateUnknown_CanConnectLocally_IsTrue()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Throws<Exception>();

            GetMock<IDatabase>()
                .Setup(q => q.CheckCanOpenDatabase())
                .Returns(true);

            ClassUnderTest.CheckDatabaseExists().Should().BeTrue();
        }

        [Test]
        public void CheckDatabaseExists_StateUnknown_CannotConnectLocally_IsFalse()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Throws<Exception>();

            GetMock<IDatabase>()
                .Setup(q => q.CheckCanOpenDatabase())
                .Returns(false);

            ClassUnderTest.CheckDatabaseExists().Should().BeFalse();
        }

        [Test]
        public void IsUpgradeAvailable_CurrentVersionIsLatestVersion_IsFalse()
            => ClassUnderTest.IsUpgradeAvailable(11, 11).Should().BeFalse();

        [Test]
        public void IsUpgradeAvailable_CurrentVersionIsMoreThanLatestVersion_IsFalse()
            => ClassUnderTest.IsUpgradeAvailable(12, 11).Should().BeFalse();

        [Test]
        public void IsUpgradeAvailable_CurrentVersionIsLessThanLatestVersion_IsTrue()
            => ClassUnderTest.IsUpgradeAvailable(10, 11).Should().BeTrue();

        [Test]
        public void IsUpgradeAvailable_CurrentVersionIsLessThan10_IsFalse()
            => ClassUnderTest.IsUpgradeAvailable(9, 11).Should().BeFalse();

        [Test]
        public void GetRequiredVersion_GetsLatestVersion()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(100);

            ClassUnderTest.GetRequiredDBVersion().Should().Be(100);
        }

        [Test]
        public void GetCurrentVersion_GetsDBVersion()
        {
            GetMock<IDatabase>()
                .Setup(l => l.GetCurrentVersion())
                .Returns(90);

            ClassUnderTest.GetCurrentDBVersion().Should().Be(90);
        }

        [Test]
        public void GetCurrentVersion_DatabaseIsEmpty_ThrowsException()
        {
            GetMock<IDatabase>()
                .Setup(l => l.ValidVersionTableExists())
                .Returns(false);

            Action getVersion = () => ClassUnderTest.GetCurrentDBVersion();
            getVersion.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void GetCurrentVersion_ExceptionIsThrown_VersionIsZero()
        {
            GetMock<IDatabase>()
                .Setup(l => l.GetCurrentVersion())
                .Throws(new Exception());

            ClassUnderTest.GetCurrentDBVersion().Should().Be(0);
        }

        [Test]
        public void GetSessionRowCount_ReturnsCount()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetSessionLogRowCount())
                .Returns(100);

            ClassUnderTest.GetSessionLogRowCount()
                .Should().Be(100);
        }

        [Test]
        public void GetSessionRowSize_ReturnsSize()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetSessionLogSizeKb())
                .Returns(100);

            ClassUnderTest.GetSessionLogSizeKB()
                .Should().Be(100);
        }

        [Test]
        public void CheckIntegrity_DatabaseDoesNotExist_ThrowsException()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Returns(false);

            Action checkIntegrity = () => ClassUnderTest.CheckIntegrity();
            checkIntegrity.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CheckIntegrity_CurrentVersionLessThanRequired_ThrowsException()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(11);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(12);

            Action checkIntegrity = () => ClassUnderTest.CheckIntegrity();
            checkIntegrity.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CheckIntegrity_CurrentVersionMoreThanRequired_ThrowsException()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(12);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(11);

            Action checkIntegrity = () => ClassUnderTest.CheckIntegrity();
            checkIntegrity.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CheckIntegrity_NoUsers_NotAD_ThrowsException()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(12);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(12);

            GetMock<IDatabase>()
                .Setup(q => q.GetIsActiveDirectory())
                .Returns(false);

            GetMock<IDatabase>()
                 .Setup(q => q.GetNumberOfUsers())
                 .Returns(0);

            Action checkIntegrity = () => ClassUnderTest.CheckIntegrity();
            checkIntegrity.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CheckIntegrity_NoUsers_IsAD_DoesNotThrowException()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(12);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(12);

            GetMock<IDatabase>()
                .Setup(q => q.GetIsActiveDirectory())
                .Returns(true);

            GetMock<IDatabase>()
                 .Setup(q => q.GetNumberOfUsers())
                 .Returns(0);

            Action checkIntegrity = () => ClassUnderTest.CheckIntegrity();
            checkIntegrity.ShouldNotThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CheckIntegrity_CurrentVersionIsRequiredVersion_HasUsers_DoesNotThrow()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(12);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(12);

            GetMock<IDatabase>()
                .Setup(q => q.GetIsActiveDirectory())
                .Returns(false);

            GetMock<IDatabase>()
                 .Setup(q => q.GetNumberOfUsers())
                 .Returns(100);

            Action checkIntegrity = () => ClassUnderTest.CheckIntegrity();
            checkIntegrity.ShouldNotThrow<DatabaseInstallerException>();
        }

        [Test]
        public void SessionLogMigrationRequired_CurrentMoreThanFixedVersion_ReturnsFalse()
            => ClassUnderTest.SessionLogMigrationRequired(296, 301)
                .Should().BeFalse();

        [Test]
        public void SessionLogMigrationRequired_RequiredLessThanFixedVersion_ReturnsFalse()
            => ClassUnderTest.SessionLogMigrationRequired(200, 294)
                .Should().BeFalse();

        [Test]
        public void SessionLogMigrationRequired_CurrentEqualsFixedVersion_ReturnsFalse()
            => ClassUnderTest.SessionLogMigrationRequired(295, 296)
                .Should().BeFalse();

        [Test]
        public void SessionLogMigrationRequired_CurrentLessThanAndRequiredMoreThanFixedVersion_ReturnsTrue()
            => ClassUnderTest.SessionLogMigrationRequired(294, 296)
                .Should().BeTrue();

        [Test]
        public void CreateDatabase_ConfigureOnly_DatabaseDoesNotExist_ThrowsException()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Returns(false);

            Action create = () => ClassUnderTest.CreateDatabase(null, false, true, 0);
            create.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CreateDatabase_NotConfigureOnly_DatabaseExists_DropExisting_DeletesAndSetsUpDatabase()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(60);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(100);

            ClassUnderTest.CreateDatabase(null, true, false, 200);

            GetMock<IDatabase>()
                .Verify(p => p.DeleteDatabase());

            GetMock<IDatabase>()
                .Verify(p => p.InitialiseDatabase());

            GetMock<IDatabase>()
               .Verify(p => p.ExecuteCreateScript());

            GetMock<IDatabase>()
               .Verify(p => p.CreateAdminUser());

            GetMock<IDatabase>()
               .Verify(p => p.UpgradeDatabase(100));

            GetMock<IDatabase>()
               .Verify(p => p.EnableValidationCheckForExceptionTypes());

            GetMock<IDatabase>()
               .Verify(p => p.AllowSnapshotIsolation());
        }

        [Test]
        public void CreateDatabase_NotConfigureOnly_DatabaseDoesNotExist_SetsUpDatabase()
        {
            GetMock<IDatabase>()
                .Setup(q => q.CheckDatabaseExists(It.IsAny<string>()))
                .Returns(false);

            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(60);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(100);

            ClassUnderTest.CreateDatabase(null, false, false, 200);
            
            GetMock<IDatabase>()
               .Verify(p => p.InitialiseDatabase());

            GetMock<IDatabase>()
               .Verify(p => p.ExecuteCreateScript());

            GetMock<IDatabase>()
               .Verify(p => p.UpgradeDatabase(100));

            GetMock<IDatabase>()
               .Verify(p => p.EnableValidationCheckForExceptionTypes());

            GetMock<IDatabase>()
               .Verify(p => p.AllowSnapshotIsolation());
        }

        [Test]
        public void CreateDatabase_ConfigureOnly_NotAD_ConfiguresAdminUser()
        {
            ClassUnderTest.CreateDatabase(null, false, true, 0);

            GetMock<IDatabase>()
                .Verify(q => q.ConfigureAdminUser());
        }

        [Test]
        public void CreateDatabase_NotConfigureOnly_NotAD_CreatesAdminUser()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(60);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(100);

            ClassUnderTest.CreateDatabase(null, false, false, 0);

            GetMock<IDatabase>()
                .Verify(q => q.CreateAdminUser());
        }

        [Test]
        public void CreateDatabase_NotConfigureOnly_UpgradesDatabase()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(60);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(100);

            ClassUnderTest.CreateDatabase(null, false, false, 200);

            GetMock<IDatabase>()
                .Verify(p => p.UpgradeDatabase(100));
        }

        [Test]
        public void CreateDatabase_ConfigureOnly_DoesNotUpgradeDatabase()
        {
            ClassUnderTest.CreateDatabase(null, false, true, 200);

            GetMock<IDatabase>()
                .Verify(p => p.UpgradeDatabase(200), Times.Never);
        }

        [Test]
        public void CreateDatabase_AD_InitialisesAD()
        {
            GetMock<IDatabase>()
                .Setup(q => q.GetCurrentVersion())
                .Returns(60);

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(100);

            var settings = new DatabaseActiveDirectorySettings("DOMAIN", "ADMINGROUPID", "ADMINGROUPNAME", "ADMINGROUPPATH", "ADMINROLE");
            ClassUnderTest.CreateDatabase(settings, false, false, 200);

            GetMock<IDatabase>()
                .Verify(p => p.ConfigureForActiveDirectory(settings, "SSO_EVENT_CODE"));
        }

        [Test]
        public void CreateDatabase_NotConfigureOnly_RequiredVersionIsZero_ThrowsException()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(0);

            Action create = () => ClassUnderTest.CreateDatabase(null, false, false, 200);

            create.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CreateDatabase_NotConfigureOnly_DatabaseIsUpToDate_ThrowsException()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(50);

            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(50);

            Action create = () => ClassUnderTest.CreateDatabase(null, false, false, 200);

            create.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void CreateDatabase_NotConfigureOnly_UpgradePossible_RequiredLessThanMaxRequested_DoesUpgradeToRequired()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(150);

            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(50);

            ClassUnderTest.CreateDatabase(null, false, false, 200);

            GetMock<IDatabase>()
                .Verify(p => p.UpgradeDatabase(150));
        }

        [Test]
        public void CreateDatabase_NotConfigureOnly_UpgradePossible_RequiredMoreThanRequested_DoesUpgradeToRequested()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(250);

            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(50);

            ClassUnderTest.CreateDatabase(null, false, false, 200);

            GetMock<IDatabase>()
                .Verify(p => p.UpgradeDatabase(200));
        }

        [Test]
        public void ReportProgress_EventFires()
        {
            var reports = new List<string>();

            ClassUnderTest.ReportProgress += (_, args) 
                => reports.Add($"{args.PercentProgress} {args.Message}");

            GetMock<IDatabase>()
                .Raise(p => p.ReportProgress += null, new PercentageProgressEventArgs(0,"starting"));
            GetMock<IDatabase>()
                .Raise(p => p.ReportProgress += null, new PercentageProgressEventArgs(50, "in progress"));
            GetMock<IDatabase>()
                .Raise(p => p.ReportProgress += null, new PercentageProgressEventArgs(100, "finished"));

            var expected = new List<string> { "0 starting", "50 in progress", "100 finished" };
            reports.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void CancelProgress_EventFires()
        {
            var reports = new List<CancelStatus>();

            ClassUnderTest.CancelProgress += (_, args)
                => reports.Add(args.Status);

            GetMock<IDatabase>()
                .Raise(p => p.CancelProgress += null, new CancelProgressEventArgs(CancelStatus.Cancelled));
            GetMock<IDatabase>()
                .Raise(p => p.CancelProgress += null, new CancelProgressEventArgs(CancelStatus.CancelConfirmed));
            GetMock<IDatabase>()
                .Raise(p => p.CancelProgress += null, new CancelProgressEventArgs(CancelStatus.Continued));

            var expected = new List<CancelStatus> { CancelStatus.Cancelled, CancelStatus.CancelConfirmed, CancelStatus.Continued };
            reports.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void UpgradeDatabase_CannotDetermineRequiredVersion_ThrowException()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(0);

            Action upgrade = () => ClassUnderTest.UpgradeDatabase(23);
            upgrade.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void UpgradeDatabase_UpgradeIsNotAvailable_ThrowException()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(22);

            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(22);

            Action upgrade = () => ClassUnderTest.UpgradeDatabase(23);
            upgrade.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void UpgradeDatabase_UpgradeIsAvailable_DoesUpgradeToSpecifiedVersion()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(25);

            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(22);

            ClassUnderTest.UpgradeDatabase(24);

            GetMock<IDatabase>()
                .Verify(p => p.UpgradeDatabase(24));
        }

        [Test]
        public void UpgradeDatabase_UpgradeIsAvailable_SpecifiedVersionIsHigherThanAvailable_DoesUpgradeToHighestAvailable()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(23);

            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(22);

            ClassUnderTest.UpgradeDatabase(25);

            GetMock<IDatabase>()
                .Verify(p => p.UpgradeDatabase(23));
        }

        [Test]
        public void ConfigurableDatabaseUpgrade_CannotDetermineRequiredVersion_ThrowException()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(0);

            Action upgrade = () => ClassUnderTest.ConfigurableDatabaseUpgrade(DatabaseInstallerOptions.None);
            upgrade.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void ConfigurableDatabaseUpgrade_UpgradeIsNotAvailable_ThrowException()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(22);

            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(22);

            Action upgrade = () => ClassUnderTest.ConfigurableDatabaseUpgrade(DatabaseInstallerOptions.MigrateSessionsPre65);
            upgrade.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void ConfigurableDatabaseUpgrade_UpgradeIsAvailable_DoesConfigureAndUpgradeToRequiredVersion()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetLatestUpgradeVersion())
                .Returns(25);

            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(22);

            ClassUnderTest.ConfigurableDatabaseUpgrade(DatabaseInstallerOptions.MigrateSessionsPre65);

            GetMock<IDatabase>()
                .Verify(p => p.ConfigureDatabaseInstallerOptions(DatabaseInstallerOptions.MigrateSessionsPre65));

            GetMock<IDatabase>()
                .Verify(p => p.UpgradeDatabase(25));
        }

        [Test]
        public void GetDBDocs_GetsExpectedDocument()
        {
            GetMock<IDatabase>()
               .Setup(q => q.GetCurrentVersion())
               .Returns(22);

            var object1 = new ObjectDescriptionInfo
            {
                Name = "Obj1",
                Description = "ObjDesc1"
            };

            object1.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "Col1",
                DataType = "string",
                Description = "ColDesc1",
                PrimaryKey = 2,
                ForeignKey = 1,
                Nullable = true,
                Default = "ThisIsTheDefault"
            });

            object1.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "Col2",
                DataType = "enum",
                Description = "ColDesc2",
                PrimaryKey = 1,
                ForeignKey = 0,
                Nullable = false,
                Default = ""
            });

            var object2 = new ObjectDescriptionInfo
            {
                Name = "Obj2",
                Description = "ObjDesc2"
            };

            object2.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "Col1",
                DataType = "number",
                Description = "ColDesc1",
                PrimaryKey = 0,
                ForeignKey = 0,
                Nullable = false,
                Default = "ThisIsTheDefault"
            });

            GetMock<IDatabase>()
               .Setup(q => q.GetTableDescriptionInfo())
               .Returns(new List<ObjectDescriptionInfo> { object1, object2 });

            GetMock<IDatabase>()
              .Setup(q => q.GetViewDescriptionInfo())
              .Returns(new List<ObjectDescriptionInfo> { object1, object2 });

            GetMock<IDatabase>()
                .Setup(q => q.GetVersionDescriptions())
                .Returns(new List<VersionDescriptionInfo> {
                            new VersionDescriptionInfo { Version = "VER1", Description = "desc1" },
                    new VersionDescriptionInfo{ Version = "VER2", Description = "desc2" },
                    new VersionDescriptionInfo{ Version = "VER3", Description = "desc3" }});

            var docs = ClassUnderTest.GetDBDocs();
            docs.Should().StartWith("Database Dictionary for database version R22");
            docs.Should().Contain(GetExpectedObjectDescriptions());
            docs.Should().EndWith(GetRevisionHistory());
        }

        private string GetRevisionHistory()
        {
            var result = new StringBuilder();
            result.AppendLine("=Revision History=");
            result.AppendLine("{| border=1");
            result.AppendLine("!Revision");
            result.AppendLine("!Description");
            result.AppendLine("|-");
            result.AppendLine("|VER1");
            result.AppendLine("|desc1");
            result.AppendLine("|-");
            result.AppendLine("|VER2");
            result.AppendLine("|desc2");
            result.AppendLine("|-");
            result.AppendLine("|VER3");
            result.AppendLine("|desc3");
            result.AppendLine("|}");
            return result.ToString();
        }

        private string GetExpectedObjectDescriptions()
        {
            var result = new StringBuilder();

            result.AppendLine("=Tables=");

            result.AppendLine("==Obj1==");
            result.AppendLine("ObjDesc1");
            result.AppendLine("{| border = 1 width=\"100%\"");
            result.AppendLine("! scope=col width = \"20%\" | Field");
            result.AppendLine("! scope=col width = \"10%\" | Type");
            result.AppendLine("! scope=col width = \"55%\" | Description");
            result.AppendLine("! scope=col width = \"5%\" | Key");
            result.AppendLine("! scope=col width = \"5%\" | Null?");
            result.AppendLine("! scope=col width = \"5%\" | Default");
            result.AppendLine("|-");
            result.AppendLine("|Col1");
            result.AppendLine("|string");
            result.AppendLine("|ColDesc1");
            result.AppendLine("|P2F");
            result.AppendLine("|yes");
            result.AppendLine("|ThisIsTheDefault");
            result.AppendLine("|-");
            result.AppendLine("|Col2");
            result.AppendLine("|enum");
            result.AppendLine("|ColDesc2");
            result.AppendLine("|P1");
            result.AppendLine("|no");
            result.AppendLine("|");
            result.AppendLine("|}");

            result.AppendLine("==Obj2==");
            result.AppendLine("ObjDesc2");
            result.AppendLine("{| border = 1 width=\"100%\"");
            result.AppendLine("! scope=col width = \"20%\" | Field");
            result.AppendLine("! scope=col width = \"10%\" | Type");
            result.AppendLine("! scope=col width = \"55%\" | Description");
            result.AppendLine("! scope=col width = \"5%\" | Key");
            result.AppendLine("! scope=col width = \"5%\" | Null?");
            result.AppendLine("! scope=col width = \"5%\" | Default");
            result.AppendLine("|-");
            result.AppendLine("|Col1");
            result.AppendLine("|number");
            result.AppendLine("|ColDesc1");
            result.AppendLine("|");
            result.AppendLine("|no");
            result.AppendLine("|ThisIsTheDefault");
            result.AppendLine("|}");

            result.AppendLine("=Views=");

            result.AppendLine("==Obj1==");
            result.AppendLine("ObjDesc1");
            result.AppendLine("{| border = 1 width=\"100%\"");
            result.AppendLine("! scope=col width = \"20%\" | Field");
            result.AppendLine("! scope=col width = \"10%\" | Type");
            result.AppendLine("! scope=col width = \"55%\" | Description");
            result.AppendLine("! scope=col width = \"5%\" | Key");
            result.AppendLine("! scope=col width = \"5%\" | Null?");
            result.AppendLine("! scope=col width = \"5%\" | Default");
            result.AppendLine("|-");
            result.AppendLine("|Col1");
            result.AppendLine("|string");
            result.AppendLine("|ColDesc1");
            result.AppendLine("|P2F");
            result.AppendLine("|yes");
            result.AppendLine("|ThisIsTheDefault");
            result.AppendLine("|-");
            result.AppendLine("|Col2");
            result.AppendLine("|enum");
            result.AppendLine("|ColDesc2");
            result.AppendLine("|P1");
            result.AppendLine("|no");
            result.AppendLine("|");
            result.AppendLine("|}");

            result.AppendLine("==Obj2==");
            result.AppendLine("ObjDesc2");
            result.AppendLine("{| border = 1 width=\"100%\"");
            result.AppendLine("! scope=col width = \"20%\" | Field");
            result.AppendLine("! scope=col width = \"10%\" | Type");
            result.AppendLine("! scope=col width = \"55%\" | Description");
            result.AppendLine("! scope=col width = \"5%\" | Key");
            result.AppendLine("! scope=col width = \"5%\" | Null?");
            result.AppendLine("! scope=col width = \"5%\" | Default");
            result.AppendLine("|-");
            result.AppendLine("|Col1");
            result.AppendLine("|number");
            result.AppendLine("|ColDesc1");
            result.AppendLine("|");
            result.AppendLine("|no");
            result.AppendLine("|ThisIsTheDefault");
            result.AppendLine("|}");

            return result.ToString();
        }

        [Test]
        public void GenerateInstallerScript_GetsGeneratedScript()
        {
            GetMock<IDatabaseScriptGenerator>()
                .Setup(q => q.GenerateInstallationScript(1,5,true))
                .Returns("correct");

           ClassUnderTest.GenerateInstallerScript(1,5,true)
                .Should().Be("correct");
        }

        [Test]
        public void GenerateCreateScript_GetsGeneratedScript()
        {
            GetMock<IDatabaseScriptGenerator>()
                .Setup(q => q.GenerateInstallationScript(DatabaseAction.Create))
                .Returns("create");

            ClassUnderTest.GenerateCreateScript()
                 .Should().Be("create");
        }

        [Test]
        public void GenerateUpgradeScript_GetsGeneratedScript()
        {
            GetMock<IDatabase>()
                .Setup(a => a.GetInstalledVersions())
                .Returns(new[] { 1, 3, 5 });

            GetMock<IDatabaseScriptGenerator>()
                .Setup(q => q.GenerateInstallationScript(DatabaseAction.Upgrade, new[] { 1, 3, 5 }))
                .Returns("upgrade");

            ClassUnderTest.GenerateUpgradeScript()
                 .Should().Be("upgrade");
        }
    }
}

#endif
