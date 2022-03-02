using BluePrism.Core.Conversion;
using BluePrism.Common.Security;

namespace BluePrism.Datapipeline.Logstash.Configuration.PreprocessorTasks
{
    public class Base64PreprocessorTask : ConfigurationPreprocessorTask
    {

        public Base64PreprocessorTask(ILogstashSecretStore secretStore)
            : base(secretStore)
        {

        }

        public override string ProcessConfiguration(string configuration)
        {
            const string openToken = "<base64>";
            const string closeToken = "</base64>";

            // get all strings inbetween <base64></base64> tags from the config.
            var valuesToEncode = ExtractValuesBetweenTokens(configuration, openToken, closeToken);


            foreach (var valueToEncode in valuesToEncode)
            {
                bool isSecret = DoesStringContainASecretStoreKey(valueToEncode);
                
                if (isSecret)
                {

                    // if the string to base64 encode contains one of more strings in the logstash keystore, then it gets a bit tricky.
                    // we need to base64 get the actual sensitive string in the keystore, and not base64 encode the id currently in the config which references the
                    // string in the keystore.
                    // And also, since base64 encoding is reversable, we must treat the resulting base64 string as sensitive data and also 
                    // store this in the logstash keystore


                    string value = valueToEncode;

                    SafeString ss = new SafeString(valueToEncode);

                    // there may be more than one sensitive string in the text we need to base64 encode. we will need to retrieve all of them
                    // before base64 encoding.
                    var secretStoreKeys = GetSecretStoreKeysInString(valueToEncode);
                    foreach (var secretStoreKey in secretStoreKeys)
                    {

                        var secret = _logstashSecretStore.GetSecret(secretStoreKey);
                        int index = valueToEncode.IndexOf(secretStoreKey);

                        for (int i = 0; i < secretStoreKey.Length; i++)
                        {
                            ss.RemoveAt(index);
                        }

                        // the secret store key in the plain text string is removed too, so that the plain text and SafeString
                        // keeps in sync. This is useful because if we are replacing multiple secret store keys in valueToEncode,
                        // we can then locate the next secret store keys in the safe string by finding the index of the keys in the plain text string.
                        value = value.Remove(index, secretStoreKey.Length);

                        using (var pinned = secret.Pin())
                        {
                            for (int i = 0; i < pinned.Chars.Length; i++)
                            {
                                ss.InsertAt(index + i, pinned.Chars[i]);
                            }

                            // insert 'x''s into the plain text string where we insert the actual sensitive text into the safestring.
                            // keeping the plain text and safestring strings aligned for the reason mentioned above.
                            value = value.Insert(index, new string('x', pinned.Chars.Length));
                        }
                    }

                    var base64Encoded = SafeStringBase64Converter.ToBase64SafeString(ss);
                    var id = _logstashSecretStore.AddSecret(base64Encoded);
                    configuration = configuration.Replace($"{openToken}{valueToEncode}{closeToken}", id);
                }
                else
                {
                    // if the string to base64 encode doesn't contain any values stored in the logstash secret store, this is a lot simpler.
                    string val = valueToEncode;
                    var encoded = Base64Encode(val);
                    configuration = configuration.Replace($"{openToken}{valueToEncode}{closeToken}", encoded);
                }
            }

            return configuration;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


    }
}
