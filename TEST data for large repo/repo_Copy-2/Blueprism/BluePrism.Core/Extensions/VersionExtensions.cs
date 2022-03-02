using System;
using System.Text.RegularExpressions;

namespace BluePrism.Core.Extensions
{
    public static class VersionExtensions
    {
        private static readonly TimeSpan RegexMatchTimeout = new TimeSpan(0, 0, 1);
        public static bool TryParseVersionString(string versionString, out Version version)
        {
            versionString = versionString ?? "";
            var versionRegex = new Regex(@"^(\d+\.)(\d+\.)(\d+\.)(\d+)", RegexOptions.None, RegexMatchTimeout);
            if (versionRegex.IsMatch(versionString))
            {
                version = new Version(versionRegex.Match(versionString).ToString());
                return true;
            }

            version = null;
            return false;
        }
   }
}
