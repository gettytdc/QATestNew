using BluePrism.Common.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BluePrism.DatabaseInstaller
{
    public class Database : IDatabase
    {
        private const string DefaultAdminUsername = "admin";
        private readonly TimeSpan _minimumSessionLogMigrationTimeout = TimeSpan.FromMinutes(90);
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IDatabaseScriptLoader _scriptLoader;
        private readonly ISqlDatabaseConnectionSetting _settings;
        private TimeSpan _commandTimeout;

        public bool IsCancelling { get; set; }
        public bool IsCancelled { get; set; }

        public Database(
            ISqlConnectionFactory connectionFactory,
            IDatabaseScriptLoader scriptLoader,
            ISqlDatabaseConnectionSetting settings,
            TimeSpan commandTimeout)
        {
            _connectionFactory = connectionFactory;
            _scriptLoader = scriptLoader;
            _settings = settings;
            _commandTimeout = commandTimeout;
        }

        public event PercentageProgressEventHandler ReportProgress;
        public event CancelProgressEventHandler CancelProgress;

        private object ExecuteScalarQuery(string commandText, Dictionary<string, object> parameters, TimeSpan? commandTimeout)
        {
            using (var connection = _connectionFactory.Create(_settings.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    foreach (var parameter in parameters ?? new Dictionary<string, object>())
                        command.Parameters.Add(CreateParameter(command, parameter.Key, parameter.Value));

                    if (commandTimeout != null)
                        command.CommandTimeout = (int)commandTimeout.Value.TotalSeconds;

                    command.CommandText = commandText;

                    object result = new object();
                    try { result = command.ExecuteScalar(); }
                    catch(Exception ex) { throw new DatabaseInstallerException(ex.Message); }

                    return result;
                }
            }
        }

        public void InitialiseDatabase()
        {
            using (var connection = _connectionFactory.Create(_settings.MasterConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.Parameters.Add(CreateParameter(command, "@dbname", _settings.DatabaseName));
                    if (string.IsNullOrEmpty(_settings.DatabaseFilePath))
                    {
                        command.CommandText =
                            "DECLARE @sql NVARCHAR(MAX); SET @sql = " +
                            "N'CREATE DATABASE ' + QUOTENAME(@dbname) + N';'; exec(@sql);";
                    }
                    else
                    {
                        command.Parameters.Add(CreateParameter(command, "@dbfilepath", _settings.DatabaseFilePath));
                        command.CommandText =
                            "DECLARE @sql NVARCHAR(MAX); SET @sql = " +
                            "N'CREATE DATABASE ' + QUOTENAME(@dbname) + N' " +
                            "  ON ( NAME = ' + QUOTENAME(@dbname) + N', " +
                            "       FILENAME = ' + QUOTENAME(@dbfilepath) + N' " +
                            "  );'; exec(@sql);";
                    }
                    command.CommandTimeout = (int)_commandTimeout.TotalSeconds;
                    command.ExecuteNonQuery();
                }
            }

            AllowSnapshotIsolation(_commandTimeout);
        }

        public void DeleteDatabase()
        {
            using (var connection = _connectionFactory.Create(_settings.MasterConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    var parameters = new Dictionary<string, object>();
                    parameters.Add("@dbname", _settings.DatabaseName);

                    foreach (var parameter in parameters)
                        command.Parameters.Add(CreateParameter(command, parameter.Key, parameter.Value));

                    command.CommandText = "declare @sql nvarchar(max); " +
                                            "set @sql = " +
                                                "N'alter database ' + quotename(@dbname) + " +
                                                "    N' set offline with rollback immediate; ' + " +
                                                "N'alter database ' + quotename(@dbname) + " +
                                                "    N' set online; ' + " +
                                                "N'drop database ' + quotename(@dbname) + N'; '; " +
                                            "exec(@sql); ";
                    command.CommandTimeout = (int)_commandTimeout.TotalSeconds;
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AllowSnapshotIsolation()
            => AllowSnapshotIsolation(_commandTimeout);

        private void AllowSnapshotIsolation(TimeSpan? timeout)
        {
            try
            {
                using (var connection = _connectionFactory.Create(_settings.ConnectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        if (timeout != null)
                            command.CommandTimeout = (int)timeout.Value.TotalSeconds;
                        command.CommandText = "alter database CURRENT set ALLOW_SNAPSHOT_ISOLATION on";
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Silently fail if the user doesn't have permission to set ALLOW_SNAPSHOT_ISOLATION
            }
        }

        public void AnnotateDatabase()
        {
            var script = _scriptLoader.GetDescribeDbScript();

            using (var connection = _connectionFactory.Create(_settings.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    foreach (var sql in script.SqlStatements)
                    {
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void ConfigureAdminUser()
        {
            ConfigureAdminUser(null, null);
        }

        public void ConfigureAdminUser(string username, SafeString password)
        {
            var hash = new PBKDF2PasswordHasher().GenerateHash(password ?? new SafeString(DefaultAdminUsername), out string salt);

            var createUserParameters = new Dictionary<string, object>();

            createUserParameters.Add("@username", username ?? DefaultAdminUsername);
            createUserParameters.Add("@hash", hash);
            createUserParameters.Add("@salt", salt);

            var validToDate = DateTime.UtcNow;
            if (password != null) validToDate = validToDate.AddDays(30);
            createUserParameters.Add("@validtodate", validToDate);

            var createUserCommand = " declare @userid uniqueidentifier = newid();" +
                        " insert into BPAUser (userid, username, validfromdate, validtodate, passwordexpirydate, useremail, authtype) " +
                        " values (@userid, @username, getutcdate(),'20991231',@validtodate,'', '1');" +
                        " insert into BPAPassword (active,type,hash,salt,userid,lastuseddate)" +
                        " values (1,1,@hash,@salt,@userid,null);";

            ExecuteScalarQuery(createUserCommand, createUserParameters, _commandTimeout);

            var assignRoleParameters = new Dictionary<string, object>();
            assignRoleParameters.Add("@username", username ?? DefaultAdminUsername);

            var assignRoleCommand = "insert into BPAUserRoleAssignment (userid,userroleid) (select userid, 1 from BPAUser where username=@username);";
            ExecuteScalarQuery(assignRoleCommand, assignRoleParameters, _commandTimeout);
        }

        public void ConfigureForActiveDirectory(DatabaseActiveDirectorySettings activeDirectorySettings, string eventCode)
        {
            var parameters = new Dictionary<string, object>();
            var comment = String.Format(Resources.DatabaseInstaller_NewActiveDirectoryConfigurationIs, activeDirectorySettings.Domain, activeDirectorySettings.AdminRole, activeDirectorySettings.AdminGroupId, activeDirectorySettings.AdminGroupName, activeDirectorySettings.AdminGroupPath);
            parameters.Add("@domain", activeDirectorySettings.Domain);
            parameters.Add("@admingroup", activeDirectorySettings.AdminGroupId);
            parameters.Add("@adminrole", activeDirectorySettings.AdminRole);
            parameters.Add("@eventcode", eventCode);
            parameters.Add("@narrative", Resources.DatabaseInstaller_DatabaseCreatedWithSingleSignOnConfiguration);
            parameters.Add("@srcuserid", Guid.Empty);
            parameters.Add("@tgtresourceid", Guid.Empty);
            parameters.Add("@comments", comment);

            var commandText = "update BPASysConfig set activedirectoryprovider = @domain; " +
                                            " update BPAUserRole set ssogroup = @admingroup where name = @adminrole; " +
                                            " insert into BPAAuditEvents(eventdatetime, sCode, sNarrative, gSrcUserID, gTgtResourceID, comments) " +
                                            " values(getutcdate(), @eventcode, @narrative, @srcuserid, @tgtresourceid, @comments); ";

            ExecuteScalarQuery(commandText, parameters, _commandTimeout);
        }

        private IDataParameter CreateParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }

        public void CreateAdminUser()
        {
            var insertUserCommand = "insert into BPAUser (UserId, Username, Password, ValidFromDate, ValidToDate, PasswordExpiryDate, useremail) " +
                    "values (NewID(), @username,'208512264222772174181102151942010236531331277169151',GetDate(),'20991231',GetUTCDate(),'');";
            var parameters = new Dictionary<string, object>();
            parameters.Add("@username", DefaultAdminUsername);
            ExecuteScalarQuery(insertUserCommand, parameters, _commandTimeout);

            ExecuteScalarQuery("insert into BPAUserRole (userid,roleid) (select userid,1 from BPAUser);", null, _commandTimeout);
        }

        public void SetDefaultPasswordRules()
        => ExecuteScalarQuery("UPDATE BPAPasswordRules SET uppercase = 1,lowercase = 1, digits = 1, length = 8;", null, _commandTimeout);

        public void EnableValidationCheckForExceptionTypes()
            => ExecuteScalarQuery("update BPAValCheck SET enabled = 1 where checkid = 142;", null, _commandTimeout);

        public void ExecuteCreateScript()
        {
            var createScript = _scriptLoader.GetCreateScript();
            if (createScript == null)
                throw new DatabaseInstallerException(Resources.DatabaseInstaller_DatabaseCreationScriptCouldNotBeFound);

            using (var connection = _connectionFactory.Create(_settings.ConnectionString))
            {
                connection.Open();

                for (var i = 0; i < createScript.SqlStatements.Count; i++)
                {
                    using (var command = connection.CreateCommand())
                    {
                        DoReportProgress(createScript.Name, 0, 1, i + 1, createScript.SqlStatements.Count);
                        command.CommandText = createScript.SqlStatements[i];
                        command.CommandTimeout = (int)_commandTimeout.TotalSeconds;
                        command.ExecuteScalar();
                    }
                }
            }
        }

        private void DoReportProgress(
            string name,
            int scriptsComplete,
            int totalScripts,
            int currentPart,
            int totalParts)
        {
            var percentage = (int)scriptsComplete * 100 / totalScripts;
            var message = string.Format(Resources.DatabaseInstaller_Script0Part1Of2, name, currentPart, totalParts);
            ReportProgress?.Invoke(this, new PercentageProgressEventArgs(percentage, message));
        }

        private bool HasActiveTransaction()
        {
            var command = "select count([ActiveTransactions].transaction_id) " +
                            " from sys.dm_tran_active_transactions [ActiveTransactions]  " +
                            " left outer join sys.dm_tran_database_transactions [DatabaseTransactions] " +
                                " on ([ActiveTransactions].transaction_id = [DatabaseTransactions].transaction_id) " +
                            " left outer join sys.databases as [Databases] " +
                                " on [DatabaseTransactions].database_id = [Databases].database_id " +
                            " where [Databases].name = @dbname " +
                            " and [state] = 7 "; // 7 = The transaction is being rolled back.

            var parameters = new Dictionary<string, object>();
            parameters.Add("@dbname", _settings.DatabaseName);

            var result = ExecuteScalarQuery(command, parameters, _commandTimeout);
            return (int)result > 0;
        }

        public void UpgradeDatabase(int version)
        {
            if (HasActiveTransaction())
                throw new DatabaseInstallerException(Resources.DatabaseInstaller_UpgradeCannotBePerformedAtThisTimeAnActiveTransactionIsBeingRolledBack);

            using (var connection = _connectionFactory.Create(_settings.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var batches = GetScriptBatchesToRun(version).ToList();

                    ExecuteBatches(batches, connection, transaction);

                    RemoveInstallInProgressColumn(version, connection, transaction);

                    transaction.Commit();
                }
            }

            AllowSnapshotIsolation(_commandTimeout);
        }

        private void RemoveInstallInProgressColumn(int version, IDbConnection connection, IDbTransaction transaction)
        {
            if (version >= 219)
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandTimeout = (int)_commandTimeout.TotalSeconds;
                    command.CommandText = _scriptLoader.GetResetInstallMarkerSql();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void ExecuteBatches(List<DatabaseInstallerScript> batches, IDbConnection connection, IDbTransaction transaction)
        {
            var totalScripts = batches.Sum(b => b.SqlStatements.Count);
            var currentScriptNumber = 0;

            foreach (var batch in batches)
            {
                for (int scriptIndex = 0; scriptIndex < batch.SqlStatements.Count; scriptIndex++)
                {
                    currentScriptNumber++;
                    HandleCancelStatus();

                    var percentageProgress = 100 * currentScriptNumber / totalScripts;
                    var message = string.Format(Resources.DatabaseInstaller_Script0Part1Of2, batch.Name, scriptIndex + 1, batch.SqlStatements.Count);
                    ReportProgress?.Invoke(this, new PercentageProgressEventArgs(percentageProgress, message));

                    ExecuteScript(connection, transaction, batch.Name, batch.SqlStatements[scriptIndex]);
                }
            }
        }

        private void ExecuteScript(IDbConnection connection, IDbTransaction transaction, string scriptName, string script)
        {
            using (var command = connection.CreateCommand())
            {
                try
                {
                    command.Transaction = transaction;
                    command.CommandTimeout = (int)_commandTimeout.TotalSeconds;
                    command.CommandText = script;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    var errorMessage = ex.Message + Environment.NewLine + Environment.NewLine + string.Format(Resources.DatabaseInstaller_AtStage0, scriptName);
                    throw new DatabaseInstallerException(errorMessage);
                }
            }
        }

        private void HandleCancelStatus()
        {
            if (!IsCancelling)
                return;

            CancelProgress?.Invoke(this, new CancelProgressEventArgs(CancelStatus.Cancelled));

            if (IsCancelled)
            {
                CancelProgress?.Invoke(this, new CancelProgressEventArgs(CancelStatus.CancelConfirmed));
                throw new DatabaseInstallerException(Resources.DatabaseInstallerTheOperationWasCancelled);
            }
            else
                CancelProgress?.Invoke(this, new CancelProgressEventArgs(CancelStatus.Continued));
        }

        private IEnumerable<DatabaseInstallerScript> GetScriptBatchesToRun(int version)
                    => Enumerable.Range(11, version - 10).Except(GetInstalledVersions())
                        .Select(v => _scriptLoader.GetUpgradeScript(v))
                        .Where(batch => batch != null);

        public void ConfigureDatabaseInstallerOptions(DatabaseInstallerOptions options)
        {
            var addColumnCommand = " IF COL_LENGTH('BPASysConfig', 'DatabaseInstallerOptions') IS NULL " +
                "   ALTER TABLE BPASysConfig ADD DatabaseInstallerOptions INT ";

            ExecuteScalarQuery(addColumnCommand, null, _commandTimeout);

            var parameters = new Dictionary<string, object>();
            parameters.Add("@options", options);

            ExecuteScalarQuery(" UPDATE BPASysConfig SET DatabaseInstallerOptions = @options ", parameters, _commandTimeout);

            if (options == DatabaseInstallerOptions.MigrateSessionsPre65 && _commandTimeout < _minimumSessionLogMigrationTimeout)
                _commandTimeout = _minimumSessionLogMigrationTimeout;
        }

        public bool CheckCanOpenDatabase()
        {
            using (var connection = _connectionFactory.Create(_settings.ConnectionString))
            {
                connection.Open();
                return true;
            }
        }

        public bool CheckDatabaseExists(string databaseName)
        {
            if (!_settings.IsComplete) return false;

            using (var connection = _connectionFactory.Create(_settings.MasterConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.Parameters.Add(CreateParameter(command, "@dbname", databaseName));
                    command.CommandText = "SELECT count(*) FROM master.dbo.sysdatabases WHERE name = @dbname";
                    return int.Parse(command.ExecuteScalar().ToString()) > 0;
                }
            }
        }

        public int GetCurrentVersion()
        {
            var command = "SELECT TOP 1 dbversion FROM bpadbversion ORDER BY CONVERT(INT,dbversion) DESC";
            var result = ExecuteScalarQuery(command, null, null);
            return int.Parse(result.ToString());
        }

        public IEnumerable<int> GetInstalledVersions()
        {
            var result = new List<int>();
            try
            {
                using (var connection = _connectionFactory.Create(_settings.ConnectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT dbversion FROM bpadbversion WHERE CONVERT(INT, dbversion) > 10";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var val = reader.GetValue(0).ToString();
                                result.Add(int.Parse(val));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DatabaseInstallerException(string.Format(Resources.DatabaseInstaller_FailedToGetInstalledScriptVersions0, ex.Message));
            }

            return result;
        }

        public bool GetIsActiveDirectory()
        {
            try
            {
                var command = "SELECT activedirectoryprovider FROM BPASysConfig";
                var result = Convert.ToString(ExecuteScalarQuery(command, null, null));
                return !string.IsNullOrEmpty(result);
            }
            catch
            {
                // Dealing with the case where the 'activedirectoryprovider' field
                //doesn't yet exist(old database!)
                return false;
            }
        }

        public int GetNumberOfUsers()
        {
            try
            {
                var command = "SELECT COUNT(*) FROM BPAUser WHERE username IS NOT NULL";
                return (int)ExecuteScalarQuery(command, new Dictionary<string, object>(), null);
            }
            catch
            {
                return 0;
            }
        }

        public long GetSessionLogRowCount()
        {
            var command = " SELECT SUM(row_count) " +
                    " FROM sys.dm_db_partition_stats " +
                    " WHERE object_id IN (OBJECT_ID('BPASessionLog_NonUnicode'), OBJECT_ID('BPASessionLog_Unicode')) AND [index_id] = 1";

            long result = 0;
            long.TryParse(ExecuteScalarQuery(command, null, _commandTimeout).ToString(), out result);
            return result;
        }

        public long GetSessionLogSizeKb()
        {
            var command = " SELECT SUM(reserved_page_count) * 8 " +
                    " FROM sys.dm_db_partition_stats " +
                    " WHERE object_id IN (OBJECT_ID('BPASessionLog_NonUnicode'), OBJECT_ID('BPASessionLog_Unicode')) ";

            long result = 0;
            long.TryParse(ExecuteScalarQuery(command, null, _commandTimeout).ToString(), out result);
            return result;
        }

        public IEnumerable<ObjectDescriptionInfo> GetViewDescriptionInfo()
            => GetObjectDescriptionInfo("views");

        public IEnumerable<ObjectDescriptionInfo> GetTableDescriptionInfo()
            => GetObjectDescriptionInfo("tables");

        private IEnumerable<ObjectDescriptionInfo> GetObjectDescriptionInfo(string type)
        {
            using (var connection = _connectionFactory.Create(_settings.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = GetObjectDescriptionCommand(type);
                    using (var reader = command.ExecuteReader())
                    {
                        return ParseAsObjectDescriptions(reader);
                    }
                }
            }
        }

        private IEnumerable<ObjectDescriptionInfo> ParseAsObjectDescriptions(IDataReader reader)
        {
            var objects = new List<ObjectDescriptionInfo>();

            if (reader is null)
                return objects;

            while (reader.Read())
            {
                var objectName = (string)reader["ObjectName"];

                if (!objects.Any(o => o.Name == objectName))
                    objects.Add(new ObjectDescriptionInfo { Name = objectName, Description = (string)reader["ObjectDescription"] });

                var obj = objects.First(o => o.Name == objectName);

                obj.ColumnDescriptions.Add(new ColumnDescriptionInfo
                {
                    Name = (string)reader["ColumnName"],
                    Description = (string)reader["ColumnDescription"],
                    DataType = (string)reader["DataType"],
                    PrimaryKey = (int)reader["PrimaryKey"],
                    ForeignKey = (int)reader["ForeignKey"],
                    Nullable = (bool)reader["Null"],
                    Default = (string)reader["Default"]
                });
            }

            return objects;
        }

        private string GetObjectDescriptionCommand(string type)
        {
            return " SELECT" +
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
                  $"   join ( select name, object_id from sys.{type} ) obj on obj.name = cols.table_name" +
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
                  " ORDER BY ObjectName, cols.ORDINAL_POSITION ASC";
        }

        public IEnumerable<VersionDescriptionInfo> GetVersionDescriptions()
        {
            using (var connection = _connectionFactory.Create(_settings.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * from BPADBVersion order by convert(int,dbversion)";
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader is null)
                            yield break;

                        while (reader.Read())
                        {
                            yield return new VersionDescriptionInfo
                            {
                                Version = (string)reader["dbversion"],
                                Description = (string)reader["description"]
                            };
                        }
                    }
                }
            }

        }

        public bool ValidVersionTableExists()
        {
            using (var connection = _connectionFactory.Create(_settings.ConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.Parameters.Add(CreateParameter(command, "@dbname", _settings.DatabaseName));
                    command.CommandText = "declare @sql nvarchar(max); " +
                        "set @sql = N'select 1 from ' + quotename(@dbname) + N'.INFORMATION_SCHEMA.COLUMNS C " +
                        "WHERE C.TABLE_NAME = ''BPADBVersion'' " +
                        "AND C.COLUMN_NAME = ''dbversion'' '; " +
                        "exec(@sql);";

                    return command.ExecuteScalar() != null;
                }
            }
        }
    }
}
