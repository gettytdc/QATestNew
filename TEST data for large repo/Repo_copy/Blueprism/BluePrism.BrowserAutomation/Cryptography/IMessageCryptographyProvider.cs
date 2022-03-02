namespace BluePrism.BrowserAutomation.Cryptography
{
    using System;

    /// <summary>
    /// Provides methods for encrypting/decrypting messages.
    /// </summary>
    public interface IMessageCryptographyProvider
    {
        /// <summary>
        /// Encrypts the message.
        /// </summary>
        /// <param name="key">The encryption key.</param>
        /// <returns>A function accepting the data to be encrypted and returning the encrypted data in base64.</returns>
        Func<string, string> EncryptMessage(string key);

        /// <summary>
        /// Decrypts the message.
        /// </summary>
        /// <param name="key">The encryption key.</param>
        /// <returns>A function accepting the data (in base64) to be decrypted and returning the decrypted data.</returns>
        Func<string, string> DecryptMessage(string key);
    }
}
