using System;
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
    internal class TableSetCellTextHandlerTests : UIAutomationHandlerTestBase<TableSetCellTextHandler>
    {
        [Test]
        public void Execute_ItemWithCell_ShouldSetSpecifiedText()
        {
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var valuePatternMock = cellMock.MockPattern<IValuePattern>(PatternType.ValuePattern);
            cellMock.MockChildren(cellMock);
            var query = clsQuery.Parse("UIATableSetCellText ColumnNumber=5 RowNumber=2 NewText=\"test new text\"");
            var reply = Execute(query);
            valuePatternMock.Verify(vp => vp.SetValue("test new text"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
        }

        [Test]
        public void Execute_ItemWithCellChildElement_ShouldSetSpecifiedText()
        {
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var child1 = new Mock<IAutomationElement>();
            var child2 = new Mock<IAutomationElement>();
            var valuePatternMock = child2.MockPattern<IValuePattern>(PatternType.ValuePattern);
            cellMock.MockChildren(child1, child2);
            var query = clsQuery.Parse("UIATableSetCellText ColumnNumber=5 RowNumber=2 NewText=\"test new text\" ElementNumber=2");
            var reply = Execute(query);
            valuePatternMock.Verify(vp => vp.SetValue("test new text"));
            Assert.That(reply, Is.EqualTo(Reply.Ok));
        }

        [Test]
        public void Execute_InvalidCell_ShouldThrow()
        {
            var query = clsQuery.Parse("UIATableSetCellText ColumnNumber=5 RowNumber=2 NewText=\"test new text\"");
            ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            Assert.Throws<NoSuchElementException>(() => Execute(query));
        }

        [Test]
        public void Execute_DefaultInnerElement_ShouldThrow()
        {
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            var cellMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);
            var child1 = new Mock<IAutomationElement>();
            var child2 = new Mock<IAutomationElement>();
            var valuePatternMock = child2.MockPattern<IValuePattern>(PatternType.ValuePattern);
            valuePatternMock.Setup(e => e.SetValue(It.IsAny<string>())).Callback(() => { throw new InvalidOperationException(); });
            cellMock.MockChildren(child1, child2);
            var query = clsQuery.Parse("UIATableSetCellText ColumnNumber=5 RowNumber=2 NewText=\"test new text\" ElementNumber=0");
            Assert.Throws<OperationFailedException>(() => Execute(query));
        }
    }
}
