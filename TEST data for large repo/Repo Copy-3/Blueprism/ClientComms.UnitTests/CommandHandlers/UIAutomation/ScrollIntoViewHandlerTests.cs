using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class ScrollIntoViewHandlerTests : UIAutomationHandlerTestBase<ScrollIntoViewHandler>
    {
        [Test]
        public void Execute_WithElementUsingPattern_ShouldToggle()
        {
            var patternMock = ElementMock.MockPattern<IScrollItemPattern>();
            var reply = Execute(clsQuery.Parse("UIAScrollIntoView"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
            patternMock.Verify(p => p.ScrollIntoView());
        }

        [Test]
        public void Execute_WithElementNotImplementingPatterns_ShouldThrow()
        {
            Assert.Throws<PatternNotFoundException<IScrollItemPattern>>(() => Execute(clsQuery.Parse("UIAScrollIntoView")));
        }
    }
}