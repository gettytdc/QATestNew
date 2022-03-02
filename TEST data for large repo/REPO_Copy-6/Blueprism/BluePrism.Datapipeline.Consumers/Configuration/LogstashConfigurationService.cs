using BluePrism.AutomateAppCore;
using BluePrism.Common.Security;
using BluePrism.BPCoreLib;
using BluePrism.Datapipeline.Logstash.Wrappers;
using System;
using System.IO;
using System.Security.Cryptography;
using NLog;

namespace BluePrism.Datapipeline.Logstash.Configuration
{
    public class LogstashConfigurationService : ILogstashConfigurationService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IConfigurationPreprocessorFactory _configurationPreprocessorFactory;
        private IServerDataPipeline _appServer;
        private IFileSystemService _fileSystemService;
        private readonly IEventLogger _eventLogger;
        private Func<ILogstashSecretStore> _logstashStoreFactory;
        private string _logstashDirectory;

        public string ConfigFilePath => ConfigFolder + "bpconf.conf";
        public string KeyStoreFilePath => ConfigFolder + "logstash.keystore";
        public string ConfigFolder => _logstashDirectory + "\\config\\";

        public LogstashConfigurationService(IServerDataPipeline appServer, IConfigurationPreprocessorFactory configurationPreprocessorFactory, IFileSystemService fileSystemService, IEventLogger eventLogger, Func<ILogstashSecretStore> logstashStoreFactory, string logstashDirectory)
        {
            _appServer = appServer;
            _configurationPreprocessorFactory = configurationPreprocessorFactory;
            _fileSystemService = fileSystemService;
            _logstashDirectory = logstashDirectory;
            _logstashStoreFactory = logstashStoreFactory;
            _eventLogger = eventLogger;
        }

        public (string config, SafeString secretStorePassword) GetConfiguration(int datagatewayProcessId)
        {
            // Get the config from the app server.
            var config = _appServer.GetConfigForDataPipelineProcess(datagatewayProcessId);

            // This randomly generated password is used to secure the logstash keystore when we build it.
            // It also needs to be provided to logstash when starting it, so it can open the keystore.
            var secretStorePassword = GeneratePasswordForSecretStore();

            // Process the config file before it is ready to be passed to logstash.
            // This performs tasks such as removing invalid characters, replacing credential parameters with actual credentials, and building
            // and adding sensitive strings to the logstash key store.

            var logstashStore = _logstashStoreFactory();
            var configurationPreprocessor = _configurationPreprocessorFactory.CreateConfigurationPreprocessor(logstashStore, ConfigFolder);
            var processedConfig = configurationPreprocessor.ProcessConfiguration(config);
            logstashStore.SaveStore(ConfigFolder, secretStorePassword);

            // return the config file, and the password to access the logstash secret store.
            return (processedConfig, secretStorePassword);
        }

        private SafeString GeneratePasswordForSecretStore()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bit_count = (32 * 6);
                var byte_count = ((bit_count + 7) / 8); // rounded up
                var bytes = new byte[byte_count];
                rng.GetBytes(bytes);
                
                // Convert the binary input into Base64 UUEncoded output.
                // Each 3 byte sequence in the source data becomes a 4 byte
                // sequence in the character array. 
                long arrayLength = (long)((4.0d / 3.0d) * bytes.Length);

                // If array length is not divisible by 4, go up to the next
                // multiple of 4.
                if (arrayLength % 4 != 0)
                {
                    arrayLength += 4 - arrayLength % 4;
                }

                char[] base64CharArray = new char[arrayLength];
                Convert.ToBase64CharArray(bytes, 0, bytes.Length, base64CharArray, 0);

                Array.Clear(bytes, 0, bytes.Length);

                var ss = new SafeString();
                for (int i = 0; i < base64CharArray.Length; i++)
                {
                    ss.AppendChar(base64CharArray[i]);
                    base64CharArray[i] = '\0';
                }

                return ss;
            }
        }

        /// <summary>
        /// Copies the contents of the config directory from the logstash install location to a directory the user will have
        /// write access to, as we will need to write the config file and keystore file to that directory.
        /// </summary>
        /// <param name="targetConfigDir"></param>
        private void PrepareTargetConfigurationDirectory(string targetConfigDir)
        {
            if(!_fileSystemService.DirectoryExists(targetConfigDir))
            {
                _fileSystemService.CreateDirectory(targetConfigDir);
            }

            string logstashConfigDir = Path.Combine(_logstashDirectory, "config");
            foreach(var file in Directory.GetFiles(logstashConfigDir))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(targetConfigDir, fileName), true);
            }
        }

        public void SaveConfigToFile(string config)
        {
            if (!_fileSystemService.DirectoryExists(ConfigFolder))
            {
                _fileSystemService.CreateDirectory(ConfigFolder);
            }

            _fileSystemService.WriteToFile(ConfigFilePath, config);
        }

        public void DeleteConfigurationFileOnDisk()
        {
            try
            {
                File.Delete(ConfigFilePath);
                File.Delete(KeyStoreFilePath);
            }
            catch (Exception e)
            {
                _eventLogger.Error(string.Format(Properties.Resources.UnableToDeleteLogstashConfigFile, e.Message), Log);
            }
        }
    }
}
