using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BluePrism.DatabaseInstaller
{
    public class DatabaseInstallerScript
    {
        public readonly string Name;
        public readonly IList<string> SqlStatements;

        private DatabaseInstallerScript(string name, IList<string> sqlStatements)
        {
            Name = name;
            SqlStatements = sqlStatements;
        }

        public static DatabaseInstallerScript Parse(string name, string script)
        {
            if (string.IsNullOrEmpty(script))
                return null;

            const string separator = "(\r\n|\n)GO";

            var parts = Regex.Split(script, separator, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return new DatabaseInstallerScript(name, parts);
        }
    }
}

