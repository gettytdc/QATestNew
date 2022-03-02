using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using BluePrism.Common.Security;

namespace BluePrism.Skills
{
    [Serializable, DataContract(Namespace = "bp"), KnownType(typeof(WebSkillVersion))]
    public class Skill
    {
        public const string FileExtension = "bpskill";

        private const int KeySize = 128;
        private const int IvSize = 16;

        private const string SymmetricKey = "LWUXWVtoD1dvPEYEGDtHVyEXUiIQcm0L";
        private const string PublicKey = "fwAmKn9XGDVZBAcVdE9/VjYNVTsqDSddKnh/SBoLPU83ZjEvckNXAXMFOCJ9GVtUOg4SHjwKGQVxCGwIGhUXCBokGQF8SBYrcS8FQSUGeg0nTQoILX49Tz0ncRIHFg0vbD0FAVBqCAlKUDYXEEFYdSE6dS0XWmIHJCwIAgYgKw4ZNjYzVmBWSHsjBAVlEUh+GU9cOwhlPmUuNF40FBYaAA8VPTt/AwJSEy5BQzw7Q1wdPFYDEnkzdBklewgaG0ouFm9JRHldBRZUHQFOdjZKST0WXCAtDRFnCQ8EVScZCRUtNxsfCg5OMWspORUzJVNVJx0H";


        [DataMember]
        private Guid _id;

        [DataMember(Name = "p")]
        private String _provider;

        [DataMember(Name = "e")]
        private bool _enabled;

        [DataMember(Name = "v")]
        private ReadOnlyCollection<SkillVersion> _versions;

        public Guid Id => _id;
        public String Provider => _provider;
        public bool Enabled => _enabled;
        public ReadOnlyCollection<SkillVersion> Versions => _versions;
        public ReadOnlyCollection<SkillVersion> PreviousVersions => Versions.Where(x => !x.Equals(LatestVersion)).ToList().AsReadOnly();
        public SkillVersion LatestVersion => Versions.OrderByDescending(v => v.ImportedAt).FirstOrDefault();

        public string GetWebApiName() => ((WebSkillVersion)LatestVersion).WebApiName;

        public Skill(Guid id,
                    string provider,
                    bool enabled,
                    IEnumerable<SkillVersion> versions)
        {
            if (string.IsNullOrEmpty(provider))
                throw new ArgumentNullException(nameof(provider));

            _id = id;
            _provider = provider;
            _enabled = enabled;
            _versions = versions.ToList().AsReadOnly();
        }

        public static Stream DecryptAndVerify(Stream encryptedStream)
        {
            string encryptedStuff;
            encryptedStream.Position = 0;
            using (StreamReader reader = new StreamReader(encryptedStream, Encoding.UTF8))
            {
                encryptedStuff = reader.ReadToEnd();
            }

            var content = DecryptAndVerify(encryptedStuff);

            var unencryptedStream = new MemoryStream();
            using (var writer = new StreamWriter(unencryptedStream, Encoding.UTF8, 1024, true))
            {
                writer.Write(content);
                writer.Flush();
                unencryptedStream.Position = 0;
                return unencryptedStream;
            }
        }

        public static string DecryptAndVerify(string encryptedString)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedString);
            var unencryptedBytes = DecryptSymmetrically(encryptedBytes, Convert.FromBase64String(GetKey(SymmetricKey)));
            var signature = unencryptedBytes.Skip(unencryptedBytes.Length - KeySize).ToArray();
            var content = unencryptedBytes.Take(unencryptedBytes.Length - KeySize).ToArray();

            Verify(signature, content, GetKey(PublicKey));
            return Encoding.UTF8.GetString(content);
        }

        private static void Verify(byte[] signature, byte[] content, string publicKey)
        {
            using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
            using (var shaCryptoServiceProvider = new SHA256CryptoServiceProvider())
            {
                rsaCryptoServiceProvider.FromXmlString(publicKey);
                if (!rsaCryptoServiceProvider.VerifyData(content, shaCryptoServiceProvider, signature))
                    throw new CryptographicException(SkillResources.UnableToVerifySignature);
            }
        }

        private static byte[] DecryptSymmetrically(byte[] stuff, byte[] key)
        {
            var ivStringBytes = stuff.Take(IvSize).ToArray();
            var cipherTextBytes = stuff.Skip(IvSize).ToArray();

            using (var symmetricKey = new AesCryptoServiceProvider()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            using (var decryptor = symmetricKey.CreateDecryptor(key, ivStringBytes))
            using (var memoryStream = new MemoryStream(cipherTextBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                var plainTextBytes = new byte[cipherTextBytes.Length];
                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                return plainTextBytes.Take(decryptedByteCount).ToArray();
            }
        }

        private static string GetKey(string key)
        {
            var obfuscator = new CipherObfuscatorV2();
            var safeString = new SafeString(obfuscator.DeObfuscate(key));
            return safeString.AsString();
        }
    }
}