using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BluePrism.DatabaseInstaller.Data;
using BluePrism.Common.Security;

namespace BluePrism.DatabaseInstaller
{
    public class Installer : IInstaller
    {
        private const int SessionLogMigrationVersion = 295;

        private readonly IDatabaseScriptGenerator _scriptWrapper;
        private readonly ISqlConnectionWrapper _sqlConnectionWrapper;
        private readonly IDatabase _database;
        private readonly string _applicationName;
        private readonly string _ssoAuditEventCode;
        private readonly ISqlDatabaseConnectionSetting _settings;
        private readonly IDatabaseScriptLoader _scriptLoader;

        public bool IsCancelled
        {
            get => _database.IsCancelled;
            set 
            {
                _database.IsCancelled = value;
            }
        }
        public bool IsCancelling
        {
            get => _database.IsCancelling;
            set
            {
                _database.IsCancelling = value;
            }
        }

        public event PercentageProgressEventHandler ReportProgress;
        public event CancelProgressEventHandler CancelProgress;

        public Installer(
            ISqlDatabaseConnectionSetting settings,
            TimeSpan commandTimeout,
            string applicationName,
            string ssoAuditEventCode,
            Func<ISqlDatabaseConnectionSetting, TimeSpan, IDatabase> databaseFactory,
            IDatabaseScriptGenerator scriptWrapper,
            ISqlConnectionWrapper sqlConnectionWrapper,
            IDatabaseScriptLoader scriptLoader)
        {
            _settings = settings;
            _applicationName = applicationName;
            _ssoAuditEventCode = ssoAuditEventCode;
            _database = databaseFactory(_settings, commandTimeout);
            _scriptWrapper = scriptWrapper;
            _sqlConnectionWrapper = sqlConnectionWrapper;
            _scriptLoader = scriptLoader;

            _database.ReportProgress += (_, e) => ReportProgress?.Invoke(this, e);
            _database.CancelProgress += (_, e) => CancelProgress?.Invoke(this, e);
        }

        public void AnnotateDatabase()
            => _database.AnnotateDatabase();

        public bool CheckDatabaseExists()
        {
            try
            {
                return _database.CheckDatabaseExists(_settings.DatabaseName);
            }
            catch(ArgumentException)
            {
                throw new DatabaseInstallerException(Resources.ErrorMsg_InvalidConnectionString);
            }
            catch(Exception)
            {
                try
                {
                    return _database.CheckCanOpenDatabase();
                }
                catch(Exception ex)
                {
                    throw new DatabaseInstallerException(string.Format(Resources.DatabaseInstaller_UnableToDetermineWhetherDatabaseExists0, ex.Message));
                }
            }
        }

        public void CheckIntegrity()
        {
            if (!CheckDatabaseExists())
                throw new DatabaseInstallerException(string.Format(Resources.DatabaseInstaller_TheSpecifiedDatabase0DoesNotExist, _settings.DatabaseName));

            var currentDbVersion = GetCurrentDBVersion(out var dbVersionFailureException);
            var requiredDbVersion = GetRequiredDBVersion();

            if (currentDbVersion == 0)
            {
                throw new DatabaseInstallerException(Resources.DatabaseInstaller_DatabaseVersionCouldNotBeDetermined)
                {
                    AssociatedException = dbVersionFailureException
                };
            }

            if(currentDbVersion != requiredDbVersion)
            {
                var description = string.Format(Resources.DatabaseInstaller_DatabaseVersion0RequiredVersion1, currentDbVersion.ToString(), requiredDbVersion.ToString());

                var message = currentDbVersion < requiredDbVersion
                    ? Resources.DatabaseInstaller_YouAreRunning0AgainstAnOldVersionOfThe0DatabaseThisDatabaseMustBeUpgradedBefore
                    : Resources.DatabaseInstaller_TheChosenDatabaseHasBeenUpgradedByAVersionOf0WhichIsNewerThanTheCurrentVersionY;

                throw new DatabaseInstallerException(string.Format(message, _applicationName) + description, new Exception("DB_UPGRADE_NEEDED"));
            }

            if (!_database.GetIsActiveDirectory() && _database.GetNumberOfUsers() == 0)
            {
                throw new DatabaseInstallerException(Resources.DatabaseInstaller_TheDatabaseNeedsConfiguringBeforeItCanBeUsed);
            }
        }

        public void ConfigurableDatabaseUpgrade(DatabaseInstallerOptions options)
        {
            var targetVersion = _scriptLoader.GetLatestUpgradeVersion();
            var currentVersion = _database.GetCurrentVersion();

            if(!IsUpgradeAvailable(currentVersion, targetVersion))
            {
                var errorMessage = string.Format(Resources.DatabaseInstaller_NoUpgradeAvailableCurrentVersionReadAs0AndRequiredVersionReadAs1, currentVersion, targetVersion);
                throw new DatabaseInstallerException(errorMessage);
            }

            _database.ConfigureDatabaseInstallerOptions(options);
            _database.UpgradeDatabase(targetVersion);
        }

        public void CreateLocalDatabase(string username, SafeString password)
        {
            _sqlConnectionWrapper.ClearAllPools();
            try
            {
                _database.DeleteDatabase();
                _database.InitialiseDatabase();
            }
            catch
            {
                _database.InitialiseDatabase();
            }

            _database.ExecuteCreateScript();
            UpgradeDatabase(0);
            _database.ConfigureAdminUser(username, password);

            // For brand new databases we want to enable the validation check for
            // exception types (which is added as disabled by the upgrade script to
            // avoid breaking existing processes)
            _database.EnableValidationCheckForExceptionTypes();

            _database.AllowSnapshotIsolation();
        }

        public void CreateDatabase(DatabaseActiveDirectorySettings adSettings, bool dropExisting, bool configureOnly, int maxVer)
        {
            var databaseExists = CheckDatabaseExists();

            if (configureOnly)
            {
                if (!databaseExists)
                    throw new DatabaseInstallerException(Resources.DatabaseInstaller_DatabaseDoesNotExistCanTConfigure);

                if (adSettings == null)
                    _database.ConfigureAdminUser();
            }
            else
            {
                if (!databaseExists || dropExisting)
                    InitialiseDatabase(databaseExists && dropExisting);

                _database.ExecuteCreateScript();

                if (adSettings == null)
                    _database.CreateAdminUser();

                UpgradeDatabase(maxVer);
            }

            if (adSettings != null)
                _database.ConfigureForActiveDirectory(adSettings, _ssoAuditEventCode);

            // For brand new databases we want to enable the validation check for
            // exception types (which is added as disabled by the upgrade script to
            // avoid breaking existing processes)
            _database.EnableValidationCheckForExceptionTypes();

            _database.AllowSnapshotIsolation();
        }

        public void UpgradeDatabase(int maxVersion)
        {
            var targetVersion = _scriptLoader.GetLatestUpgradeVersion();
            var currentVersion = GetCurrentDBVersion();
            if (!IsUpgradeAvailable(currentVersion, targetVersion))
                throw new DatabaseInstallerException(string.Format(
                    Resources.DatabaseInstaller_NoUpgradeAvailableCurrentVersionReadAs0AndRequiredVersionReadAs1, currentVersion, targetVersion));

            if (maxVersion != 0 && targetVersion > maxVersion)
                targetVersion = maxVersion;

            if (currentVersion >= targetVersion)
                throw new DatabaseInstallerException(Resources.DatabaseInstaller_DatabaseIsUpToDate);

            _database.UpgradeDatabase(targetVersion);
        }

        private void InitialiseDatabase(bool deleteExisting)
        {
            _sqlConnectionWrapper.ClearAllPools();

            if (deleteExisting)
                _database.DeleteDatabase();

            _database.InitialiseDatabase();
        }

        public string CreateProgressLabel(object data) 
            => data as string;

        public string GenerateInstallerScript(int fromVersion, int toVersion, bool minify)
            =>  _scriptWrapper.GenerateInstallationScript(fromVersion, toVersion, minify);

        public string GenerateUpgradeScript()
            => _scriptWrapper.GenerateInstallationScript(DatabaseAction.Upgrade, _database.GetInstalledVersions().ToArray());

        public string GenerateCreateScript()
            => _scriptWrapper.GenerateInstallationScript(DatabaseAction.Create);

        public void SetDefaultPasswordRules()
            => _database.SetDefaultPasswordRules();

        public int GetCurrentDBVersion() => GetCurrentDBVersion(out _);

        public int GetCurrentDBVersion(out Exception failureException)
        {
            failureException = null;

            try
            {
                if (!_database.ValidVersionTableExists())
                    throw new DatabaseInstallerException(Resources.DatabaseInstaller_ThisDoesNotAppearToBeAValidBluePrismDatabase);

                return _database.GetCurrentVersion();
            }
            catch (DatabaseInstallerException)
            {
                throw;
            }
            catch (Exception ex)
            {
                failureException = ex;

                return 0;
            }
        }

        public string GetDBDocs()
        {
            var result = new StringBuilder();

            result.AppendLine(GetDBDocsPreAmble(_database.GetCurrentVersion()));

            result.AppendLine("=Tables=");
            result.Append(GenerateObjectDescription(_database.GetTableDescriptionInfo()));

            result.AppendLine("=Views=");
            result.Append(GenerateObjectDescription(_database.GetViewDescriptionInfo()));

            result.Append(GenerateVersionDescription());

            return result.ToString();
        }

        private string GenerateVersionDescription()
        {
            var result = new StringBuilder();
            result.AppendLine("=Revision History=");
            result.AppendLine("{| border=1");
            result.AppendLine("!Revision");
            result.AppendLine("!Description");

            foreach (var version in _database.GetVersionDescriptions().ToList())
            {
                result.AppendLine("|-");
                result.AppendLine("|" + version.Version);
                result.AppendLine("|" + version.Description);
            }

            result.AppendLine("|}");

            return result.ToString();
        }

        private string GenerateObjectDescription(IEnumerable<ObjectDescriptionInfo> objectDescriptions)
        {
            var result = new StringBuilder();

            foreach (var description in objectDescriptions)
            {
                result.AppendLine($"=={description.Name}==");
                result.AppendLine(description.Description);

                result.AppendLine("{| border = 1 width=\"100%\"");
                result.AppendLine("! scope=col width = \"20%\" | Field");
                result.AppendLine("! scope=col width = \"10%\" | Type");
                result.AppendLine("! scope=col width = \"55%\" | Description");
                result.AppendLine("! scope=col width = \"5%\" | Key");
                result.AppendLine("! scope=col width = \"5%\" | Null?");
                result.AppendLine("! scope=col width = \"5%\" | Default");

                foreach (var column in description.ColumnDescriptions)
                {
                    result.AppendLine("|-");
                    result.AppendLine($"|{column.Name}");
                    result.AppendLine($"|{column.DataType}");
                    result.AppendLine($"|{column.Description}");
                    result.AppendLine($"|{(column.PrimaryKey > 0 ? $"P{column.PrimaryKey}" : "") + (column.ForeignKey > 0 ? "F" : "")}");
                    result.AppendLine($"|{(column.Nullable ? "yes": "no")}");
                    result.AppendLine($"|{column.Default}");
                }

                result.AppendLine("|}");
            }

            return result.ToString();
        }

        private string GetDBDocsPreAmble(int version)
        {
            var result = new StringBuilder();
            result.AppendLine(string.Format(Resources.DatabaseInstaller_DatabaseDictionaryForDatabaseVersionR0, version.ToString()));
            result.AppendLine();
            result.Append(Resources.DatabaseInstaller_ThisDocumentationIsForInformationOnly);
            result.Append(Resources.DatabaseInstaller_BluePrismCannotSupportSolutionsThatAccessTheDatabaseOutsideOfTheSuppliedBluePri);
            result.AppendLine(Resources.DatabaseInstaller_ImplementingAnySolutionThatAccessesTheDatabaseCouldHaveUndesirableConsequencesI);
            result.AppendLine(Resources.DatabaseInstaller_PerformanceDegradationOfTheEntireSystemDueToInefficientQueries);
            result.AppendLine(Resources.DatabaseInstaller_CompleteFailureOfTheSolutionFollowingTableChangesInUpgrades);
            result.AppendLine(Resources.DatabaseInstaller_SecurityRisks);
            result.AppendLine(Resources.DatabaseInstaller_InaccessibilityOfEncryptedData);
            result.AppendLine();

            return result.ToString();
        }

        public int GetRequiredDBVersion()
            => _scriptLoader.GetLatestUpgradeVersion();

        public long GetSessionLogRowCount()
            => _database.GetSessionLogRowCount();

        public long GetSessionLogSizeKB()
            => _database.GetSessionLogSizeKb();

        public bool IsUpgradeAvailable(int currentVersion, int requiredVersion)
            => currentVersion >= 10 && requiredVersion > currentVersion;

        public bool SessionLogMigrationRequired(int current, int required)
            => current < SessionLogMigrationVersion && required >= SessionLogMigrationVersion;

        public int GetNumberOfUsers()
            => _database.GetNumberOfUsers();
    }
}
