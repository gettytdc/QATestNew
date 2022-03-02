using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.Server.Domain.Models;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class TableExpandCollapseHandlerTests : UIAutomationHandlerTestBase<TableExpandCollapseHandler>
    {
        [Test]
        public void Execute_WithChildrenSupportingPattern_ShouldToggleFirstChild()
        {
            var query = clsQuery.Parse("UIATableExpanded ColumnNumber=5 RowNumber=2");

            // Mock grid pattern implemented by root element
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            // Set up selected cell with child supporting ExpandCollapsePattern
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var childMock = new Mock<IAutomationElement>();
            cellMock.MockChildren(childMock);
            var patternMock = childMock.MockPattern<IExpandCollapsePattern>(PatternType.ExpandCollapsePattern);
            patternMock.Setup(m => m.CurrentExpandCollapseState).Returns(ExpandCollapseState.Expanded);
            var reply = Execute(query);
            patternMock.Verify(p => p.ExpandCollapse());
            Assert.That(reply, Is.EqualTo(Reply.Ok));
        }

        [Test]
        public void Execute_InvalidCell_ShouldThrow()
        {
            var query = clsQuery.Parse("UIATableExpanded ColumnNumber=5 RowNumber=2");

            // Mock grid pattern implemented by root element
            ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            Assert.Throws<NoSuchElementException>(() => Execute(query));
        }
    }
}
