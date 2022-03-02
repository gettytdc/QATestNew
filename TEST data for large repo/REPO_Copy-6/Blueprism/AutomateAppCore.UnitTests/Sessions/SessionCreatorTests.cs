using System;
using BluePrism.AutomateAppCore.Sessions;
using FluentAssertions;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Sessions
{
    [TestFixture]
    public class SessionCreatorTests
    {
        [Test]
        public void GivenSessionCreatorClassIsInstansiated_AndResourceConnectionManagerIsNothing_ThenArgumentExceptionShouldBeThrown()
        {
            // Arrange
            // Act
            Action act = () => new SessionCreator(null, null, null, null);

            // Assert
            act.ShouldThrowExactly<ArgumentException>()
                .And.Message.Should().StartWith(BluePrism.AutomateAppCore.My.Resources.Resources.ParameterCannotBeNull);
        }
    }
}
