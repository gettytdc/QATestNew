using BluePrism.Common.Security;


namespace BluePrism.Datapipeline.Logstash
{
    /// <summary>
    /// A service for retrieving logstash configuration files.
    /// </summary>
    public interface ILogstashConfigurationService
    {
        /// <summary>
        /// Get the configuration for the specified data gateway process.
        /// </summary>
        /// <param name="datagatewayProcessId">The id of the datag gateway process to get the configuration for.</param>
        /// <returns>The path to the configuration file, and the password to access the logstash secret store</returns>
        (string config, SafeString secretStorePassword) GetConfiguration(int datagatewayProcessId);

        /// <summary>
        /// Deletes the configuration file from disk
        /// </summary>
        void DeleteConfigurationFileOnDisk();

        /// <summary>
        /// Saves the configuration file to disk
        /// </summary>
        /// <param name="config">The configuration file to save</param>
        void SaveConfigToFile(string config);

        /// <summary>
        /// The configuration file path
        /// </summary>
        string ConfigFilePath { get; }

        /// <summary>
        /// The configuration file path
        /// </summary>
        string ConfigFolder { get; }

    }
}
