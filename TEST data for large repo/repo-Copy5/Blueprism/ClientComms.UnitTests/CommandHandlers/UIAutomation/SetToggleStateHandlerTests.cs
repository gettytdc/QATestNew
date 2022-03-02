using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class SetToggleStateHandlerTests : UIAutomationHandlerTestBase<SetToggleStateHandler>
    {
        [TestCase("NewText=True", ToggleState.Off, ToggleState.On)]
        [TestCase("NewText=True", ToggleState.On, ToggleState.On)]
        [TestCase("NewText=False", ToggleState.Off, ToggleState.Off)]
        [TestCase("NewText=False", ToggleState.On, ToggleState.Off)]
        public void Execute_WithElementUsingPattern_ShouldToggleIfNecessary(string parameters, ToggleState initialState, ToggleState expectedState)
        {
            var patternMock = ToggleSetupHelper.SetupState(ElementMock, initialState);
            var reply = Execute(clsQuery.Parse($"UIASetToggleState {parameters}"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
            Assert.That(patternMock.Object.CurrentToggleState, Is.EqualTo(expectedState));
        }

        [Test]
        public void Execute_WithElementNotImplementingPatterns_ShouldThrow()
        {
            Assert.Throws<PatternNotFoundException<ITogglePattern>>(() => Execute(clsQuery.Parse("UIASetToggleState")));
        }
    }
}