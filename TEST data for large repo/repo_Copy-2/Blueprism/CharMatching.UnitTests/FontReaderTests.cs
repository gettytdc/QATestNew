#if UNITTESTS

using System;
using NUnit.Framework;

namespace BluePrism.CharMatching.UnitTests
{
    /// <summary>
    /// Unit tests for the FontReader class
    /// </summary>
    [TestFixture]
    public class FontReaderTests
    {
        /// <summary>
        ///  Tests that the parsing of the colour string works as expected and,
        /// indeed, fails when expected
        /// </summary>
        [Test]
        public void TestParseColourString()
        {
            const int red = 0xff0000;

            Assert.That(FontReader.ParseColourString("red"), Is.EqualTo(red));
            Assert.That(FontReader.ParseColourString("Red"), Is.EqualTo(red));
            Assert.That(FontReader.ParseColourString("ff0000"), Is.EqualTo(red));
            Assert.That(FontReader.ParseColourString("#ff0000"), Is.EqualTo(red));

            // transparent comes back as white
            Assert.That(FontReader.ParseColourString("transparent"),
                Is.EqualTo(0xffffff));
            Assert.That(FontReader.ParseColourString("#cafeba"),
                Is.EqualTo(0xcafeba));
            Assert.That(FontReader.ParseColourString("abacab"),
                Is.EqualTo(0xabacab));

            // Gold Value taken from http://tinyurl.com/kdbhq2y (MSDN)
            Assert.That(FontReader.ParseColourString("Gold"),
                Is.EqualTo(0xffd700));

            // Null and empty strings a) are valid and b) both return -1 values
            Assert.That(FontReader.ParseColourString(null), Is.EqualTo(-1));
            Assert.That(FontReader.ParseColourString(""), Is.EqualTo(-1));

            {
                int colInt;
                Assert.That(FontReader.TryParseColourString("nonsense", out colInt),
                    Is.False);
                // Should be -1 after a failed parsde
                Assert.That(colInt, Is.EqualTo(-1));
                colInt = 0;
                Assert.That(FontReader.TryParseColourString("fishygreen", out colInt),
                    Is.False);
                // Should be -1 after a failed parsde
                Assert.That(colInt, Is.EqualTo(-1));
            }

            try
            {
                FontReader.ParseColourString("no");
                Assert.Fail("ParseColourString(\"no\") should have failed");
            }
            catch (ArgumentException) { /* working correctly */ }
            catch (Exception e)
            {
                Assert.Fail(
                    "ParseColourString(\"no\") should throw ArgumentException; " +
                    "instead, it threw: " + e);
            }
        }
    }
}

#endif