namespace BluePrism.Core.UnitTests.Utility
{
    using System;
    using System.Security;
    using BluePrism.Core.Utility;
    using NUnit.Framework;

    [TestFixture]
    public class StringExtensionMethodsTests
    {
        [Test]
        public void ParseEnumWithValidValue()
        {
            var result = nameof(TestEnum.Value1).ParseEnum<TestEnum>();

            Assert.AreEqual(TestEnum.Value1, result);
        }

        [Test]
        public void ParseEnumWithInvalidValue()
        {
            TestDelegate test = () => "InvalidValue".ParseEnum<TestEnum>();
            Assert.Throws<ArgumentException>(test);
        }

        [Test]
        public void ToSecureStringWithText()
        {
            const string text = "This is a test";
            var result = text.ToSecureString();

            Assert.AreEqual(text.Length, result.Length);
        }

        [Test]
        public void ToSecureStringWithEmptyString()
        {
            var result = string.Empty.ToSecureString();

            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void MakeInsecureWithText()
        {
            const string text = "This is a test";
            var secureString = new SecureString();
            foreach (var character in text)
                secureString.AppendChar(character);

            var result = secureString.MakeInsecure();

            Assert.AreEqual(text, result);
        }

        [Test]
        public void MakeInsecureWithEmptyString()
        {
            var result = (new SecureString()).MakeInsecure();

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ToSecureStringAndMakeInsecureYieldInitialValue()
        {
            const string text = "This is a test";

            var result = text.ToSecureString().MakeInsecure();

            Assert.AreEqual(text, result);
        }

        [Test]
        public void TruncateWithEllipsis_CharacterLimitLessThanZero_ShouldThrowException()
        {
            Assert.That(() => "some test".TruncateWithEllipsis(-10),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void TruncateWithEllipsis_EmptyString_ShouldReturnEmptyString()
        { 
            Assert.That("".TruncateWithEllipsis(5), Is.EqualTo(""));
        }
        
        [Test]
        public void TruncateWithEllipsis_NullString_ShouldReturnEmptyString()
        {
            Assert.That(((string)null).TruncateWithEllipsis(5), Is.EqualTo(""));
        }

        [TestCase(0, ExpectedResult = "")]
        [TestCase(1, ExpectedResult = ".")]
        [TestCase(2, ExpectedResult = "..")]
        public string TruncateWithEllipsis_CharacterLimitLessThanEllipsisLength_ShouldReturnTruncatedEllipsis(int characterLimit)
        {
            return "test words".TruncateWithEllipsis(characterLimit);
        }

        [Test]
        public void TruncateWithEllipsis_StringLessThanCharacterLimit_ShouldReturnString()
        {
            Assert.That("test words".TruncateWithEllipsis(100), Is.EqualTo("test words"));
        }

        [Test]
        public void TruncateWithEllipsis_StringExceedsCharacterLimit_ShouldReturnTruncatedString()
        {
            Assert.That("test words".TruncateWithEllipsis(7), Is.EqualTo("test..."));
        }

        protected enum TestEnum
        {
            Value1,
            Value2
        }
    }
}
