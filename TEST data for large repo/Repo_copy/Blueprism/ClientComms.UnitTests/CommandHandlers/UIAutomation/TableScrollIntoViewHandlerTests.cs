using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.Server.Domain.Models;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class TableScrollIntoViewHandlerTests : UIAutomationHandlerTestBase<TableScrollIntoViewHandler>
    {
        [Test]
        public void Execute_ItemWithCell_ShouldScrollIntoView()
        {
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            // Set up cell that supports ScrollItemPattern
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var patternMock = cellMock.MockPattern<IScrollItemPattern>();
            // Set up mock for to ensure that TextRange will return a mock object
            patternMock.Setup(p => p.ScrollIntoView());
            var query = clsQuery.Parse($"UIATableScrollIntoView ColumnNumber=5 RowNumber=2");
            var reply = Execute(query);
            Assert.That(reply, Is.EqualTo(Reply.Ok));
            patternMock.Verify(p => p.ScrollIntoView());
        }

        [Test]
        public void Execute_InvalidCell_ShouldThrow()
        {
            var query = clsQuery.Parse($"UIATableScrollIntoView ColumnNumber=5 RowNumber=2");
            ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            Assert.Throws<NoSuchElementException>(() => Execute(query));
        }
    }
}
