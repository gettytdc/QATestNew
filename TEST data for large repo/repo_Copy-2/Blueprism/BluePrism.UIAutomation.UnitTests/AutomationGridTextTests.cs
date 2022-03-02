#if UNITTESTS

namespace BluePrism.UIAutomation.UnitTests
{
    using Moq;
    using NUnit.Framework;

    using Patterns;
    using BluePrism.Utilities.Testing;

    public class AutomationGridTextTests : UnitTestBase<AutomationGridText>
    {

        private const string TestString = "Automation Anywhere should stop targeting me with facebook ads.";

        [Test]
        public void GridWithNoPatternElementReturnsDefaultText()
        {
            var grid = GetMockGridWithNoPatternElement();
            var gridTextProvider = new AutomationGridText();

            var gridCell = grid.GetItem(0, 0);
            var textFromCell = gridTextProvider.GetTextFromElement(gridCell);

            Assert.IsTrue(string.Equals(textFromCell, TestString));
        }

        [Test]
        public void GridWithValuePatternElementReturnsDefaultText()
        {
            var grid = GetMockGridWithPatternElement<IValuePattern>(PatternType.ValuePattern);
            var gridTextProvider = new AutomationGridText();

            var gridCell = grid.GetItem(0, 0);
            var textFromCell = gridTextProvider.GetTextFromElement(gridCell);

            Assert.IsTrue(string.Equals(textFromCell, TestString));
        }

        [Test]
        public void GridWithTextChildPatternElementReturnsDefaultText()
        {
            var grid = GetMockGridWithPatternElement<ITextChildPattern>(PatternType.TextChildPattern);
            var gridTextProvider = new AutomationGridText();

            var gridCell = grid.GetItem(0, 0);
            var textFromCell = gridTextProvider.GetTextFromElement(gridCell);

            Assert.IsTrue(string.Equals(textFromCell, TestString));
        }

        [Test]
        public void GridWithTextPatternElementReturnsDefaultText()
        {
            var grid = GetMockGridWithPatternElement<ITextPattern>(PatternType.TextPattern);
            var gridTextProvider = new AutomationGridText();

            var gridCell = grid.GetItem(0, 0);
            var textFromCell = gridTextProvider.GetTextFromElement(gridCell);

            Assert.IsTrue(string.Equals(textFromCell, TestString));
        }

        private IGridPattern GetMockGridWithNoPatternElement()
        {
            var automationElementMock = GetMock<IAutomationElement>();
            automationElementMock
                .Setup(m => m.CurrentName)
                .Returns(TestString);

            var grid = GetMock<IGridPattern>();
            grid
                .Setup(m => m.GetItem(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => automationElementMock.Object);

            return grid.Object;
        }

        private IGridPattern GetMockGridWithPatternElement<TPattern>(PatternType patternType)
            where TPattern : IAutomationPattern
        {
            var mockPattern = GetMockPattern(patternType);

            var automationElementMock = GetMock<IAutomationElement>();
            automationElementMock
                .Setup(m => m.PatternIsSupported(patternType)) //It.IsAny<PatternType>()
                .Returns(true);

            automationElementMock
                .Setup(m => m.GetCurrentPattern<TPattern>())
                .Returns(() => (TPattern)mockPattern);

            var grid = GetMock<IGridPattern>();
            grid
                .Setup(m => m.GetItem(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(() => automationElementMock.Object);

            return grid.Object;
        }

        private IAutomationPattern GetMockPattern(PatternType patternType)
        {
            IAutomationPattern pattern;
            if (patternType == PatternType.ValuePattern)
            {
                var m = GetMock<IValuePattern>();
                m.SetupGet(p => p.CurrentValue).Returns(TestString);
                pattern = m.Object;
            }
            else if(patternType == PatternType.TextChildPattern)
            {
                var m = GetMock<ITextChildPattern>();
                m.Setup(p => p.TextRange.GetText(It.IsAny<int>())).Returns(TestString);
                pattern = m.Object;
            }
            else
            {
                var m = GetMock<ITextPattern>();
                m.Setup(p => p.DocumentRange.GetText(It.IsAny<int>())).Returns(TestString);
                pattern = m.Object;
            }

            return pattern;
        }
    }
}
#endif