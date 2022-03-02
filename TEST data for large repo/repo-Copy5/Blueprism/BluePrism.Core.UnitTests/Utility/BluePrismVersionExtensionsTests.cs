using BluePrism.Core.Utility;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace BluePrism.Core.UnitTests.Utility
{
    public class BluePrismVersionExtensionsTests
    {
        [TestFixture]
        public class The_GetFromAssembly_Method
        {
            [TestCase(0)]
            [TestCase(1)]
            [TestCase(2)]
            [TestCase(3)]
            [TestCase(4)]
            public void TestVersionReturned(int fieldCount)
            {
                var version = this.GetBluePrismVersion(fieldCount);

                var expectedNumberOfDots = fieldCount - 1;
                if (expectedNumberOfDots < 0) expectedNumberOfDots = 0;
                var minimumExpectedLength = fieldCount + expectedNumberOfDots;
                version.Length.Should().BeGreaterOrEqualTo(minimumExpectedLength);
            }

            [TestCase(-1)]
            [TestCase(5)]
            public void TestInvalidFieldCountRequested(int fieldCount)
            {
                Action version = () => this.GetBluePrismVersion(fieldCount);

                version.ShouldThrow<ArgumentException>().WithMessage(
                    "Argument must be between 0 and 4.\r\nParameter name: fieldCount");
            }
        }
    }
}
