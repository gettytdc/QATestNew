using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BluePrism.Datapipeline.Logstash.Configuration.PreprocessorTasks
{

    /// <summary>
    /// Base class for a Preprocessor tasks which modifies a logstash configuration and returns the result.
    /// </summary>
    public abstract class ConfigurationPreprocessorTask
    {

        protected ILogstashSecretStore _logstashSecretStore;


        public ConfigurationPreprocessorTask(ILogstashSecretStore logstashSecretStore)
        {
            _logstashSecretStore = logstashSecretStore;
        }

        /// <summary>
        /// Modify the logstash configuration returning the modified configuration.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public abstract string ProcessConfiguration(string configuration);

        protected bool DoesStringContainASecretStoreKey(string stringToCheck)
        {
            return Regex.IsMatch(stringToCheck, @"\${KEY[0-9]+}");
        }


        protected List<string> GetSecretStoreKeysInString(string stringToCheck)
        {
            var matches = Regex.Matches(stringToCheck, @"(?<key>\${KEY[0-9]+})");
            return matches.Cast<Match>().Select(x => x.Groups["key"].Value).ToList();
        }

        protected List<string> ExtractParametersFromString(string text, string openParameterToken, string closeParameterToken)
        {
            var matches = Regex.Matches(text, $@"{openParameterToken}(?<parameter>(?!\s)[\w-,. ]*(?<!\s))\{closeParameterToken}");

            var results = matches.Cast<Match>().Select(x => x.Groups["parameter"].Value);

            return results.ToList();
        }

        /// <summary>
        /// Allows the extraction of values contained within the supplied openParameterToken and closeParameterToken
        /// </summary>
        /// <param name="text">The text to be read</param>
        /// <param name="openParameterToken">The opening token</param>
        /// <param name="closeParameterToken">The closing token</param>
        /// <returns>The IEnumerable of values between specified opening and ending tokens</returns>
        protected IEnumerable<string> ExtractValuesBetweenTokens(string text, string openParameterToken,
            string closeParameterToken)
        {
            Regex r = new Regex(Regex.Escape(openParameterToken) + "(.*?)" + Regex.Escape(closeParameterToken));
            MatchCollection matches = r.Matches(text);
            foreach (Match match in matches)
            {
                yield return match.Groups[1].Value;
            }
        }
    }
}
