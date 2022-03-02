using System.Collections.Generic;
using BluePrism.BPServer.FormValidationRules;
using BluePrism.BPServer.Properties;
using NUnit.Framework;

namespace BPServer.UnitTests
{
    public class FormValidationTests
    {
        [Test]
        public void GivenPortIsTooLow_ThenValidationErrorShown()
        {
            //Arrange
            var portNum = 1023;
            var portRule = new PortRangeRule(portNum);
            var expectedErrorMsg = Resources.InvalidPort;

            //Act
            var result = portRule.IsValid();

            //Assert
            Assert.IsFalse(result.Result);
            Assert.IsTrue(result.ErrorMessage == expectedErrorMsg);
        }

        [Test]
        public void GivenPortIsTooHigh_ThenValidationErrorShown()
        {
            //Arrange
            var portNum = 65354;
            var portRule = new PortRangeRule(portNum);
            var expectedErrorMsg = Resources.InvalidPort;

            //Act
            var result = portRule.IsValid();

            //Assert
            Assert.IsFalse(result.Result);
            Assert.IsTrue(result.ErrorMessage == expectedErrorMsg);
        }

        [Test]
        public void GivenPortIsWithinValidRange_ThenNoErrorIsShown()
        {
            //Arrange
            var portNum = 65100;
            var portRule = new PortRangeRule(portNum);

            //Act
            var result = portRule.IsValid();

            //Assert
            Assert.IsTrue(result.Result);
            Assert.IsTrue(result.ErrorMessage == string.Empty);
        }

        [Test]
        public void GivenPortIsSameAsServerPort_ThenErrorIsShown()
        {
            //Arrange
            var portRule = new UniquePortRule(5600, 5600);
            var expectedErrorMessage = Resources.ConflictServerBindingPort;

            //Act
            var results = portRule.IsValid();

            //Assert
            Assert.IsFalse(results.Result);
            Assert.IsTrue(results.ErrorMessage == expectedErrorMessage);
        }

        [Test]
        public void GivenPortIsDifferentToServerPort_ThenNoErrorIsShown()
        {
            //Arrange
            var portRule = new UniquePortRule(5600, 5700);
            var expectedErrorMessage = string.Empty;

            //Act
            var results = portRule.IsValid();

            //Assert
            Assert.IsTrue(results.Result);
            Assert.IsTrue(results.ErrorMessage == expectedErrorMessage);
        }

        [Test]
        public void GivenHostNameIsEmpty_ThenErrorIsShown()
        {
            //Arrange
            var hostNameRule = new HostNamePopulatedRule(string.Empty);
            var expectedErrorMesg = Resources.InvalidHostname;

            //Act
            var result = hostNameRule.IsValid();

            //Assert
            Assert.IsFalse(result.Result);
            Assert.IsTrue(result.ErrorMessage == expectedErrorMesg);
        }

        [Test]
        public void GivenHostNameIsWhitespace_ThenErrorIsShown()
        {
            //Arrange
            var hostNameRule = new HostNamePopulatedRule("      ");
            var expectedErrorMesg = Resources.InvalidHostname;

            //Act
            var result = hostNameRule.IsValid();

            //Assert
            Assert.IsFalse(result.Result);
            Assert.IsTrue(result.ErrorMessage == expectedErrorMesg);
        }

        [Test]
        public void GivenHostNameIsPopulated_ThenNoErrorIsShown()
        {
            //Arrange
            var hostNameRule = new HostNamePopulatedRule("localhost");
            var expectedErrorMesg = string.Empty;

            //Act
            var result = hostNameRule.IsValid();

            //Assert
            Assert.IsTrue(result.Result);
            Assert.IsTrue(result.ErrorMessage == expectedErrorMesg);
        }

        
        [Test]
        public void GivenErrorsArePresent_ThenCorrectNumberAreReturned()
        {
            //Arrange
            var results = new List<string>();

            //Act
           results.AddRange(AscrSettingsFormValidator.Validate(string.Empty, 1025, 8999,true));

            //Assert
            Assert.IsTrue(results.Count == 1);
        }

        [Test]
        public void GivenNoErrors_ThenNoErrorMessagesAreReturned()
        {
            //Arrange
            var results = new List<string>();

            //Act
            results.AddRange(AscrSettingsFormValidator.Validate("localhost", 8998, 8999, true));

            //Assert
            Assert.IsTrue(results.Count == 0);
        }
    }
}
