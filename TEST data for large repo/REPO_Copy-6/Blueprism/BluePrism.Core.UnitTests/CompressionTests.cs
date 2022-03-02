#if UNITTESTS

using System.Runtime.Serialization;
using BluePrism.Core.Compression;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests
{
    [TestFixture]
    public class CompressionTests
    {
        [Test]
        public void GZipCompression_TestCompressSimpleString()
        {
            string input = "Mary had a little lamb, It's fleece was white as snow; And everywhere that Mary went. The lamb was sure to go.";

            var zipped = GZipCompression.Compress(input);
            var output = GZipCompression.Decompress(zipped);

            Assert.AreEqual(input, output, "Input string matches output string");

        }

        [Test]
        public void GZipCompression_TestCompressEmpty()
        {
            string input = "";

            var zipped = GZipCompression.Compress(input);
            var output = GZipCompression.Decompress(zipped);

            Assert.AreEqual(input, output, "Input string matches output string");
        }

        [Test]
        public void GZipCompression_TestCompressUnicode()
        {
            string input = "メアリーは子羊を少し持っていました、それはフリースは雪のように白でした。 そしてメアリーはあちこちで行った。 子羊は必ず行った。";

            var zipped = GZipCompression.Compress(input);
            var output = GZipCompression.Decompress(zipped);

            Assert.AreEqual(input, output, "Input string matches output string");
        }

        [Test]
        public void GZipCompression_TestCompressedAndSerializedSimple()
        {
            string input = "The quick brown fox jumps over the lazy dog.";

            var zipped = GZipCompression.SerializeAndCompress(input);
            var output = GZipCompression.InflateAndDeserialize<string>(zipped);

            Assert.AreEqual(input, output, "Input string matches output string");

        }

        [Test]
        public void GZipCompression_TestCompressedAndSerializedUnicode()
        {
            string input = "敏捷的棕色狐狸跳过了懒狗。";

            var zipped = GZipCompression.SerializeAndCompress(input);
            var output = GZipCompression.InflateAndDeserialize<string>(zipped);

            Assert.AreEqual(input, output, "Input string matches output string");

        }

        [Test]
        public void GZipCompression_TestCompressedAndSerializedEmpty()
        {
            string input = "";

            var zipped = GZipCompression.SerializeAndCompress(input);
            var output = GZipCompression.InflateAndDeserialize<string>(zipped);

            Assert.AreEqual(input, output, "Input string matches output string");

        }

    }
}
#endif
