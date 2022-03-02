namespace BluePrism.BrowserAutomation.Cryptography
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides methods for applying symmetric algorithms.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ISymmetricAlgorithm : IDisposable
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        byte[] Key { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        CipherMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the padding.
        /// </summary>
        PaddingMode Padding { get; set; }

        /// <summary>
        /// Gets or sets the iv.
        /// </summary>
        byte[] Iv { get; set; }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <returns>An encryptor for the underlying algorithm.</returns>
        ICryptoTransform CreateEncryptor();

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <returns>An decryptor for the underlying algorithm.</returns>
        ICryptoTransform CreateDecryptor();
    }
}
