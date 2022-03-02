namespace BluePrism.BrowserAutomation.Cryptography
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Utilities.Functional;

    /// <summary>
    /// Provides methods for encrypting/decrypting messages.
    /// </summary>
    /// <seealso cref="BluePrism.BrowserAutomation.Cryptography.IMessageCryptographyProvider" />
    public class MessageCryptographyProvider : IMessageCryptographyProvider
    {
        private readonly Func<IHashAlgorithm> _hashAlgorithmFactory;
        private readonly Func<ISymmetricAlgorithm> _symmetricAlgorithmFactory;
        private readonly Func<Stream, ICryptoTransform, CryptoStreamMode, ICryptoStream> _cryptoStreamFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageCryptographyProvider"/> class.
        /// </summary>
        /// <param name="hashAlgorithmFactory">The hash algorithm factory.</param>
        /// <param name="symmetricAlgorithmFactory">The symmetric algorithm factory.</param>
        /// <param name="cryptoStreamFactory">The crypto stream factory.</param>
        public MessageCryptographyProvider(
            Func<IHashAlgorithm> hashAlgorithmFactory,
            Func<ISymmetricAlgorithm> symmetricAlgorithmFactory,
            Func<Stream, ICryptoTransform, CryptoStreamMode, ICryptoStream> cryptoStreamFactory)
        {
            _hashAlgorithmFactory = hashAlgorithmFactory;
            _symmetricAlgorithmFactory = symmetricAlgorithmFactory;
            _cryptoStreamFactory = cryptoStreamFactory;
        }

        /// <summary>
        /// Encrypts the message.
        /// </summary>
        /// <param name="key">The encryption key.</param>
        /// <returns>
        /// A function accepting the data to be encrypted and returning the encrypted data in base64.
        /// </returns>
        public Func<string, string> EncryptMessage(string key) => message =>
        {
            var dataBytes = message.Map(Encoding.UTF8.GetBytes);
            var keyBytes = key.Map(HashKey);

            return
                EncryptData(dataBytes, keyBytes)
                    .Map(Convert.ToBase64String);
        };

        /// <summary>
        /// Decrypts the message.
        /// </summary>
        /// <param name="key">The encryption key.</param>
        /// <returns>
        /// A function accepting the data (in base64) to be decrypted and returning the decrypted data.
        /// </returns>
        public Func<string, string> DecryptMessage(string key) => message =>
        {
            var dataBytes = message.Map(Convert.FromBase64String);
            var keyBytes = key.Map(HashKey);

            return DecryptData(dataBytes, keyBytes);
        };

        private static (byte[] Iv, byte[] Data) ExtractDecryptionInformation(byte[] message) =>
        (
            message.Skip(message.Length - 16).ToArray(),
            message.Take(message.Length - 16).ToArray()
        );

        private static string WithDecryptionInformation(byte[] data, Func<(byte[] Iv, byte[] Data), string> func) =>
            data
                .Map(ExtractDecryptionInformation)
                .Map(func);

        private ISymmetricAlgorithm SetupSymmetricAlgorithm(byte[] key) =>
            _symmetricAlgorithmFactory()
                .Tee(x => x.Key = key)
                .Tee(x => x.Mode = CipherMode.CBC)
                .Tee(x => x.Padding = PaddingMode.PKCS7);

        private byte[] HashKey(string key) =>
            key
                .Map(Encoding.UTF8.GetBytes)
                .Map(x =>
                    _hashAlgorithmFactory()
                        .Use(crypto => crypto.ComputeHash(x)));

        private byte[] EncryptData(byte[] data, byte[] key) =>
            SetupSymmetricAlgorithm(key)
                .Use(cryptoProvider => cryptoProvider
                    .CreateEncryptor()
                    .Map(EncryptWithEncryptor(data))
                    .Concat(cryptoProvider.Iv))
                .ToArray();

        private Action<Stream> WriteEncryptedDataToStream(byte[] data, ICryptoTransform encryptor) => stream =>
            _cryptoStreamFactory(stream, encryptor, CryptoStreamMode.Write)
                .Use(cryptoStream => cryptoStream.Write(data, 0, data.Length));

        private Func<ICryptoTransform, byte[]> EncryptWithEncryptor(byte[] data) => encryptor =>
        {
            using (var memoryStream = new MemoryStream())
            {
                return
                    memoryStream
                    .Tee<MemoryStream>(WriteEncryptedDataToStream(data, encryptor))
                    .ToArray();
            }
        };

        private string DecryptData(byte[] data, byte[] key) =>
            WithDecryptionInformation(data, decryptionInformation =>
                SetupSymmetricAlgorithm(key)
                    .Use(cryptoProvider => cryptoProvider
                        .Tee(x => x.Iv = decryptionInformation.Iv)
                        .CreateDecryptor()
                        .Map(DecryptWithDecryptor(decryptionInformation.Data))));

        private Func<ICryptoTransform, string> DecryptWithDecryptor(byte[] data) => decryptor =>
            new MemoryStream(data)
                .Use(ReadDecryptedDataFromStream(decryptor));

        private Func<Stream, string> ReadDecryptedDataFromStream(ICryptoTransform decryptor) => stream =>
            _cryptoStreamFactory(stream, decryptor, CryptoStreamMode.Read)
                .Use(x => x.ReadToEnd());
    }
}
