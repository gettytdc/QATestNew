namespace BluePrism.BrowserAutomation.Cryptography
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides a wrapper for hashing algorithms.
    /// </summary>
    /// <typeparam name="TAlgorithm">The type of the algorithm.</typeparam>
    /// <seealso cref="BluePrism.BrowserAutomation.Cryptography.IHashAlgorithm" />
    public class HashAlgorithmWrapper <TAlgorithm> : IHashAlgorithm
        where TAlgorithm : HashAlgorithm
    {
        private readonly TAlgorithm _algorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashAlgorithmWrapper{TAlgorithm}"/> class.
        /// </summary>
        /// <param name="algorithm">The algorithm.</param>
        public HashAlgorithmWrapper(TAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="data">The data to produce a hash from.</param>
        /// <returns>A byte array containing the hashed data.</returns>
        public byte[] ComputeHash(byte[] data) => _algorithm.ComputeHash(data);

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

        ~HashAlgorithmWrapper()
        {
            Dispose(false);
        }

    }
}
