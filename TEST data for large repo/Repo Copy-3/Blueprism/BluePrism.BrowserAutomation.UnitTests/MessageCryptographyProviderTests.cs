#if UNITTESTS

namespace BluePrism.BrowserAutomation.UnitTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Utilities.Functional;
    using FluentAssertions;
    using NUnit.Framework;
    using Cryptography;
    using BluePrism.Utilities.Testing;
    using Autofac;

    [TestFixture]
    public class MessageCryptographyProviderTests : UnitTestBase<MessageCryptographyProvider>
    {
        private const string StringToEncrypt = "Hello, World!";
        private const string Key = "1234567890";

        private Func<Stream, ICryptoTransform, CryptoStreamMode, ICryptoStream> _cryptoStreamFactory;
        private IHashAlgorithm _hashAlgorithm;
        private ISymmetricAlgorithm _symmetricAlgorithm;

        public override void Setup()
        {
            base.Setup(builder =>
            {
                builder.Register(_ => _hashAlgorithm);
                builder.Register(_ => _symmetricAlgorithm);
                builder.Register(_ => _cryptoStreamFactory);
            });
            _hashAlgorithm = Moq.Mock.Of<IHashAlgorithm>();
            _symmetricAlgorithm = Moq.Mock.Of<ISymmetricAlgorithm>();
            _cryptoStreamFactory = (s, t, m) => new CryptoStreamMock(s);
        }

        [Test]
        public void EncryptionReturnsExpectedValue()
        {
            var cryptoTransformMock = GetMock<ICryptoTransform>();
            var symmetricAlgorithmMock = GetMock<ISymmetricAlgorithm>();
            symmetricAlgorithmMock
                .Setup(m => m.CreateEncryptor())
                .Returns(cryptoTransformMock.Object);

            _symmetricAlgorithm = symmetricAlgorithmMock.Object;

            var result = ClassUnderTest.EncryptMessage(Key)(StringToEncrypt);

            result.Should().Be(StringToEncrypt.Map(Encoding.UTF8.GetBytes).Map(Convert.ToBase64String));
        }

        [Test]
        public void EncryptionAppendsIv()
        {
            var iv = Enumerable.Range(0, 16).Select(x => (byte)x).ToArray();
            var cryptoTransformMock = GetMock<ICryptoTransform>();
            var symmetricAlgorithmMock = GetMock<ISymmetricAlgorithm>();
            symmetricAlgorithmMock
                .Setup(m => m.CreateEncryptor())
                .Returns(cryptoTransformMock.Object);
            symmetricAlgorithmMock
                .SetupGet(m => m.Iv)
                .Returns(iv);

            _symmetricAlgorithm = symmetricAlgorithmMock.Object;

            var result = ClassUnderTest.EncryptMessage(Key)(StringToEncrypt);
            var resultBytes = result.Map(Convert.FromBase64String);

            resultBytes.Skip(resultBytes.Length - 16).Should().Equal(iv);
        }

        [Test]
        public void DecryptionReturnsResult()
        {
            _hashAlgorithm = new HashAlgorithmWrapper<SHA256CryptoServiceProvider>(new SHA256CryptoServiceProvider());
            _symmetricAlgorithm = new SymmetricAlgorithmWrapper<AesCryptoServiceProvider>(new AesCryptoServiceProvider());
            _cryptoStreamFactory = (stream, transform, mode) => new CryptoStreamWrapper(stream, transform, mode);

            var testEncryptedMessage = "HriSogEMBFaubQNMH4F2EJHuC4HZ5zjOtMr8gI2UYlU=";
            var testDecryptedMessage = "This is a test";
            var testKey = "Test";

            var result = ClassUnderTest.DecryptMessage(testKey)(testEncryptedMessage);

            result.Should().Be(testDecryptedMessage);
        }
    }

    public class CryptoStreamMock : ICryptoStream
    {
        private readonly Stream _stream;

        public CryptoStreamMock(Stream stream)
        {
            _stream = stream;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _stream.Dispose();

            _disposed = true;
        }

        ~CryptoStreamMock()
        {
            Dispose(false);
        }


        public string ReadToEnd()
        {
            throw new NotImplementedException();
        }
    }
}

#endif
