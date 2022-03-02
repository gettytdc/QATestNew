using BluePrism.AutomateAppCore;
using BluePrism.Datapipeline.Logstash.Configuration.PreprocessorTasks;
using System.Collections.Generic;

namespace BluePrism.Datapipeline.Logstash.Configuration
{
    public class ConfigurationPreprocessorFactory : IConfigurationPreprocessorFactory
    {
        IServer _appServer;
        IProcessFactory _processFactory;
        string _logstashDirectory;
        public ConfigurationPreprocessorFactory(IServer appServer, IProcessFactory processFactory, string logstashDirectory)
        {
            _appServer = appServer;
            _processFactory = processFactory;
            _logstashDirectory = logstashDirectory;
        }

        public IConfigurationPreprocessor CreateConfigurationPreprocessor(ILogstashSecretStore logstashStore, string targetConfigurationDirectory)
        {
            return new ConfigurationPreprocessor(new List<ConfigurationPreprocessorTask>()
            {
                new InvalidCharacterCleanupPreprocessorTask(logstashStore),
                new CredentialPreprocessorTask(new CredentialService(_appServer), logstashStore),
                new Base64PreprocessorTask(logstashStore)
            });
        }
    }
}
