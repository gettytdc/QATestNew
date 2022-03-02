using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class PressHandlerTests : UIAutomationHandlerTestBase<PressHandler>
    {
        [Test]
        public void Execute_ShouldReturnOK()
        {
            var patternMock = ElementMock.MockPattern<IInvokePattern>();
            var reply = Execute(clsQuery.Parse("UIAPress"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
            patternMock.Verify(p => p.Invoke(), Times.Once());
        }
    }
}