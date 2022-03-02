#if UNITTESTS

using System;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.CharMatching.UnitTests
{
    public class ParseElementTests
    {
        [Test]
        public void GivenWhenElementIsNotParsed_ThenExceptionShouldBeThrown_AndErrorMessageShouldBeCorrect()
        {
            //Arrange
            const string parsedValue = "test value";
            const string errorMessage = "This is the expected error";

            //Act
          Action act = () => ElementParser.ParseElement<int>
                                (parsedValue, errorMessage, int.TryParse);

            //Assert
            act.ShouldThrow<InvalidOperationException>()
                .WithMessage(errorMessage);
        }

        [Test]
        public void GivenWhenElementIsParsed_ThenElementShouldBeReturned()
        {
            //Arrange
            const string parsedValue = "5";
            const int expectedResult = 5;
            const string errorMessage = "This is the expected error";

            //Act
            var result = ElementParser.ParseElement<int>(parsedValue, errorMessage, int.TryParse);

            //Assert
            result.Should().Be(expectedResult);
        }
    }
}
#endif
