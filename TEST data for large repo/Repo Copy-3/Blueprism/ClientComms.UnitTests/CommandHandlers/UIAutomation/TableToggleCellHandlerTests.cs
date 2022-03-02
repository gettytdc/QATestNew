using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class TableToggleCellHandlerTests : UIAutomationHandlerTestBase<TableToggleCellHandler>
    {
        [Test]
        public void Execute_WithChildrenSupportingToggle_ShouldToggleFirstElement()
        {
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>();
            // Set up cell and child
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var childMock1 = new Mock<IAutomationElement>();
            var childMock2 = new Mock<IAutomationElement>();
            cellMock.MockChildren(childMock1, childMock2);
            var patternMock1 = childMock1.MockPattern<ITogglePattern>(PatternType.TogglePattern);
            var patternMock2 = childMock2.MockPattern<ITogglePattern>(PatternType.TogglePattern);
            var query = clsQuery.Parse("UIATableToggleCell ColumnNumber=5 RowNumber=2");
            var reply = Execute(query);
            patternMock1.Verify(p => p.Toggle());
            patternMock2.Verify(p => p.Toggle(), Times.Never());
            Assert.That(reply, Is.EqualTo(Reply.Ok));
        }

        [Test]
        public void Execute_WithoutElementSupportingToggle_ShouldThrow()
        {
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>();
            // Set up cell to not contain any children implementing pattern
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var childMock = new Mock<IAutomationElement>();
            cellMock.MockChildren(childMock);
            var query = clsQuery.Parse("UIATableToggleCell ColumnNumber=5 RowNumber=2");
            Assert.Throws<PatternNotFoundException<ITogglePattern>>(() => Execute(query));
        }
    }
}