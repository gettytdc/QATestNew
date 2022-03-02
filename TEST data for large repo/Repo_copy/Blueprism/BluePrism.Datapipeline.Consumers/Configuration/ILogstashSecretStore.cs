using BluePrism.Common.Security;

namespace BluePrism.Datapipeline.Logstash.Configuration
{
    /// <summary>
    /// Adds sensitive strings to the logstash secrets keystore so they are not visible in the configuration file.
    /// </summary>
    public interface ILogstashSecretStore
    {
        /// <summary>
        /// Adds a secret to the logstash secrets store and returns the id to use to access it.
        /// The id should be used in the config file to reference the sensitive string.
        /// Logstash will replace the id with the sensitive string at runtime.
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        string AddSecret(SafeString secret);

        /// <summary>
        /// Access a secret from the store using the id.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        SafeString GetSecret(string id);

        /// <summary>
        /// Save the logstash secret store to disk so it can be used by Logstash.
        /// </summary>
        void SaveStore(string targetDirectory, SafeString password);

    }
}
