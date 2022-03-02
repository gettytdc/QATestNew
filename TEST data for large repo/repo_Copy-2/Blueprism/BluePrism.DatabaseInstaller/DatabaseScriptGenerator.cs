using BluePrism.DatabaseInstaller.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluePrism.DatabaseInstaller
{
    public class DatabaseScriptGenerator : IDatabaseScriptGenerator
    {
        private const string inlineCommentStart = "/*";
        private const string inlineCommentEnd = "*/";
        private const string commentStart = "--";
        private readonly IDatabaseScriptLoader _scriptLoader;

        public DatabaseScriptGenerator(IDatabaseScriptLoader scriptLoader)
        {
            _scriptLoader = scriptLoader;
        }

        public string GenerateInstallationScript(int fromVersion, int toVersion, bool minify)
        {
            if(toVersion == 0)
                toVersion = _scriptLoader.GetLatestUpgradeVersion();

            var range = Enumerable.Range(fromVersion + 1, Math.Max(0, toVersion - fromVersion)).ToList();
            if (fromVersion == 0)
                range.Add(0);

            return GetDBScripts(range, minify);
        }

        public string GenerateInstallationScript(DatabaseAction action, params int[] excludedVersions)
        {
            var startingScriptNumber = GetStartingScript(action);
            var versions = Enumerable.Range(startingScriptNumber, _scriptLoader.GetLatestUpgradeVersion() + 1 - startingScriptNumber)
                            .Except(excludedVersions);

            return GetDBScripts(versions, true);
        }

        private int GetStartingScript(DatabaseAction action)
        {
            switch (action)
            {
                case (DatabaseAction.Create):
                        return 0;
                case (DatabaseAction.Upgrade):
                        return 11;
                default:
                    throw new ArgumentException(nameof(action));
            }
        }

        private string GetDBScripts(IEnumerable<int> versions, bool minify)
        {
            var orderedVersions = versions.Distinct().OrderBy(i => i);

            var result = new StringBuilder();
            foreach (var version in orderedVersions)
            {
                if (version == 0)
                    result.Append(GetCreateScriptContent(minify));

                if (version < 11) continue;

                result.Append(GetExecutableUpgradeBatchContent(version, minify));
            }

            result.AppendLine(_scriptLoader.GetResetInstallMarkerSql());
            return result.ToString();
        }

        private string GetCreateScriptContent(bool minify)
        {
            var result = new StringBuilder();

            var sqlStatements = _scriptLoader.GetCreateScript().SqlStatements;
            foreach (var sql in sqlStatements)
            {
                result.AppendLine(minify ? MinifyScript(sql) : sql);
                result.AppendLine("GO");
            }

            return result.ToString();
        }

        private string GetExecutableUpgradeBatchContent(int version, bool minify)
        {
            var script = _scriptLoader.GetUpgradeScript(version);
            if (script is null)
                return string.Empty;

            var result = new StringBuilder();
            result.Append($"if not exists (select 1 from BPADBVersion where dbversion='{version}') begin ");

            foreach (var sql in script.SqlStatements)
            {
                var content = (minify ? MinifyScript(sql) : sql).Replace("'", "''");
                result.Append($"exec('{content}')");
            }

            result.AppendLine(" end ");

            if (!minify)
                result.AppendLine("GO");

            return result.ToString();
        }

        private string MinifyScript(string script)
        {
            var result = new StringBuilder();
            var unfinishedComment = false;

            foreach (var line in GetTrimmedLines(script))
            {
                if (unfinishedComment)
                {
                    if (!line.Contains(inlineCommentEnd))
                        continue;

                    unfinishedComment = false;
                    var remainingText = GetTextAfterCommentEnd(line).Trim();
                    if (remainingText.Length > 0)
                        result.Append(remainingText + " ");
                }
                else
                {
                    var amendedLine = line;
                    if (amendedLine.Contains(inlineCommentStart))
                    {
                        var commentStartPosition = amendedLine.IndexOf(inlineCommentStart);
                        var remainingText = amendedLine.Substring(commentStartPosition + inlineCommentStart.Length);
                        amendedLine = amendedLine.Substring(0, commentStartPosition);

                        if (remainingText.Contains(inlineCommentEnd))
                            amendedLine += GetTextAfterCommentEnd(line);
                        else
                            unfinishedComment = true;
                    }

                    if (amendedLine.Contains(commentStart))
                        amendedLine = amendedLine.Substring(0, amendedLine.IndexOf(commentStart));

                    amendedLine = amendedLine.Trim();
                    if (amendedLine.Length > 0)
                        result.Append(amendedLine + " ");
                }
            }

            return result.ToString();
        }

        private string GetTextAfterCommentEnd(string line)
            => line.Substring(line.IndexOf(inlineCommentEnd) + inlineCommentEnd.Length);

        private List<string> GetTrimmedLines(string script)
            => script.Split('\n')
                .Select(s => s.Trim())
                .Where(l => !string.IsNullOrEmpty(l))
                .ToList();
    }
}
