using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class TableExpandedHandlerTests : UIAutomationHandlerTestBase<TableExpandedHandler>
    {
        [TestCase(ExpandCollapseState.Expanded, "True")]
        [TestCase(ExpandCollapseState.Collapsed, "False")]
        public void Execute_WithChildrenSupportingPattern_ShouldReturnStateFromFirst(ExpandCollapseState state, string expected)
        {

            // Set up cell with ExpandCollapsePattern
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>();
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var childMock = new Mock<IAutomationElement>();
            cellMock.MockChildren(childMock);
            var patternMock = childMock.MockPattern<IExpandCollapsePattern>(PatternType.ExpandCollapsePattern);
            patternMock.Setup(p => p.CurrentExpandCollapseState).Returns(state);
            var query = clsQuery.Parse($"UIATableExpanded ColumnNumber=5 RowNumber=2");
            var reply = Execute(query);
            Assert.That(reply.Message, Is.EqualTo(expected));
        }
    }
}