namespace BluePrism.BrowserAutomation.Cryptography
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using Utilities.Functional;

    /// <summary>
    /// Provides a wrapper for <see cref="System.Security.Cryptography.CryptoStream"/>
    /// </summary>
    /// <seealso cref="BluePrism.BrowserAutomation.Cryptography.ICryptoStream" />
    public class CryptoStreamWrapper : ICryptoStream
    {
        private readonly CryptoStream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoStreamWrapper"/> class.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <param name="cryptoTransform">The cryptographic transform being applied.</param>
        /// <param name="mode">The stream mode.</param>
        public CryptoStreamWrapper(Stream stream, ICryptoTransform cryptoTransform, CryptoStreamMode mode)
        {
            _stream = new CryptoStream(stream, cryptoTransform, mode);
        }

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
                _stream.Dispose();
            }                    

            _disposed = true;
        }

        ~CryptoStreamWrapper()
        {
            Dispose(false);
        }


        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset in the array to take bytes from.</param>
        /// <param name="count">The number of bytes to write.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Returns all data in the stream.
        /// </summary>
        /// <returns>Data in the stream as a string.</returns>
        public string ReadToEnd() =>
            new StreamReader(_stream)
                .Use(x => x.ReadToEnd());
    }
}
