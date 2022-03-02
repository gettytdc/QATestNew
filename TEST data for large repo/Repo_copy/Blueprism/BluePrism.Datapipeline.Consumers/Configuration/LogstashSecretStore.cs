using BluePrism.Common.Security;
using BluePrism.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace BluePrism.Datapipeline.Logstash.Configuration
{

    /// <summary>
    /// Stores key-values pairs in a keystore readable by Logstash, where the value is a secret string, and the key is a token inserted into the 
    /// configuration file which is replaced with the secret string at runtime by Logstash.
    /// </summary>
    public class LogstashSecretStore : ILogstashSecretStore
    {
        IProcessFactory _processFactory;
        // the batch file which launches the application to create the keystore
        string _logstashStoreBatchFilePath;

        // the logstash install directory.
        string _logstashDirectory;

        // used to obfuscate the values passed to the keystore builder application.
        char[] obfuscationKey = { 'b', 'l', 'u', 'e', 'p', 'r', 'i', 's', 'm' };

        // stores key - values pairs to add to the keystore. 
        // the key should be added to the config file in place of the secret value. Logstash will lookup the secret value from the keystore
        // at when it reads the config file into memory.
        private Dictionary<string, SafeString> _secrets = new Dictionary<string, SafeString>();

        //used to generate unique key names
        private int _nextKeyIndex = 1;

        public LogstashSecretStore(IProcessFactory processFactory, string logstashDirectory)
        {
            _processFactory = processFactory;
            _logstashDirectory = logstashDirectory;
            _logstashStoreBatchFilePath = Path.Combine(logstashDirectory, @"..\BluePrism\blueprism-logstash-keystore-builder.bat");
        }

        /// <summary>
        /// Adds a secret string to the keystore. The key returned from the method 
        /// should be inserted into the config file in place of the secret string.
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public string AddSecret(SafeString secret)
        {
            if(secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }

            string key = $"${{KEY{_nextKeyIndex}}}";
            _secrets.Add(key, secret);
            _nextKeyIndex++;
            return key;
        }



        public SafeString GetSecret(string id)
        {
            if (_secrets.ContainsKey(id))
            {
                return _secrets[id];
            }

            return null;
        }

        /// <summary>
        /// This executes a java commandline application created by Blue Prism, which creates a keystore file in the format expected by Logstash.
        /// </summary>
        /// <param name="secrets"></param>
        public void SaveStore(string targetDirectory, SafeString password)
        {
            // if a keystore with this name already exists in the target directory then delete it.
            string targetPath = Path.Combine(targetDirectory, "logstash.keystore");
            if(File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            // the logstash keystore path will be passed as the first argument to the keystore builder application.
            var arguments = $"\"{targetPath}\"";

            // the secret strings to add to the keystore.
            // the will be added to an environment variable named LOGSTASH_KEYSTORE_ITEMSTOADD, available to the process we are about to start.
            // The keystore builder app will read these values from that environment variable.
            // The environment variable value is in the format KEY:SECRETSTRING , where both the key and secret will be obfuscated and base64 encoded separately.
            // multiple key/secret pairs will be separated by the '_' character.
            string itemsToAdd = "";
            bool added = false;
            foreach (var secret in _secrets)
            {
                itemsToAdd += $"{EncodeKeyAndSecret(GetKeyNameForKeystore(secret.Key), secret.Value)}_";
                added = true;
            }
            
            if (added) itemsToAdd = itemsToAdd.Remove(itemsToAdd.LastIndexOf('_'), 1);


            var process = _processFactory.CreateProcess();
            process.StartInfo.FileName = _logstashStoreBatchFilePath;
            process.StartInfo.WorkingDirectory = _logstashDirectory;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;

            // The password used to secure the keystore file will be added to the LOGSTASH_KEYSTORE_PASS environment variable.
            process.StartInfo.EnvironmentVariables.Add("LOGSTASH_KEYSTORE_PASS", password.SecureString.MakeInsecure());
            process.StartInfo.EnvironmentVariables.Add("LOGSTASH_KEYSTORE_ITEMSTOADD", itemsToAdd);
            process.Start();

            process.WaitForExit();

            int exitCode = process.ExitCode;

            if(exitCode != 0)
            {
                throw new InvalidOperationException("An error occurred when creating the keystore: " + process.StandardError.ReadToEnd());
            }
        }

        /// <summary>
        /// Obfuscates and base64 encodes the key and secret string for passing to the key store commandline application.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        string EncodeKeyAndSecret(string key, SafeString secret)
        {
            return $"{Obfuscate(key)}:{Obfuscate(secret)}";
        }


        /// <summary>
        /// Obfuscate and then base64 encode the string so there aren't any invalid characters to mess up parsing on the receiving end.
        /// The values will be base64 decoded and deobfuscated in the keystore builder application before being added to the logstash key store.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string Obfuscate(string str)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(str);
            for(int i = 0; i < plainTextBytes.Length; i++)
            {
                plainTextBytes[i] = (byte)(plainTextBytes[i] ^ obfuscationKey[i % 8]);
            }

            return Convert.ToBase64String(plainTextBytes);
        }

        private string Obfuscate(SafeString secret)
        {
            using (var pinned = secret.Pin())
            { 
                var obfuscatedPlainText = new byte[pinned.Chars.Length];

                for (int i = 0; i < pinned.Chars.Length; i++)
                {
                    obfuscatedPlainText[i] = (byte)(pinned.Chars[i] ^ obfuscationKey[i % 8]);
                }

                return Convert.ToBase64String(obfuscatedPlainText);
            }            
        }


        // the key as it appears in the config is in the format ${KEY1}, but when we add the key to the
        // keystore the ${} needs to be removed. We just need to insert KEY1 in the store. 
        private string GetKeyNameForKeystore(string key)
        {
            return key.Substring(2, key.Length - 3);
        }
    }
}
