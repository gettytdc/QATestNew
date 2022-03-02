#if UNITTESTS

using System;
using System.Collections.Generic;
using NUnit.Framework;
using BluePrism.Utilities.Testing;
using Moq;
using FluentAssertions;
using System.Data;
using System.Linq;

namespace BluePrism.DatabaseInstaller.UnitTests
{
    [TestFixture]
    public class DatabaseTests : UnitTestBase<Database>
    {
        Mock<ISqlDatabaseConnectionSetting> _settings;

        protected override Database TestClassConstructor()
        {
            return new Database(
                GetMock<ISqlConnectionFactory>().Object,
                GetMock<IDatabaseScriptLoader>().Object,
                _settings.Object,
                TimeSpan.FromSeconds(5));
        }

        public override void Setup()
        {
            base.Setup();

            _settings = GetMock<ISqlDatabaseConnectionSetting>();

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetResetInstallMarkerSql())
                .Returns("Install SQL");
        }

        private void SetUpValidConnection()
        {
            GetMock<ISqlConnectionFactory>()
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns(GetMock<IDbConnection>().Object);
        }

        [Test]
        public void AnnotateDB_RunsCorrectCommand()
        {
            SetUpValidConnection();

            var command = new Command();

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(SetUpCommand(command));

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetDescribeDbScript())
                .Returns(DatabaseInstallerScript.Parse("Describe","SOME SQL"));

            ClassUnderTest.AnnotateDatabase();

            command.Text.Should().Be("SOME SQL");
        }

        [Test]
        public void ValidVersionTableExists_RunsCorrectCommand()
        {
            SetUpValidConnection();

            _settings
                .Setup(s => s.DatabaseName)
                .Returns("DBNAME");

            var command = new Command();
            var parameters = new Dictionary<string, object>();
            var mockCommand = Moq.Mock.Get(SetUpCommand(command, parameters));
            mockCommand
                .Setup(c => c.ExecuteScalar())
                .Returns(1);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.ValidVersionTableExists();

            command.Text.Should().Be("declare @sql nvarchar(max); set @sql = N'select 1 from ' + quotename(@dbname) + N'.INFORMATION_SCHEMA.COLUMNS C WHERE C.TABLE_NAME = ''BPADBVersion'' AND C.COLUMN_NAME = ''dbversion'' '; exec(@sql);");

            parameters.Should().HaveCount(1);
            parameters["@dbname"].Should().Be("DBNAME");
        }

        [Test]
        public void ValidVersionTableExists_DoesExist_ReturnsTrue()
        {
            SetUpValidConnection();

            _settings
                .Setup(s => s.DatabaseName)
                .Returns("DBNAME");

            var mockCommand = Moq.Mock.Get(SetUpCommand(new Command()));
            mockCommand
                .Setup(c => c.ExecuteScalar())
                .Returns(1);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);
            
            ClassUnderTest.ValidVersionTableExists()
                    .Should().BeTrue();
        }

        [Test]
        public void ValidVersionTableExists_DoesNotExist_ReturnsFalse()
        {
            SetUpValidConnection();

            _settings
                .Setup(s => s.DatabaseName)
                .Returns("DBNAME");

            var mockCommand = Moq.Mock.Get(SetUpCommand(new Command()));
            mockCommand
                .Setup(c => c.ExecuteScalar())
                .Returns(null);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.ValidVersionTableExists()
                    .Should().BeFalse();
        }

        [Test]
        public void AllowSnapshotIsolation_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(SetUpCommand(command));

            ClassUnderTest.AllowSnapshotIsolation();

            command.Text.Should().Be("alter database CURRENT set ALLOW_SNAPSHOT_ISOLATION on");
        }

        [Test]
        public void AllowSnapshotIsolation_ErrorThrownInExecution_CatchesSilently()
        {
            GetMock<ISqlConnectionFactory>()
                .Setup(f => f.Create(It.IsAny<string>()))
                .Throws<Exception>();

            Action action = () => ClassUnderTest.AllowSnapshotIsolation();

            action.ShouldNotThrow();
        }

        [Test]
        public void ConfigureAdminUser_RunsExpectedQueries()
        {
            SetUpValidConnection();

            var createUserCommand = new Command();
            var createUserParams = new Dictionary<string, object>();
            var createUserRoleCommand = new Command();

            GetMock<IDbConnection>()
                .SetupSequence(c => c.CreateCommand())
                .Returns(SetUpCommand(createUserCommand, createUserParams))
                .Returns(SetUpCommand(createUserRoleCommand));

            ClassUnderTest.ConfigureAdminUser();

            createUserCommand.Text.Should().Be(" declare @userid uniqueidentifier = newid();" +
                        " insert into BPAUser (userid, username, validfromdate, validtodate, passwordexpirydate, useremail, authtype) " +
                        " values (@userid, @username, getutcdate(),'20991231',@validtodate,'', '1');" +
                        " insert into BPAPassword (active,type,hash,salt,userid,lastuseddate)" +
                        " values (1,1,@hash,@salt,@userid,null);");

            createUserParams.Should().HaveCount(4);
            createUserParams["@hash"].Should().NotBeNull();
            createUserParams["@salt"].Should().NotBeNull();
            createUserParams["@username"].Should().NotBeNull();
            createUserParams["@validtodate"].Should().NotBeNull();

            createUserRoleCommand.Text.Should().Be("insert into BPAUserRoleAssignment (userid,userroleid) (select userid, 1 from BPAUser where username=@username);");
        }

        [Test]
        public void CreateAdminUser_RunsExpectedQueries()
        {
            SetUpValidConnection();

            var createUserCommand = new Command();
            var createUserParams = new Dictionary<string, object>();
            var createUserRoleCommand = new Command();

            GetMock<IDbConnection>()
                .SetupSequence(c => c.CreateCommand())
                .Returns(SetUpCommand(createUserCommand, createUserParams))
                .Returns(SetUpCommand(createUserRoleCommand));

            ClassUnderTest.CreateAdminUser();

            createUserCommand.Text.Should().Be("insert into BPAUser (UserId, Username, Password, ValidFromDate, ValidToDate, PasswordExpiryDate, useremail) " +
                    "values (NewID(), @username,'208512264222772174181102151942010236531331277169151',GetDate(),'20991231',GetUTCDate(),'');");

            createUserParams.Should().HaveCount(1);
            createUserParams["@username"].Should().NotBeNull();

            createUserRoleCommand.Text.Should().Be("insert into BPAUserRole (userid,roleid) (select userid,1 from BPAUser);");
        }

        [Test]
        public void DeleteDatabase_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();
            var parameters = new Dictionary<string, object>();

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(SetUpCommand(command, parameters));

            _settings
                .Setup(s => s.DatabaseName)
                .Returns("DBNAME");

            ClassUnderTest.DeleteDatabase();

            command.Text.Should().Be("declare @sql nvarchar(max); set @sql = N'alter database ' + quotename(@dbname) +     N' set offline with rollback immediate; ' + N'alter database ' + quotename(@dbname) +     N' set online; ' + N'drop database ' + quotename(@dbname) + N'; '; exec(@sql); ");

            parameters.Should().HaveCount(1);
            parameters["@dbname"].Should().Be("DBNAME");
        }

        [Test]
        public void InitialiseDatabase_RunsExpectedQueries()
        {
            SetUpValidConnection();

            var createCommand = new Command();
            var createParmameters = new Dictionary<string, object>();
            var snapshotCommand = new Command();

            GetMock<IDbConnection>()
                .SetupSequence(c => c.CreateCommand())
                .Returns(SetUpCommand(createCommand, createParmameters))
                .Returns(SetUpCommand(snapshotCommand));

            _settings
                .Setup(s => s.DatabaseName)
                .Returns("DBNAME");

            ClassUnderTest.InitialiseDatabase();

            createCommand.Text
                .Should().Be("DECLARE @sql NVARCHAR(MAX); SET @sql = N'CREATE DATABASE ' + QUOTENAME(@dbname) + N';'; exec(@sql);");

            createParmameters.Should().HaveCount(1);
            createParmameters["@dbname"].Should().Be("DBNAME");

            snapshotCommand.Text
                .Should().Be("alter database CURRENT set ALLOW_SNAPSHOT_ISOLATION on");
        }

        [Test]
        public void ExecuteCreateScript_CannotGetScriptContent_ThrowsException()
        {
            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetCreateScript())
                .Returns(DatabaseInstallerScript.Parse("Create", string.Empty));

            Action execute = () => ClassUnderTest.ExecuteCreateScript();
            execute.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void ExecuteCreateScript_FindsScriptContent_RunsExpectedScript()
        {
            SetUpValidConnection();

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetCreateScript())
                .Returns(DatabaseInstallerScript.Parse("Create", "Create script content"));

            var command = new Command();

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(SetUpCommand(command));

            var reportedMessages = new List<string>();

            ClassUnderTest.ReportProgress += (_, e) => reportedMessages.Add($"{e.PercentProgress} {e.Message}");

            ClassUnderTest.ExecuteCreateScript();

            command.Text.Should().Be("Create script content");
            reportedMessages.Should().HaveCount(1);
            reportedMessages[0].Should().Be("0 Script 'Create' Part 1 of 1");
        }

        [Test]
        public void EnableValidationCheckForExceptionTypes_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(SetUpCommand(command));

            ClassUnderTest.EnableValidationCheckForExceptionTypes();

            command.Text.Should().Be("update BPAValCheck SET enabled = 1 where checkid = 142;");
        }

        [Test]
        public void ConfigureForActiveDirectory_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();
            var parameters = new Dictionary<string, object>();

            GetMock<IDbConnection>()
                .SetupSequence(c => c.CreateCommand())
                .Returns(SetUpCommand(command, parameters));
            var adSettings = new DatabaseActiveDirectorySettings("DOMAIN", "ADMINGROUPID", "ADMINGROUPNAME", "ADMINGROUPPATH", "ADMINROLE");
            ClassUnderTest.ConfigureForActiveDirectory(adSettings, "SSOEVENTCODE");

            command.Text
                .Should().Be("update BPASysConfig set activedirectoryprovider = @domain; " + 
                " update BPAUserRole set ssogroup = @admingroup where name = @adminrole; " + 
                " insert into BPAAuditEvents(eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) " + 
                " values(getutcdate(), @eventcode, @narrative, @srcuserid, @tgtresourceid, @comments); ");

            parameters.Should().HaveCount(8);
            parameters["@domain"].Should().Be("DOMAIN");
            parameters["@admingroup"].Should().Be("ADMINGROUPID");
            parameters["@adminrole"].Should().Be("ADMINROLE");
            parameters["@eventcode"].Should().Be("SSOEVENTCODE");
            parameters["@narrative"].Should().Be("Database created with Single Sign-on configuration");
            parameters["@srcuserid"].Should().Be(Guid.Empty);
            parameters["@tgtresourceid"].Should().Be(Guid.Empty);
            parameters["@comments"].Should().Be(
                "New Active Directory configuration is (Domain: DOMAIN, ADMINROLE Role: ADMINGROUPID, ADMINGROUPNAME, ADMINGROUPPATH)");
        }
        
        [Test]
        public void UpgradeDatabase_ExecutesAllExpectedScripts()
        {
            SetUpValidConnection();

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetUpgradeScript(It.IsAny<int>()))
                .Returns<int>(scriptNumber => DatabaseInstallerScript.Parse(scriptNumber.ToString(), $"{scriptNumber} content"));

            var executedCommands = new List<string>();

            Func<IDbCommand> createCommand = () =>
            {
                var mockCommand = Moq.Mock.Get(SetUpCommand(new Command()));

                mockCommand
                .SetupSet(c => c.CommandText = It.IsAny<string>())
                .Callback<string>(text => executedCommands.Add(text));

                SetUpCommandToReturnInstalledVersions(mockCommand, new[] { 12, 13, 15, 16, 19 });

                return mockCommand.Object;
            };

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(createCommand());

            GetMock<IDbConnection>()
                .Setup(c => c.BeginTransaction())
                .Returns(GetMock<IDbTransaction>().Object);

            ClassUnderTest.UpgradeDatabase(20);

            var expectedCommands = new List<string> { "select count([ActiveTransactions].transaction_id) " +
                            " from sys.dm_tran_active_transactions [ActiveTransactions]  " +
                            " left outer join sys.dm_tran_database_transactions [DatabaseTransactions] " +
                                " on ([ActiveTransactions].transaction_id = [DatabaseTransactions].transaction_id) " +
                            " left outer join sys.databases as [Databases] " +
                                " on [DatabaseTransactions].database_id = [Databases].database_id " +
                            " where [Databases].name = @dbname " +
                            " and [state] = 7 ",
                                                        "SELECT dbversion FROM bpadbversion WHERE CONVERT(INT, dbversion) > 10",
                                                        "11 content" ,
                                                        "14 content" ,
                                                        "17 content" ,
                                                        "18 content" ,
                                                        "20 content",
                                                        "alter database CURRENT set ALLOW_SNAPSHOT_ISOLATION on"};

            executedCommands.ShouldBeEquivalentTo(expectedCommands);
        }

        [Test]
        public void UpgradeDatabase_VersionOver218_ResetsSqlMarker()
        {
            SetUpValidConnection();

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetUpgradeScript(It.IsAny<int>()))
                .Returns<int>(version => DatabaseInstallerScript.Parse(version.ToString(), $"{version} content"));

            var executedCommands = new List<string>();

            Func<IDbCommand> createCommand = () =>
            {
                var mockCommand = Moq.Mock.Get(SetUpCommand(new Command()));
                mockCommand
                .SetupSet(c => c.CommandText = It.IsAny<string>())
                .Callback<string>(text => executedCommands.Add(text));

                SetUpCommandToReturnInstalledVersions(mockCommand, Enumerable.Range(11, 207).ToArray());

                return mockCommand.Object;
            };

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(createCommand());

            GetMock<IDbConnection>()
                .Setup(c => c.BeginTransaction())
                .Returns(GetMock<IDbTransaction>().Object);

            ClassUnderTest.UpgradeDatabase(220);

            var expectedCommands = new List<string> { "select count([ActiveTransactions].transaction_id) " +
                            " from sys.dm_tran_active_transactions [ActiveTransactions]  " +
                            " left outer join sys.dm_tran_database_transactions [DatabaseTransactions] " +
                                " on ([ActiveTransactions].transaction_id = [DatabaseTransactions].transaction_id) " +
                            " left outer join sys.databases as [Databases] " +
                                " on [DatabaseTransactions].database_id = [Databases].database_id " +
                            " where [Databases].name = @dbname " +
                            " and [state] = 7 ",
                                                        "SELECT dbversion FROM bpadbversion WHERE CONVERT(INT, dbversion) > 10",
                                                        "218 content" ,
                                                        "219 content" ,
                                                        "220 content" ,
                                                        "Install SQL",
                                                        "alter database CURRENT set ALLOW_SNAPSHOT_ISOLATION on"};

            executedCommands.ShouldBeEquivalentTo(expectedCommands);
        }

        [Test]
        public void UpgradeDatabase_ActiveTransactionExists_ThrowsError()
        {
            SetUpValidConnection();

            var mockCommand = Moq.Mock.Get(SetUpCommand(new Command()));

            mockCommand
               .Setup(c => c.ExecuteScalar())
               .Returns(1);
            
            GetMock<IDbConnection>()
                    .Setup(c => c.CreateCommand())
                    .Returns(mockCommand.Object);

            GetMock<IDbConnection>()
                .Setup(c => c.BeginTransaction())
                .Returns(GetMock<IDbTransaction>().Object);

            Action upgrade = () => ClassUnderTest.UpgradeDatabase(20);
            upgrade.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void ConfigureDatabaseInstallerOptions_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var addColumnCommand = new Command();
            var populateColumnCommand = new Command();
            var parameters = new Dictionary<string, object>();

            GetMock<IDbConnection>()
                .SetupSequence(c => c.CreateCommand())
                .Returns(SetUpCommand(addColumnCommand))
                .Returns(SetUpCommand(populateColumnCommand, parameters));

            ClassUnderTest.ConfigureDatabaseInstallerOptions(DatabaseInstallerOptions.MigrateSessionsPre65);

            addColumnCommand.Text.Should().Be(" IF COL_LENGTH('BPASysConfig', 'DatabaseInstallerOptions') IS NULL " +
                                                    "   ALTER TABLE BPASysConfig ADD DatabaseInstallerOptions INT ");

            populateColumnCommand.Text.Should().Be(" UPDATE BPASysConfig SET DatabaseInstallerOptions = @options ");
            parameters.Should().HaveCount(1);
            parameters["@options"].Should().Be(DatabaseInstallerOptions.MigrateSessionsPre65);
        }

        [Test]
        public void CheckCanOpenDatabase_Opens_IsTrue()
        {
            SetUpValidConnection();

            _settings
                .Setup(s => s.IsComplete)
                .Returns(true);

            ClassUnderTest.CheckCanOpenDatabase().Should().BeTrue();
        }

        [Test]
        public void CheckDatabaseExists_SettingsAreNotComplete_IsFalse()
        {
            _settings
                .Setup(s => s.IsComplete)
                .Returns(false);

            ClassUnderTest.CheckDatabaseExists("").Should().BeFalse();
        }

        [Test]
        public void CheckDatabaseExists_RunsExpectedQuery()
        {
            SetUpValidConnection();

            _settings
               .Setup(s => s.IsComplete)
                .Returns(true);

            var command = new Command();
            var parameters = new Dictionary<string, object>();
            var mockCommand = SetUpCommand(command, parameters);

            Moq.Mock.Get(mockCommand)
                .Setup(c => c.ExecuteScalar())
                .Returns(1);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand);

            ClassUnderTest.CheckDatabaseExists("DBNAME");

            parameters.Should().HaveCount(1);
            parameters["@dbname"].Should().Be("DBNAME");

            command.Text.Should().Be("SELECT count(*) FROM master.dbo.sysdatabases WHERE name = @dbname");
        }

        [Test]
        public void CheckDatabaseExists_FindAtLeastOne_IsTrue()
        {
            SetUpValidConnection();

            _settings
                .Setup(s => s.IsComplete)
                .Returns(true);

            var command = SetUpCommand(new Command());

            Moq.Mock.Get(command)
                .Setup(c => c.ExecuteScalar())
                .Returns(1);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(command);

            ClassUnderTest.CheckDatabaseExists("DBNAME").Should().BeTrue();
        }

        [Test]
        public void CheckDatabaseExists_FindNone_IsFalse()
        {
            SetUpValidConnection();

            _settings
                .Setup(s => s.IsComplete)
                .Returns(true);
            
            var command = SetUpCommand(new Command());

            Moq.Mock.Get(command)
                .Setup(c => c.ExecuteScalar())
                .Returns(0);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(command);

            ClassUnderTest.CheckDatabaseExists("DBNAME").Should().BeFalse();
        }

        [Test]
        public void GetCurrentVersion_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            var mockCommand = Moq.Mock.Get(SetUpCommand(command));

            mockCommand
                .Setup(c => c.ExecuteScalar())
                .Returns(212);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.GetCurrentVersion().Should().Be(212);
            command.Text.Should().Be("SELECT TOP 1 dbversion FROM bpadbversion ORDER BY CONVERT(INT,dbversion) DESC");
        }

        [Test]
        public void GetSessionLogRowCount_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            var mockCommand = Moq.Mock.Get(SetUpCommand(command));
            
            mockCommand
                .Setup(c => c.ExecuteScalar())
                .Returns(100L);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.GetSessionLogRowCount()
                .Should().Be(100);

            command.Text.Should().Be(" SELECT SUM(row_count) " +
                    " FROM sys.dm_db_partition_stats " +
                    " WHERE object_id IN (OBJECT_ID('BPASessionLog_NonUnicode'), OBJECT_ID('BPASessionLog_Unicode')) AND [index_id] = 1");
        }

        [Test]
        public void GetSessionLogSize_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            var mockCommand = Moq.Mock.Get(SetUpCommand(command));

            mockCommand
                .Setup(c => c.ExecuteScalar())
                .Returns(100L);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.GetSessionLogSizeKb()
                .Should().Be(100);

            command.Text.Should().Be(" SELECT SUM(reserved_page_count) * 8 " +
                    " FROM sys.dm_db_partition_stats " +
                    " WHERE object_id IN (OBJECT_ID('BPASessionLog_NonUnicode'), OBJECT_ID('BPASessionLog_Unicode')) ");
        }

        [Test]
        public void GetIsActiveDirectory_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            var mockCommand = Moq.Mock.Get(SetUpCommand(command));

            mockCommand
                .Setup(c => c.ExecuteScalar())
                .Returns("thisProvider");

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.GetIsActiveDirectory()
                .Should().BeTrue();

            command.Text.Should().Be("SELECT activedirectoryprovider FROM BPASysConfig");
        }

        [Test]
        public void GetIsActiveDirectory_ExceptionThrown_ReturnsFalse()
        {
            GetMock<ISqlConnectionFactory>()
                .Setup(f => f.Create(It.IsAny<string>()))
                .Throws<Exception>();

            ClassUnderTest.GetIsActiveDirectory().Should().BeFalse();
        }

        [Test]
        public void GetNumberOfUsers_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            var mockCommand = Moq.Mock.Get(SetUpCommand(command));

            mockCommand
                .Setup(c => c.ExecuteScalar())
                .Returns(55);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.GetNumberOfUsers()
                .Should().Be(55);

            command.Text.Should().Be("SELECT COUNT(*) FROM BPAUser WHERE username IS NOT NULL");
        }

        [Test]
        public void GetNumberOfUsers_ExceptionThrown_Is0()
        {
            GetMock<ISqlConnectionFactory>()
                .Setup(f => f.Create(It.IsAny<string>()))
                .Throws<Exception>();

            ClassUnderTest.GetNumberOfUsers().Should().Be(0);
        }

        [Test]
        public void GetInstalledVersions_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            var mockCommand = Moq.Mock.Get(SetUpCommand(command));

            SetUpCommandToReturnInstalledVersions(mockCommand, new[] { 12, 14, 15, 16, 19 });

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.GetInstalledVersions()
                .Should().BeEquivalentTo(new List<int> { 12, 14, 15, 16, 19 });

            command.Text.Should().Be("SELECT dbversion FROM bpadbversion WHERE CONVERT(INT, dbversion) > 10");
        }

        [Test]
        public void GetInstalledVersions_ErrorOccurs_ThrowsException()
        {
            GetMock<ISqlConnectionFactory>()
                .Setup(f => f.Create(It.IsAny<string>()))
                .Throws<Exception>();

            Action getVersions = () => ClassUnderTest.GetInstalledVersions();
            getVersions.ShouldThrow<DatabaseInstallerException>();
        }

        [Test]
        public void GetTableDescriptionInfo_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(SetUpCommand(command));

            ClassUnderTest.GetTableDescriptionInfo()
                .Should().BeEmpty();

            command.Text.Should().Be(
                  " SELECT" +
                  "   TABLE_NAME as [ObjectName]," +
                  "   isnull(objprop.value,'') as [ObjectDescription]," +
                  "   cols.ORDINAL_POSITION as [ColumnNo]," +
                  "   clmns.name as [ColumnName]," +
                  "   RTRIM(CAST(ISNULL(colprop.value,'') AS VARCHAR(1000))) as [ColumnDescription]," +
                  "   ISNULL(idxcol.index_column_id, 0) as [PrimaryKey]," +
                  "   ISNULL(" +
                  "     (SELECT TOP 1 1" +
                  "      FROM sys.foreign_key_columns AS fkclmn " +
                  "      WHERE (fkclmn.parent_column_id = clmns.column_id) " +
                  "        AND fkclmn.parent_object_id = clmns.object_id" +
                  "     ), 0) as [ForeignKey]," +
                  "   udt.name + " +
                  "     case " +
                  "      when typ.name IN (N'nchar', N'nvarchar') AND clmns.max_length <> -1 AND clmns.max_length <> 2" +
                  "       then '(' + CONVERT(varchar, clmns.max_length/2) + ')'" +
                  "      when typ.name IN (N'char', N'varchar') AND clmns.max_length <> -1 AND clmns.max_length <> 1" +
                  "       then '(' + CONVERT(varchar, clmns.max_length) + ')'" +
                  "      else ''" +
                  "     end as [DataType]," +
                  "   clmns.is_nullable as [Null]," +
                  "   isnull(CAST(cnstr.definition AS VARCHAR(20)),'') as [Default]" +
                  " FROM INFORMATION_SCHEMA.COLUMNS cols" +
                  $"   join ( select name, object_id from sys.tables ) obj on obj.name = cols.table_name" +
                  "   JOIN sys.all_columns AS clmns ON clmns.object_id=obj.object_id and clmns.name = cols.COLUMN_NAME" +
                  "   LEFT JOIN sys.indexes AS idx ON idx.object_id = clmns.object_id AND 1 =idx.is_primary_key " +
                  "   LEFT JOIN sys.index_columns AS idxcol ON idxcol.index_id = idx.index_id " +
                  "     AND idxcol.column_id = clmns.column_id " +
                  "     AND idxcol.object_id = clmns.object_id " +
                  "     AND 0 = idxcol.is_included_column " +
                  "   LEFT JOIN sys.types AS udt ON udt.user_type_id = clmns.user_type_id " +
                  "   LEFT JOIN sys.types AS typ ON typ.user_type_id = clmns.system_type_id " +
                  "     AND typ.user_type_id = typ.system_type_id " +
                  "   LEFT JOIN sys.default_constraints AS cnstr ON cnstr.object_id=clmns.default_object_id " +
                  "   LEFT JOIN sys.extended_properties colprop ON colprop.major_id = clmns.object_id " +
                  "     AND colprop.minor_id = clmns.column_id " +
                  "     AND colprop.name = 'MS_Description'" +
                  "   LEFT JOIN sys.extended_properties objprop ON objprop.major_id = obj.object_id " +
                  "     AND objprop.minor_id = 0" +
                  "     AND objprop.name = 'MS_Description'" +
                  " ORDER BY ObjectName, cols.ORDINAL_POSITION ASC");
        }

        [Test]
        public void GetTableDescriptionInfo_ConvertsQueryResultsCorrectly()
        {
            SetUpValidConnection();

            var queryData = new DataTable();

            queryData.Columns.Add("ObjectName");
            queryData.Columns.Add("ObjectDescription");
            queryData.Columns.Add("ColumnNo", typeof(int));
            queryData.Columns.Add("ColumnName");
            queryData.Columns.Add("ColumnDescription");
            queryData.Columns.Add("PrimaryKey", typeof(int));
            queryData.Columns.Add("ForeignKey", typeof(int));
            queryData.Columns.Add("DataType");
            queryData.Columns.Add("Null", typeof(bool));
            queryData.Columns.Add("Default");

            queryData.Rows.Add("obj1", "desc", 1, "col1", "coldesc", 2, 1, "string", true, "default");
            queryData.Rows.Add("obj2", "desc", 2, "col1", "coldesc", 1, 0, "string", false, "");
            queryData.Rows.Add("obj2", "desc", 3, "col2", "coldesc", 0, 1, "string", true, "");
            queryData.Rows.Add("obj3", "desc", 4, "col1", "coldesc", 0, 0, "string", false, "default");

            var queryResults = new DataTableReader(queryData);

            var mockCommand = GetMock<IDbCommand>();
            mockCommand
                .Setup(c => c.ExecuteReader())
                .Returns(queryResults);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            var obj1 = new ObjectDescriptionInfo
            {
                Name = "obj1",
                Description = "desc"
            };

            obj1.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "col1",
                Description = "coldesc",
                PrimaryKey = 2,
                ForeignKey = 1,
                DataType = "string",
                Nullable = true,
                Default = "default"
            });

            var obj2 = new ObjectDescriptionInfo
            {
                Name = "obj2",
                Description = "desc"
            };

            obj2.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "col1",
                Description = "coldesc",
                PrimaryKey = 1,
                ForeignKey = 0,
                DataType = "string",
                Nullable = false,
                Default = ""
            });

            obj2.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "col2",
                Description = "coldesc",
                PrimaryKey = 0,
                ForeignKey = 1,
                DataType = "string",
                Nullable = true,
                Default = ""
            });

            var obj3 = new ObjectDescriptionInfo
            {
                Name = "obj3",
                Description = "desc"
            };

            obj3.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "col1",
                Description = "coldesc",
                PrimaryKey = 0,
                ForeignKey = 0,
                DataType = "string",
                Nullable = false,
                Default = "default"
            });

            ClassUnderTest.GetTableDescriptionInfo()
                .ShouldBeEquivalentTo(new[] { obj1, obj2, obj3 });
        }

        [Test]
        public void GetViewDescriptionInfo_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(SetUpCommand(command));

            ClassUnderTest.GetViewDescriptionInfo()
                .Should().BeEmpty();

            command.Text.Should().Be(" SELECT" +
                  "   TABLE_NAME as [ObjectName]," +
                  "   isnull(objprop.value,'') as [ObjectDescription]," +
                  "   cols.ORDINAL_POSITION as [ColumnNo]," +
                  "   clmns.name as [ColumnName]," +
                  "   RTRIM(CAST(ISNULL(colprop.value,'') AS VARCHAR(1000))) as [ColumnDescription]," +
                  "   ISNULL(idxcol.index_column_id, 0) as [PrimaryKey]," +
                  "   ISNULL(" +
                  "     (SELECT TOP 1 1" +
                  "      FROM sys.foreign_key_columns AS fkclmn " +
                  "      WHERE (fkclmn.parent_column_id = clmns.column_id) " +
                  "        AND fkclmn.parent_object_id = clmns.object_id" +
                  "     ), 0) as [ForeignKey]," +
                  "   udt.name + " +
                  "     case " +
                  "      when typ.name IN (N'nchar', N'nvarchar') AND clmns.max_length <> -1 AND clmns.max_length <> 2" +
                  "       then '(' + CONVERT(varchar, clmns.max_length/2) + ')'" +
                  "      when typ.name IN (N'char', N'varchar') AND clmns.max_length <> -1 AND clmns.max_length <> 1" +
                  "       then '(' + CONVERT(varchar, clmns.max_length) + ')'" +
                  "      else ''" +
                  "     end as [DataType]," +
                  "   clmns.is_nullable as [Null]," +
                  "   isnull(CAST(cnstr.definition AS VARCHAR(20)),'') as [Default]" +
                  " FROM INFORMATION_SCHEMA.COLUMNS cols" +
                  $"   join ( select name, object_id from sys.views ) obj on obj.name = cols.table_name" +
                  "   JOIN sys.all_columns AS clmns ON clmns.object_id=obj.object_id and clmns.name = cols.COLUMN_NAME" +
                  "   LEFT JOIN sys.indexes AS idx ON idx.object_id = clmns.object_id AND 1 =idx.is_primary_key " +
                  "   LEFT JOIN sys.index_columns AS idxcol ON idxcol.index_id = idx.index_id " +
                  "     AND idxcol.column_id = clmns.column_id " +
                  "     AND idxcol.object_id = clmns.object_id " +
                  "     AND 0 = idxcol.is_included_column " +
                  "   LEFT JOIN sys.types AS udt ON udt.user_type_id = clmns.user_type_id " +
                  "   LEFT JOIN sys.types AS typ ON typ.user_type_id = clmns.system_type_id " +
                  "     AND typ.user_type_id = typ.system_type_id " +
                  "   LEFT JOIN sys.default_constraints AS cnstr ON cnstr.object_id=clmns.default_object_id " +
                  "   LEFT JOIN sys.extended_properties colprop ON colprop.major_id = clmns.object_id " +
                  "     AND colprop.minor_id = clmns.column_id " +
                  "     AND colprop.name = 'MS_Description'" +
                  "   LEFT JOIN sys.extended_properties objprop ON objprop.major_id = obj.object_id " +
                  "     AND objprop.minor_id = 0" +
                  "     AND objprop.name = 'MS_Description'" +
                  " ORDER BY ObjectName, cols.ORDINAL_POSITION ASC");
        }

        [Test]
        public void GetViewDescriptionInfo_ConvertsQueryResultsCorrectly()
        {
            SetUpValidConnection();

            var queryData = new DataTable();

            queryData.Columns.Add("ObjectName");
            queryData.Columns.Add("ObjectDescription");
            queryData.Columns.Add("ColumnNo", typeof(int));
            queryData.Columns.Add("ColumnName");
            queryData.Columns.Add("ColumnDescription");
            queryData.Columns.Add("PrimaryKey", typeof(int));
            queryData.Columns.Add("ForeignKey", typeof(int));
            queryData.Columns.Add("DataType");
            queryData.Columns.Add("Null", typeof(bool));
            queryData.Columns.Add("Default");

            queryData.Rows.Add("obj1", "desc", 1, "col1", "coldesc", 2, 1, "string", true, "default");
            queryData.Rows.Add("obj2", "desc", 2, "col1", "coldesc", 1, 0, "string", false, "");
            queryData.Rows.Add("obj2", "desc", 3, "col2", "coldesc", 0, 1, "string", true, "");
            queryData.Rows.Add("obj3", "desc", 4, "col1", "coldesc", 0, 0, "string", false, "default");

            var queryResults = new DataTableReader(queryData);

            var mockCommand = GetMock<IDbCommand>();
            mockCommand
                .Setup(c => c.ExecuteReader())
                .Returns(queryResults);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            var obj1 = new ObjectDescriptionInfo
            {
                Name = "obj1",
                Description = "desc"
            };

            obj1.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "col1",
                Description = "coldesc",
                PrimaryKey = 2,
                ForeignKey = 1,
                DataType = "string",
                Nullable = true,
                Default = "default"
            });

            var obj2 = new ObjectDescriptionInfo
            {
                Name = "obj2",
                Description = "desc"
            };

            obj2.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "col1",
                Description = "coldesc",
                PrimaryKey = 1,
                ForeignKey = 0,
                DataType = "string",
                Nullable = false,
                Default = ""
            });

            obj2.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "col2",
                Description = "coldesc",
                PrimaryKey = 0,
                ForeignKey = 1,
                DataType = "string",
                Nullable = true,
                Default = ""
            });

            var obj3 = new ObjectDescriptionInfo
            {
                Name = "obj3",
                Description = "desc"
            };

            obj3.ColumnDescriptions.Add(new ColumnDescriptionInfo
            {
                Name = "col1",
                Description = "coldesc",
                PrimaryKey = 0,
                ForeignKey = 0,
                DataType = "string",
                Nullable = false,
                Default = "default"
            });

            ClassUnderTest.GetViewDescriptionInfo()
                .ShouldBeEquivalentTo(new List<ObjectDescriptionInfo> { obj1, obj2, obj3 });
        }

        [Test]
        public void GetVersionDescriptions_RunsExpectedQuery()
        {
            SetUpValidConnection();

            var command = new Command();

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(SetUpCommand(command));

            ClassUnderTest.GetVersionDescriptions()
                .Should().BeEmpty();

            command.Text.Should().Be("SELECT * from BPADBVersion order by convert(int,dbversion)");
        }

        [Test]
        public void GetVersionDescriptions_ConvertsQueryResultsCorrectly()
        {
            SetUpValidConnection();

            var queryData = new DataTable();

            queryData.Columns.Add("dbversion");
            queryData.Columns.Add("description");

            queryData.Rows.Add("ver1", "desc1");
            queryData.Rows.Add("ver2", "desc2");

            var queryResults = new DataTableReader(queryData);

            var mockCommand = GetMock<IDbCommand>();
            mockCommand
                .Setup(c => c.ExecuteReader())
                .Returns(queryResults);

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            ClassUnderTest.GetVersionDescriptions()
                .ShouldBeEquivalentTo(new List<VersionDescriptionInfo> { new VersionDescriptionInfo
            {
                Version = "ver1",
                Description = "desc1"
            }, new VersionDescriptionInfo
            {
                Version = "ver2",
                Description = "desc2"
            } });
        }

        [Test]
        public void UpgradeDuringCancel_NotCancelling_DoesNotInvokeEvent()
        {
            SetUpValidConnection();

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetUpgradeScript(It.IsAny<int>()))
                .Returns<int>(scriptNumber => DatabaseInstallerScript.Parse(scriptNumber.ToString(), $"{scriptNumber} content"));

            var mockCommand = Moq.Mock.Get(SetUpCommand(new Command()));

            SetUpCommandToReturnInstalledVersions(mockCommand, new[] { 13 });

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            GetMock<IDbConnection>()
                .Setup(c => c.BeginTransaction())
                .Returns(GetMock<IDbTransaction>().Object);

            var receivedStatuses = new List<CancelStatus>();

            ClassUnderTest.CancelProgress += (_, e) => receivedStatuses.Add(e.Status);

            ClassUnderTest.UpgradeDatabase(11);

            receivedStatuses.Should().BeEmpty();
        }

        [Test]
        public void UpgradeDuringCancel_CancellingNotCancelled_EvokesEventCorrectly()
        {
            SetUpValidConnection();

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetUpgradeScript(It.IsAny<int>()))
                .Returns<int>(scriptNumber => DatabaseInstallerScript.Parse(scriptNumber.ToString(), $"{scriptNumber} content"));

            var mockCommand = Moq.Mock.Get(SetUpCommand(new Command()));

            SetUpCommandToReturnInstalledVersions(mockCommand, new[] { 11 });

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            GetMock<IDbConnection>()
                .Setup(c => c.BeginTransaction())
                .Returns(GetMock<IDbTransaction>().Object);

            var receivedStatuses = new List<CancelStatus>();

            ClassUnderTest.CancelProgress += (_, e) => receivedStatuses.Add(e.Status);
            
            ClassUnderTest.IsCancelling = true;
            ClassUnderTest.UpgradeDatabase(13);

            receivedStatuses.First().Should().Be(CancelStatus.Cancelled);
            receivedStatuses.Last().Should().Be(CancelStatus.Continued);
        }

        [Test]
        public void UpgradeDuringCancel_CancellingAndCancelled_EvokesEventCorrectlyAndThrows()
        {
            SetUpValidConnection();

            GetMock<IDatabaseScriptLoader>()
                .Setup(l => l.GetUpgradeScript(It.IsAny<int>()))
                .Returns<int>(scriptNumber => DatabaseInstallerScript.Parse(scriptNumber.ToString(), $"{scriptNumber} content"));

            var mockCommand = Moq.Mock.Get(SetUpCommand(new Command()));
            SetUpCommandToReturnInstalledVersions(mockCommand, new[] { 11 });

            GetMock<IDbConnection>()
                .Setup(c => c.CreateCommand())
                .Returns(mockCommand.Object);

            GetMock<IDbConnection>()
                .Setup(c => c.BeginTransaction())
                .Returns(GetMock<IDbTransaction>().Object);

            var receivedStatuses = new List<CancelStatus>();

            ClassUnderTest.CancelProgress += (_, e) => receivedStatuses.Add(e.Status);

            ClassUnderTest.IsCancelling = true;
            ClassUnderTest.IsCancelled = true;
            Action upgrade = () => ClassUnderTest.UpgradeDatabase(13);

            upgrade.ShouldThrow<DatabaseInstallerException>();
            receivedStatuses.First().Should().Be(CancelStatus.Cancelled);
            receivedStatuses.Last().Should().Be(CancelStatus.CancelConfirmed);
        }

        private void SetUpCommandToReturnInstalledVersions(Mock<IDbCommand> command, int[] versions)
        {
            command
                .Setup(c => c.ExecuteScalar())
                .Returns(0);

            var installedVersions = new Queue<int>();

            foreach (var version in versions)
                installedVersions.Enqueue(version);

            var reader = GetMock<IDataReader>();
            reader
                .Setup(r => r.Read())
                .Returns(() => { return installedVersions.Any(); });

            reader
                .Setup(r => r.GetValue(0))
                .Returns(() => { return installedVersions.Dequeue(); });

            command
                .Setup(c => c.ExecuteReader())
                .Returns(reader.Object);
        }

        private IDbCommand SetUpCommand(Command command)
            => SetUpCommand(command, new Dictionary<string, object>());

        private IDbCommand SetUpCommand(Command command, Dictionary<string, object> parameters)
        {
            var mockCommand = new Mock<IDbCommand>();

            mockCommand
                .SetupSet(c => c.CommandText = It.IsAny<string>())
                .Callback<string>(text => command.Text = text);

            var mockParameters = new Mock<IDataParameterCollection>();
            mockParameters
                .Setup(p => p.Add(It.IsAny<IDataParameter>()))
                .Callback<object>(p =>
                {
                    var param = (IDataParameter)p;
                    parameters.Add(param.ParameterName, param.Value);
                });

            mockCommand
               .SetupGet(c => c.Parameters)
               .Returns(mockParameters.Object);

            mockCommand
               .Setup(c => c.CreateParameter())
               .Returns(Moq.Mock.Of<IDbDataParameter>());

            return mockCommand.Object;
        }

        private class Command
        {
            public string Text;
        }
    }
}

#endif
