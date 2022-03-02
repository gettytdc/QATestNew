using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class GetToggleStateHandlerTests : UIAutomationHandlerTestBase<GetToggleStateHandler>
    {
        [TestCase(ToggleState.On, "True")]
        [TestCase(ToggleState.Off, "False")]
        [TestCase(ToggleState.Indeterminate, "False")]
        public void Execute_WithElementSupportingPattern_ReturnsState(ToggleState state, string expected)
        {
            var patternMock = ElementMock.MockPattern<ITogglePattern>();
            patternMock.Setup(p => p.CurrentToggleState).Returns(state);
            var reply = Execute(clsQuery.Parse("UIAGetToggleState"));
            Assert.That(reply.Message, Is.EqualTo(expected));
        }

        [Test]
        public void Execute_WithElementNotSupportingPattern_ShouldThrow()
        {
            Assert.Throws<PatternNotFoundException<ITogglePattern>>(() => Execute(clsQuery.Parse("UIAGetToggleState")));
        }
    }
}