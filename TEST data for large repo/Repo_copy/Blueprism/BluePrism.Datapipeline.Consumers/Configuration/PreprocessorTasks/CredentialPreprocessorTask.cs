using System;
using System.Linq;

namespace BluePrism.Datapipeline.Logstash.Configuration.PreprocessorTasks
{
    /// <summary>
    /// Replaces credential parameters in the logstash config with the values from credentials
    /// </summary>
    public class CredentialPreprocessorTask : ConfigurationPreprocessorTask
    {
        private ICredentialService _credentialService;

        public CredentialPreprocessorTask(ICredentialService credentialService, ILogstashSecretStore logstashStore)
            : base(logstashStore)
        {
            _credentialService = credentialService;
        }

        public override string ProcessConfiguration(string configuration)
        {
            return InsertCredentials(configuration);
        }

        private string InsertCredentials(string config)
        {
            string result = config;

            var credentialParameters = ExtractParametersFromString(config, "<%", "%>");
            if (credentialParameters.Any())
            {
                var credentials = _credentialService.GetAllCredentialsInfo();

                foreach (var credentialParameter in credentialParameters)
                {
                    var credentialParameterParts = credentialParameter.Split('.');
                    string credentialName = credentialParameterParts[0];
                    string credentialProperty = credentialParameterParts[1];

                    if (!credentials.Exists(x => x == credentialName))
                    {
                        throw new InvalidOperationException(string.Format(Properties.Resources.CredentialNotFound, credentialName));
                    }

                    var fullCredential = _credentialService.GetCredential(credentialName);

                    string replacement = null;

                    if (credentialProperty.ToLower() == "username")
                    {
                        replacement = fullCredential.Username;
                    }
                    else if (credentialProperty.ToLower() == "password")
                    {
                        replacement = _logstashSecretStore.AddSecret(fullCredential.Password);
                    }
                    else
                    {
                        if (fullCredential.Properties.ContainsKey(credentialProperty))
                        {
                            replacement = _logstashSecretStore.AddSecret(fullCredential.Properties[credentialProperty]);
                        }
                    }

                    if (replacement == null)
                    {
                        throw new InvalidOperationException(string.Format(Properties.Resources.UnableToFindCredentialProperty, credentialParameter));
                    }

                    result = result.Replace($"<%{credentialParameter}%>", replacement);

                }

            }

            return result;
        }
    }
}
