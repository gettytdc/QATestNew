using BluePrism.Core.Properties;
using BluePrism.BPCoreLib;
using BluePrism.Common.Security;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Core.Encryption
{
    /// <summary>
    /// Helper class for encrypting data to go into the database.
    /// </summary>
    public class DatabaseDataEncryption
    {
        private const int IvSize = 16;

        private const string ObfuscatedKey = "LWUXWVtoD1dvPEYEGDtHVyEXUiIQcm0L";

        /// <summary>
        /// Encrypts the data along with a row identifer
        /// </summary>
        /// <param name="rowIdentifier">string to uniquely identify the row in the table this data belongs to. </param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EncryptWithRowIdentifier(string rowIdentifier, string data)
        {
            return Encrypt(string.Join("|", rowIdentifier, data));
        }

        public static string DecryptAndVerifRowIdentifier(string expectedRowIdentifier, string encryptedData)
        {
            var decrypted = Decrypt(encryptedData);
            var splitString = decrypted.Split('|');
            var rowidentifier = splitString[0];
            var data = splitString[1];

            if (rowidentifier != expectedRowIdentifier)
            {
                throw new BluePrismException(Resource.DatabaseDataEncryption_DecryptedData0DoesnTHaveExpectedRowIdentifier1, data, expectedRowIdentifier);
            }

            return data;
        }

        
        private static string Encrypt(string plainText)
        {

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            var algorithm = new AesCryptoServiceProvider()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            using (var encryptor = algorithm.CreateEncryptor(DeObfuscateKey(ObfuscatedKey), algorithm.IV))
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(algorithm.IV.Concat(memoryStream.ToArray()).ToArray());
            }
        }



        private static string Decrypt(string encrypted)
        {
            var encryptedBytes = Convert.FromBase64String(encrypted);

            var ivStringBytes = encryptedBytes.Take(IvSize).ToArray();
            var cipherTextBytes = encryptedBytes.Skip(IvSize).ToArray();

            using (var symmetricKey = new AesCryptoServiceProvider()
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            using (var decryptor = symmetricKey.CreateDecryptor(DeObfuscateKey(ObfuscatedKey), ivStringBytes))
            using (var memoryStream = new MemoryStream(cipherTextBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                var plainTextBytes = new byte[cipherTextBytes.Length];
                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                return Encoding.UTF8.GetString(plainTextBytes.Take(decryptedByteCount).ToArray());
            }
        }

        private static byte[] DeObfuscateKey(string key)
        {
            var obfuscator = new CipherObfuscatorV2();
            var safeString = new SafeString(obfuscator.DeObfuscate(key));
            return Convert.FromBase64String(safeString.AsString());
        }
    }
}
