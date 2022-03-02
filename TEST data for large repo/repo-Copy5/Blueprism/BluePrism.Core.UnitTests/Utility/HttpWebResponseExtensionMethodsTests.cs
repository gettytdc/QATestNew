#if UNITTESTS
namespace BluePrism.Core.UnitTests.Utility
{
    using System.Linq;
    using System.Text;
    using BluePrism.BPCoreLib;
    using BluePrism.Core.Utility;
    using BluePrism.Server.Domain.Models;
    using NUnit.Framework;

    /// <summary>
    /// Tests the methods in the <see cref="HttpWebResponseExtensionMethods"/> class
    /// </summary>
    [TestFixture]
    public class HttpWebResponseExtensionMethodsTests
    {
        [Test]
        public void GetEncodingFromName_InvalidName_ShouldThrowException()
        {
            byte[] emptyBytes = { };

            Assert.That(() => emptyBytes.GetEncodingFromName("invalid"), 
                Throws.InstanceOf<InvalidValueException>());
        }

        [Test]
        public void GetEncodingFromName_ValidName_ShouldReturnCorrectType()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            var result = bytes.GetEncodingFromName("utf-32");

            Assert.That(result.encoding, Is.EqualTo(Encoding.UTF32));

        }

        [Test]
        public void GetEncodingFromName_WithBomPrefix_ShouldReturnCorrectBomLength()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            var bytesWithBom = Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
            var result = bytesWithBom.GetEncodingFromName("utf-8");

            Assert.That(result.byteOrderMarkLength, 
                Is.EqualTo(Encoding.UTF8.GetPreamble().Length));
        }

        [Test]
        public void GetEncodingFromName_WithoutBomPrefix_ShouldReturnZeroBomLength()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            var result = bytes.GetEncodingFromName("utf-8");

            Assert.That(result.byteOrderMarkLength,
                Is.EqualTo(0));
        }

        [Test]
        public void GetEncodingFromByteOrderMarkPrefix_WithBomPrefix_ShouldReturnCorrectType()
        {
            var bytes = Encoding.BigEndianUnicode.GetBytes("Test String");
            var bytesWithBom = Encoding.BigEndianUnicode.GetPreamble().Concat(bytes).ToArray();
            var result = bytesWithBom.GetEncodingFromByteOrderMarkPrefix();

            Assert.That(result.encoding, Is.EqualTo(Encoding.BigEndianUnicode));
        }

        [Test]
        public void GetEncodingFromByteOrderMarkPrefix_WithBomPrefix_ShouldReturnCorrectBomLength()
        {
            var bytes = Encoding.Unicode.GetBytes("Test String");
            var bytesWithBom = Encoding.Unicode.GetPreamble().Concat(bytes).ToArray();
            var result = bytesWithBom.GetEncodingFromByteOrderMarkPrefix();

            Assert.That(result.byteOrderMarkLength, 
                Is.EqualTo(Encoding.Unicode.GetPreamble().Length));
        }
        
        [Test]
        public void GetEncodingFromByteOrderMarkPrefix_WithoutBomPrefix_ShouldReturnUtf8()
        {
            byte[] emptyBytes = { };
            var result = emptyBytes.GetEncodingFromByteOrderMarkPrefix();

            Assert.That(result.encoding, Is.EqualTo(Encoding.UTF8));
        }

        [Test]
        public void GetEncodingFromByteOrderMarkPrefix_WithoutBomPrefix_ShouldReturnZeroBomLength()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            var result = bytes.GetEncodingFromByteOrderMarkPrefix();

            Assert.That(result.byteOrderMarkLength, Is.EqualTo(0));
        }

        [Test]
        public void StartsWithByteOrderMark_WithBomPrefix_ShouldReturnTrue()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            var byteOrderMark = Encoding.UTF8.GetPreamble();
            var bytesWithBom = byteOrderMark.Concat(bytes).ToArray();

            Assert.That(bytesWithBom.StartsWithByteOrderMark(byteOrderMark), Is.EqualTo(true));

        }

        [Test]
        public void StartsWithByteOrderMark_WithoutBomPrefix_ShouldReturnFalse()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            var byteOrderMark = Encoding.UTF8.GetPreamble();
            
            Assert.That(bytes.StartsWithByteOrderMark(byteOrderMark), Is.EqualTo(false));
        }

        [Test]
        public void StartsWithByteOrderMark_BomIsNull_ShouldReturnFalse()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            byte[] byteOrderMark = null;

            Assert.That(bytes.StartsWithByteOrderMark(byteOrderMark), Is.EqualTo(false));
        }

        [Test]
        public void ByteOrderMarkPrefixLength_WithBomPrefix_ShouldReturnCorrectLength()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            var byteOrderMark = Encoding.UTF8.GetPreamble();
            var bytesWithBom = byteOrderMark.Concat(bytes).ToArray();

            Assert.That(bytesWithBom.ByteOrderMarkPrefixLength(byteOrderMark), 
                Is.EqualTo(byteOrderMark.Length));
        }

        [Test]
        public void ByteOrderMarkPrefixLength_WithoutBomPrefix_ShouldReturnZero()
        {
            var bytes = Encoding.UTF8.GetBytes("Test String");
            var byteOrderMark = Encoding.UTF8.GetPreamble();
            
            Assert.That(bytes.ByteOrderMarkPrefixLength(byteOrderMark), Is.EqualTo(0));
        }

    }

}
#endif
