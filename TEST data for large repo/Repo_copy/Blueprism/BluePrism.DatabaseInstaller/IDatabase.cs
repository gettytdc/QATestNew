using BluePrism.Common.Security;
using System.Collections.Generic;

namespace BluePrism.DatabaseInstaller
{
    public interface IDatabase
    {
        event PercentageProgressEventHandler ReportProgress;
        event CancelProgressEventHandler CancelProgress;
        bool IsCancelling { get; set; }
        bool IsCancelled { get; set; }

        void AnnotateDatabase();
        void DeleteDatabase();
        void ExecuteCreateScript();
        void InitialiseDatabase();
        void CreateAdminUser();
        void ConfigureAdminUser();
        void ConfigureAdminUser(string username, SafeString password);
        void UpgradeDatabase(int version);
        void ConfigureForActiveDirectory(DatabaseActiveDirectorySettings activeDirectorySettings, string eventCode);
        void SetDefaultPasswordRules();
        void EnableValidationCheckForExceptionTypes();
        void AllowSnapshotIsolation();
        void ConfigureDatabaseInstallerOptions(DatabaseInstallerOptions options);
        bool CheckDatabaseExists(string databaseName);
        bool CheckCanOpenDatabase();
        int GetCurrentVersion();
        long GetSessionLogRowCount();
        long GetSessionLogSizeKb();
        int GetNumberOfUsers();
        bool GetIsActiveDirectory();
        IEnumerable<int> GetInstalledVersions();
        IEnumerable<ObjectDescriptionInfo> GetViewDescriptionInfo();
        IEnumerable<ObjectDescriptionInfo> GetTableDescriptionInfo();
        IEnumerable<VersionDescriptionInfo> GetVersionDescriptions();
        bool ValidVersionTableExists();
    }
}