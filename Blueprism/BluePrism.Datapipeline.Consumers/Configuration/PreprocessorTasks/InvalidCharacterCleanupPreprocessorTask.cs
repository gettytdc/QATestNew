namespace BluePrism.Datapipeline.Logstash.Configuration.PreprocessorTasks
{
    /// <summary>
    /// Removes control characters from the configuration.
    /// </summary>
    public class InvalidCharacterCleanupPreprocessorTask : ConfigurationPreprocessorTask
    {
        public InvalidCharacterCleanupPreprocessorTask(ILogstashSecretStore logstashStore)
            : base(logstashStore)
        {

        }

        public override string ProcessConfiguration(string configuration)
        {
            return System.Text.RegularExpressions.Regex.Replace(configuration, @"[\p{C}-[\r\n\t]]+", string.Empty);
        }
    }
}
