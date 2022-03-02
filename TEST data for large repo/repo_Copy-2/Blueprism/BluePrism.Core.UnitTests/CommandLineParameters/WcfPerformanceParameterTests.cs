using System;
using BluePrism.Core.CommandLineParameters;
using FluentAssertions;
using NUnit.Framework;

namespace BluePrism.Core.UnitTests.CommandLineParameters
{
    public class WcfPerformanceParameterTests
    {
        [Test]
        public void GivenWcfPerformanceLoggingIsAValidInteger_AndIsBetweenMinAndMaxValues_ThenValueIsValid()
        {
            //Arrange
            const string testValue = "45";
            const int expectedValue = 45;

            //Act
            var parameter = new WcfPerformanceTestingParameter(testValue);

            //Assert
            parameter.PerformanceTestDurationMinutes.Should().Be(expectedValue);
        }

        [TestCase("test12345")]
        [TestCase("-1")]
        [TestCase("0")]
        [TestCase("1141")]
        public void GivenWcfPerformanceLoggingIsNotAValidValue_ThenExceptionIsThrown(string testValue)
        {
            //Act
            Action act = () => new WcfPerformanceTestingParameter(testValue);

            //Assert
            act.ShouldThrow<ArgumentException>();
        }
    }
}
