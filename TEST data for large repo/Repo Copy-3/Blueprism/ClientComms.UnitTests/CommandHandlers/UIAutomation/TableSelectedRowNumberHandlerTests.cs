using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation
{
    internal class TableSelectedRowNumberHandlerTests : UIAutomationHandlerTestBase<TableSelectedRowNumberHandler>
    {
        [Test]
        public void Execute_WithSelectedItem_ShouldReturnCurrentRow()
        {

            // Set up a selected row with GridItemPattern
            var rowMock1 = new Mock<IAutomationElement>();
            var rowMock2 = new Mock<IAutomationElement>();
            ElementMock.MockChildren(rowMock1, rowMock2);
            rowMock1.MockPattern<ISelectionItemPattern>();
            rowMock2.MockPattern<ISelectionItemPattern>().Setup(p => p.CurrentIsSelected).Returns(true);
            var cellMock = new Mock<IAutomationElement>();
            rowMock2.MockChildren(cellMock);
            cellMock.MockPattern<IGridItemPattern>(PatternType.GridItemPattern).Setup(p => p.CurrentRow).Returns(9999);
            var reply = Execute(clsQuery.Parse("uiatablerowindex"));
            Assert.That(reply.Message, Is.EqualTo("10000"));
        }

        [Test]
        public void Execute_WithNoSelection_ShouldReturnEmptySelection()
        {

            // No selected cells
            ElementMock.MockChildren();
            var reply = Execute(clsQuery.Parse("uiatablerowindex"));
            Assert.That(reply.Message, Is.EqualTo("-1"));
        }
    }
}