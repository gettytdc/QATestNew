namespace BluePrism.BrowserAutomation.Cryptography
{
    using System;

    /// <summary>
    /// Provides methods for wrapping <see cref="System.Security.Cryptography.CryptoStream"/>
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ICryptoStream : IDisposable
    {
        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset in the array to take bytes from.</param>
        /// <param name="count">The number of bytes to write.</param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Returns all data in the stream.
        /// </summary>
        /// <returns>Data in the stream as a string.</returns>
        string ReadToEnd();
    }
}
