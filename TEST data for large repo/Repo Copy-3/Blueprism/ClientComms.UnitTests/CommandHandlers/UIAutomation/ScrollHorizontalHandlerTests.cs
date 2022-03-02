using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class ScrollHorizontalHandlerTests : UIAutomationHandlerTestBase<ScrollHorizontalHandler>
    {
        [TestCase(true, true, ScrollAmount.LargeDecrement)]
        [TestCase(true, false, ScrollAmount.SmallDecrement)]
        [TestCase(false, true, ScrollAmount.LargeIncrement)]
        [TestCase(false, false, ScrollAmount.SmallIncrement)]
        public void Execute_WithParameters_ShouldScroll(bool scrollUp, bool bigStep, ScrollAmount expectedAmount)
        {
            var patternMock = ElementMock.MockPattern<IScrollPattern>(PatternType.ScrollPattern);
            string query = $"UIAScrollHorizontal scrollup={scrollUp} bigstep={bigStep}";
            Execute(clsQuery.Parse(query));
            patternMock.Verify(p => p.Scroll(expectedAmount, ScrollAmount.NoAmount));
        }

        [Test]
        public void Execute_ElementWithoutScrollPattern_ShouldThrow()
        {
            Assert.Throws<PatternNotFoundException<IScrollPattern>>(() => Execute(clsQuery.Parse("UIAScrollHorizontal bigstep=True scrollup=True")));
        }
    }
}