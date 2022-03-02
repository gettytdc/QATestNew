using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class ExpandCollapseHandlerTests : UIAutomationHandlerTestBase<ExpandCollapseHandler>
    {
        [Test]
        public void Execute_WithElementUsingPattern_ShouldToggle()
        {
            var patternMock = ElementMock.MockPattern<IExpandCollapsePattern>();
            var reply = Execute(clsQuery.Parse("UIAExpandCollapse"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
            patternMock.Verify(p => p.ExpandCollapse());
        }

        [Test]
        public void Execute_WithElementNotImplementingPatterns_ShouldThrow()
        {
            Assert.Throws<PatternNotFoundException<IExpandCollapsePattern>>(() => Execute(clsQuery.Parse("UIAExpandCollapse")));
        }
    }
}