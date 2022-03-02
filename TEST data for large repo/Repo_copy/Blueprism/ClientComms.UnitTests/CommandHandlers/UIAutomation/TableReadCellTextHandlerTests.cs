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
    internal class TableReadCellTextHandlerTests : UIAutomationHandlerTestBase<TableReadCellTextHandler>
    {
        [Test]
        public void Execute_GridElementWithItem_ShouldReturnText()
        {
            var query = clsQuery.Parse("UIATableReadCellText ColumnNumber=5 RowNumber=2");

            // Mock grid pattern implemented by root element
            var gridPatternMock = ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);

            // Mock item element object 
            var itemMock = GridSetupHelper.SetupCell(gridPatternMock, 4, 1);

            // Mock IAutomationGridText object that is used to read the text
            var textProviderMock = new Mock<IAutomationGridText>();
            var factoryMock = GetMock<IAutomationFactory>();
            factoryMock.Setup<IAutomationGridText>(f => f.GetGridTextProvider()).Returns(textProviderMock.Object);
            textProviderMock.Setup(tp => tp.GetTextFromElement(itemMock.Object)).Returns("test text");
            var reply = Execute(query);
            Assert.That(reply.Message, Is.EqualTo("test text"));
        }

        [Test]
        public void Execute_InvalidCell_ShouldThrow()
        {
            var query = clsQuery.Parse("UIATableReadCellText ColumnNumber=5 RowNumber=2");
            ElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            Assert.Throws<NoSuchElementException>(() => Execute(query));
        }
    }
}
