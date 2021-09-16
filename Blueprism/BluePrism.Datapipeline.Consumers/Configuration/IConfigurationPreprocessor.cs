namespace BluePrism.Datapipeline.Logstash.Configuration
{
    /// <summary>
    /// Runs a set of task over a logstash configuration to prepare it for use by the logstash process.
    /// </summary>
    public interface IConfigurationPreprocessor
    {
        /// <summary>
        /// Runs a set of task over a logstash configuration to prepare it for use by the logstash process.
        /// </summary>
        /// <param name="configuration">The logstash configuration</param>
        /// <returns>The processed logstash configuration</returns>
        string ProcessConfiguration(string configuration);
    }
}
