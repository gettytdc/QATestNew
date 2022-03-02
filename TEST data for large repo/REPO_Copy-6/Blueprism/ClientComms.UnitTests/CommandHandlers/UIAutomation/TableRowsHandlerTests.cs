using System.Xml.Linq;
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
    internal class TableRowsHandlerTests : UIAutomationHandlerTestBase<TableRowsHandler>
    {
        [Test]
        public void Execute_RequestingSingleRow_ShouldReturnRow()
        {
            var query = clsQuery.Parse("UIATableRows FirstRowNumber=4 LastRowNumber=4");
            SetUpRowItems(ElementMock, columnCount: 4, rowCount: 4);
            var reply = Execute(query);
            string expectedXml = XElement.Parse("<collection><row>\r\n                                      <field type=\"text\" value=\"row 4, column 1 text\" name=\"Column1\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 2 text\" name=\"Column2\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 3 text\" name=\"Column3\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 4 text\" name=\"Column4\"/>\r\n                                  </row></collection>").ToString(SaveOptions.DisableFormatting);
            Assert.That(reply.Message, Is.EqualTo(expectedXml));
        }

        [Test]
        public void Execute_RequestingRangeOfRows_ShouldReturnRows()
        {
            var query = clsQuery.Parse("UIATableRows FirstRowNumber=2 LastRowNumber=3");
            SetUpRowItems(ElementMock, columnCount: 4, rowCount: 4);
            var reply = Execute(query);
            string expectedXml = XElement.Parse("<collection><row>\r\n                                      <field type=\"text\" value=\"row 2, column 1 text\" name=\"Column1\"/>\r\n                                      <field type=\"text\" value=\"row 2, column 2 text\" name=\"Column2\"/>\r\n                                      <field type=\"text\" value=\"row 2, column 3 text\" name=\"Column3\"/>\r\n                                      <field type=\"text\" value=\"row 2, column 4 text\" name=\"Column4\"/>\r\n                                  </row><row>\r\n                                      <field type=\"text\" value=\"row 3, column 1 text\" name=\"Column1\"/>\r\n                                      <field type=\"text\" value=\"row 3, column 2 text\" name=\"Column2\"/>\r\n                                      <field type=\"text\" value=\"row 3, column 3 text\" name=\"Column3\"/>\r\n                                      <field type=\"text\" value=\"row 3, column 4 text\" name=\"Column4\"/>\r\n                                  </row></collection>").ToString(SaveOptions.DisableFormatting);
            Assert.That(reply.Message, Is.EqualTo(expectedXml));
        }

        [Test]
        public void Execute_WithoutRowsSpecified_ShouldReturnAllRows()
        {
            var query = clsQuery.Parse("UIATableRows");
            SetUpRowItems(ElementMock, columnCount: 4, rowCount: 4);
            var reply = Execute(query);
            string expectedXml = XElement.Parse("<collection><row>\r\n                                      <field type=\"text\" value=\"row 1, column 1 text\" name=\"Column1\"/>\r\n                                      <field type=\"text\" value=\"row 1, column 2 text\" name=\"Column2\"/>\r\n                                      <field type=\"text\" value=\"row 1, column 3 text\" name=\"Column3\"/>\r\n                                      <field type=\"text\" value=\"row 1, column 4 text\" name=\"Column4\"/>\r\n                                  </row><row>\r\n                                      <field type=\"text\" value=\"row 2, column 1 text\" name=\"Column1\"/>\r\n                                      <field type=\"text\" value=\"row 2, column 2 text\" name=\"Column2\"/>\r\n                                      <field type=\"text\" value=\"row 2, column 3 text\" name=\"Column3\"/>\r\n                                      <field type=\"text\" value=\"row 2, column 4 text\" name=\"Column4\"/>\r\n                                  </row><row>\r\n                                      <field type=\"text\" value=\"row 3, column 1 text\" name=\"Column1\"/>\r\n                                      <field type=\"text\" value=\"row 3, column 2 text\" name=\"Column2\"/>\r\n                                      <field type=\"text\" value=\"row 3, column 3 text\" name=\"Column3\"/>\r\n                                      <field type=\"text\" value=\"row 3, column 4 text\" name=\"Column4\"/>\r\n                                  </row><row>\r\n                                      <field type=\"text\" value=\"row 4, column 1 text\" name=\"Column1\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 2 text\" name=\"Column2\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 3 text\" name=\"Column3\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 4 text\" name=\"Column4\"/>\r\n                                  </row></collection>").ToString(SaveOptions.DisableFormatting);
            Assert.That(reply.Message, Is.EqualTo(expectedXml));
        }

        [Test]
        public void Execute_FromChildElement_ShouldUseAncestorGridElement()
        {
            var query = clsQuery.Parse("UIATableRows FirstRowNumber=4 LastRowNumber=4");
            var parentGridMock = new Mock<IAutomationElement>();
            SetUpRowItems(parentGridMock, columnCount: 4, rowCount: 4);
            ElementMock.MockAncestors(parentGridMock);
            var reply = Execute(query);
            string expectedXml = XElement.Parse("<collection><row>\r\n                                      <field type=\"text\" value=\"row 4, column 1 text\" name=\"Column1\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 2 text\" name=\"Column2\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 3 text\" name=\"Column3\"/>\r\n                                      <field type=\"text\" value=\"row 4, column 4 text\" name=\"Column4\"/>\r\n                                  </row></collection>").ToString(SaveOptions.DisableFormatting);
            Assert.That(reply.Message, Is.EqualTo(expectedXml));
        }

        [Test]
        public void Execute_InvalidRow_ShouldThrow()
        {
            var query = clsQuery.Parse("UIATableRow FirstRowNumber=6 LastRowNumber=7");
            SetUpRowItems(ElementMock, columnCount: 4, rowCount: 4);
            Assert.Throws<NoSuchElementException>(() => Execute(query));
        }

        private void SetUpRowItems(Mock<IAutomationElement> gridElementMock, int columnCount, int rowCount)
        {

            // Mock grid pattern implemented by root element
            var gridPatternMock = gridElementMock.MockPattern<IGridPattern>(PatternType.GridPattern);
            gridPatternMock.Setup(p => p.CurrentColumnCount).Returns(columnCount);
            gridPatternMock.Setup(p => p.CurrentRowCount).Returns(rowCount);

            // Mock IAutomationGridText object that is used to read the text
            var textProviderMock = new Mock<IAutomationGridText>();
            var factoryMock = GetMock<IAutomationFactory>();
            factoryMock.Setup<IAutomationGridText>(f => f.GetGridTextProvider()).Returns(textProviderMock.Object);

            // Mock IAutomationElement objects for all columns in row
            for (int rowIndex = 0, loopTo = rowCount; rowIndex <= loopTo; rowIndex++)
            {
                int rowNumber = rowIndex + 1;
                var itemMocks = GridSetupHelper.SetUpRow(gridPatternMock, columnCount, rowIndex);
                for (int columnNumber = 1, loopTo1 = columnCount; columnNumber <= loopTo1; columnNumber++)
                {
                    int columnIndex = columnNumber - 1;
                    textProviderMock.Setup(tp => tp.GetTextFromElement(itemMocks[columnIndex].Object)).Returns($"row {rowNumber}, column {columnNumber} text");
                }
            }
        }
    }
}
