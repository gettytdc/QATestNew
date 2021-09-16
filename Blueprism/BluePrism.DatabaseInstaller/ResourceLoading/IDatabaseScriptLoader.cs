namespace BluePrism.DatabaseInstaller
{
    public interface IDatabaseScriptLoader
    {
        int GetLatestUpgradeVersion();
        DatabaseInstallerScript GetCreateScript();
        DatabaseInstallerScript GetUpgradeScript(int scriptNumber);
        DatabaseInstallerScript GetDescribeDbScript();
        string GetResetInstallMarkerSql();
    }
}
