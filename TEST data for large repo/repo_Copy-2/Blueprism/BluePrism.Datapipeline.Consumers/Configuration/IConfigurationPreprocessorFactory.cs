namespace BluePrism.Datapipeline.Logstash.Configuration
{
    public interface IConfigurationPreprocessorFactory
    {
        IConfigurationPreprocessor CreateConfigurationPreprocessor(ILogstashSecretStore logstashStore, string targetConfigurationDirectory);
    }
}
