namespace BluePrism.BrowserAutomation.Cryptography
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides a wrapper for symmetric algorithms.
    /// </summary>
    /// <typeparam name="TAlgorithm">The type of the algorithm.</typeparam>
    /// <seealso cref="BluePrism.BrowserAutomation.Cryptography.ISymmetricAlgorithm" />
    public class SymmetricAlgorithmWrapper <TAlgorithm> : ISymmetricAlgorithm
        where TAlgorithm : SymmetricAlgorithm
    {
        private readonly TAlgorithm _algorithm;

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public byte[] Key
        {
            get => _algorithm.Key;
            set => _algorithm.Key = value;
        }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        public CipherMode Mode
        {
            get => _algorithm.Mode;
            set => _algorithm.Mode = value;
        }

        /// <summary>
        /// Gets or sets the padding.
        /// </summary>
        public PaddingMode Padding
        {
            get => _algorithm.Padding;
            set => _algorithm.Padding = value;
        }

        /// <summary>
        /// Gets or sets the iv.
        /// </summary>
        public byte[] Iv
        {
            get => _algorithm.IV;
            set => _algorithm.IV = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricAlgorithmWrapper{TAlgorithm}"/> class.
        /// </summary>
        /// <param name="algorithm">The underlying algorithm.</param>
        public SymmetricAlgorithmWrapper(TAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        /// <summary>
        /// Creates the encryptor.
        /// </summary>
        /// <returns>
        /// An encryptor for the underlying algorithm.
        /// </returns>
        public ICryptoTransform CreateEncryptor() => _algorithm.CreateEncryptor();

        /// <summary>
        /// Creates the decryptor.
        /// </summary>
        /// <returns>
        /// An decryptor for the underlying algorithm.
        /// </returns>
        public ICryptoTransform CreateDecryptor() => _algorithm.CreateDecryptor();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _algorithm.Dispose();
            }                    

            _disposed = true;
        }

        ~SymmetricAlgorithmWrapper()
        {
            Dispose(false);
        }

    }
}
