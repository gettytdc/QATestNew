using BluePrism.Common.Security;

namespace BluePrism.DatabaseInstaller
{
    public interface IInstaller
    {
        bool IsCancelled { get; set; }
        bool IsCancelling { get; set; }

        event PercentageProgressEventHandler ReportProgress;
        event CancelProgressEventHandler CancelProgress;

        int GetNumberOfUsers();
        bool IsUpgradeAvailable(int current, int required);
        void UpgradeDatabase(int maxVersion);
        bool SessionLogMigrationRequired(int current, int required);
        long GetSessionLogSizeKB();
        long GetSessionLogRowCount();
        void ConfigurableDatabaseUpgrade(DatabaseInstallerOptions options);
        int GetRequiredDBVersion();
        int GetCurrentDBVersion();
        void CheckIntegrity();
        void AnnotateDatabase();
        string GetDBDocs();
        string CreateProgressLabel(object data);
        bool CheckDatabaseExists();
        string GenerateInstallerScript(int fromVersion, int toVersion, bool minify);
        string GenerateUpgradeScript();
        string GenerateCreateScript();
        void SetDefaultPasswordRules();
        void CreateDatabase(DatabaseActiveDirectorySettings adSettings, bool dropExisting, bool configureOnly, int maxVer);
        void CreateLocalDatabase(string username, SafeString password);
    }
}
