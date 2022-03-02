namespace BluePrism.BrowserAutomation.Cryptography
{
    using System;

    /// <summary>
    /// Provides methods for applying hash algorithms.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IHashAlgorithm : IDisposable
    {
        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="data">The data to produce a hash from.</param>
        /// <returns>A byte array containing the hashed data.</returns>
        byte[] ComputeHash(byte[] data);
    }
}
