using BluePrism.DatabaseInstaller.Data;

namespace BluePrism.DatabaseInstaller
{
    public interface IDatabaseScriptGenerator
    {
        string GenerateInstallationScript(int fromVersion, int toVersion, bool minify);
        string GenerateInstallationScript(DatabaseAction action, params int[] excludedVersions);
    }
}
