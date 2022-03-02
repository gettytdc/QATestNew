using System.Linq;
using System.Text.RegularExpressions;

namespace BluePrism.DatabaseInstaller
{
    public class DatabaseScriptLoader : IDatabaseScriptLoader
    {
        private const string ResourceNamespace = "BluePrism.DatabaseInstaller.Scripts";
        private const string RegexUpgradePattern = @"^.*db_upgradeR([0-9]+)\.sql$";
        private const string CreateScriptName = "db_createR10.sql";
        private const string DescribeDbScriptName = "DescribeDB.sql";

        private readonly IEmbeddedResourceLoader _resourceLoader;
        
        public DatabaseScriptLoader(IEmbeddedResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public DatabaseInstallerScript GetCreateScript() => DatabaseInstallerScript.Parse(CreateScriptName, GetSql(CreateScriptName));

        public DatabaseInstallerScript GetUpgradeScript(int scriptNumber)
        {
            var upgradeScriptName = $"db_upgradeR{scriptNumber}.sql";
            var d = DatabaseInstallerScript.Parse(upgradeScriptName, GetSql(upgradeScriptName));
            return d;
        }

        public DatabaseInstallerScript GetDescribeDbScript() => DatabaseInstallerScript.Parse(DescribeDbScriptName, GetSql(DescribeDbScriptName));

        public int GetLatestUpgradeVersion()
        {
            var regex = new Regex(RegexUpgradePattern);

            return _resourceLoader.GetResourceNames().Select(n => regex.Match(n))
                    .Where(m => m.Success)
                    .Max(m => int.Parse(m.Groups[1].Value));
        }

        public string GetResetInstallMarkerSql()
            => "if (select InstallInProgress from BPVScriptEnvironment) =  1 "
            + "alter table BPASysConfig drop column InstallInProgress;";

        private string GetSql(string fileName)
            => _resourceLoader.GetResourceContent($"{ResourceNamespace}.{fileName}") ?? string.Empty;

     
    }
}
