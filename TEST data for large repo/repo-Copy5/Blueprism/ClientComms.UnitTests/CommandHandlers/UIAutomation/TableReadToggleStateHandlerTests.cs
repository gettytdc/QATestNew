using BluePrism.ApplicationManager;
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
    internal class TableReadToggleStateHandlerTests : UIAutomationHandlerTestBase<TableReadToggleStateHandler>
    {
        [TestCase(ToggleState.On, true)]
        [TestCase(ToggleState.Off, false)]
        [TestCase(ToggleState.Indeterminate, false)]
        public void Execute_WithChildSupportingToggle_ShouldMapStateFromFirst(ToggleState state, bool expected)
        {
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>();
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var childMock1 = new Mock<IAutomationElement>();
            var childMock2 = new Mock<IAutomationElement>();
            cellMock.MockChildren(childMock1, childMock2);
            var patternMock1 = childMock1.MockPattern<ITogglePattern>(PatternType.TogglePattern);
            var patternMock2 = childMock2.MockPattern<ITogglePattern>(PatternType.TogglePattern);
            patternMock1.Setup(p => p.CurrentToggleState).Returns(state);
            var query = clsQuery.Parse($"UIATableReadToggleState ColumnNumber=5 RowNumber=2");
            var reply = Execute(query);
            var expectedReply = expected ? Reply.True : Reply.False;
            Assert.That(reply, Is.EqualTo(expectedReply));
            patternMock2.Verify(p => p.CurrentToggleState, Times.Never());
        }

        [Test]
        public void Execute_NoChildrenSupportingToggle_ShouldThrow()
        {
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>();
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            // Set up child - it does not implement pattern (loose mocks return 
            // default value of return types for calls that aren't set up, e.g. 
            // PatternIsSupported will be false
            var childMock = new Mock<IAutomationElement>();
            cellMock.MockChildren(childMock);
            var query = clsQuery.Parse("UIATableReadToggleState ColumnNumber=5 RowNumber=2");
            Assert.Throws<PatternNotFoundException<ITogglePattern>>(() => Execute(query));
        }

        [Test]
        public void Execute_InvalidCell_ShouldThrow()
        {
            var query = clsQuery.Parse("UIATableReadToggleState ColumnNumber=5 RowNumber=2");
            ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            Assert.Throws<NoSuchElementException>(() => Execute(query));
        }
    }
}
