using BluePrism.Core.Utility;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.Utility
{
    public class StringExtensionsTests
    {
        [TestFixture]
        public class The_ContainsLeadingOrTrailingWhiteSpace_Method
        {
            private readonly string _testData = "http:/www.someurl.com";

            [Test]
            public void TestNoWhitespace()
            {
                var validationResult = _testData.ContainsLeadingOrTrailingWhitespace();

                validationResult.Should().BeFalse(because: "test data contains no whitespace");
            }

            [Test]
            public void TestLeadingWhitespace()
            {
                var validationResult = StringExtensionMethods.ContainsLeadingOrTrailingWhitespace($"  {_testData}");

                validationResult.Should().BeTrue(because: "test data contains leading whitespace");
            }

            [Test]
            public void TestTrailingWhitespace()
            {
                var validationResult = StringExtensionMethods.ContainsLeadingOrTrailingWhitespace($"{_testData}  ");

                validationResult.Should().BeTrue(because: "test data contains trailing whitespace");
            }

            [Test]
            public void TestEmptyString()
            {
                var validationResult = "".ContainsLeadingOrTrailingWhitespace();

                validationResult.Should().BeFalse(because: "the string is empty");
            }

            [Test]
            public void TestNullString()
            {
                string nullTestString = null;

                var validationResult = nullTestString.ContainsLeadingOrTrailingWhitespace();

                validationResult.Should().BeFalse(because: "we don't want to throw an exception unless necessary");
            }
        }
    }
}
