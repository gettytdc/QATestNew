using BluePrism.ApplicationManager;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation;
using BluePrism.ApplicationManager.CommandHandlers.UIAutomation.Shared;
using BluePrism.Server.Domain.Models;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared
{
    [TestFixture]
    public class GridHelperTests
    {
        [SetUp]
        public void Setup()
        {

            // Set up a mock element that supports the grid pattern
            GridMock = new Mock<IAutomationElement>();
            GridPatternMock = GridMock.MockPattern<IGridPattern>();
            // Return a mock element for cell at row 5, column 2
            CellMockRow2Column5 = GridSetupHelper.SetupCell(GridPatternMock, 4, 1);
            // Set up the cell to have 2 child elements
            CellChildMock1 = new Mock<IAutomationElement>();
            CellChildMock2 = new Mock<IAutomationElement>();
            CellMockRow2Column5.MockChildren(CellChildMock1, CellChildMock2);
        }

        /// <summary>
        /// Shared mock IAutomationElement object that is set up to support
        /// the IGridPattern using the GridPatternMock property
        /// </summary>
        private Mock<IAutomationElement> GridMock { get; set; }

        /// <summary>
        /// Mock IGridPattern object of the shared mock GridElement object
        /// </summary>
        private Mock<IGridPattern> GridPatternMock { get; set; }

        /// <summary>
        /// Shared mock IAutomationElement object that GridMock is set up
        /// to return at row 5 column 2 (indexes row 0, 1)
        /// </summary>
        private Mock<IAutomationElement> CellMockRow2Column5 { get; set; }

        /// <summary>
        /// Shared mock IAutomationElement object - child 1 of CellRow5Col2Mock
        /// </summary>
        private Mock<IAutomationElement> CellChildMock1 { get; set; }

        /// <summary>
        /// Shared mock IAutomationElement object - child 2 of CellRow5Col2Mock
        /// </summary>
        private Mock<IAutomationElement> CellChildMock2 { get; set; }

        [Test]
        public void GetGridCoordinates_WithValidQuery_ReturnsCoordinates()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2");
            var coordinates = GridHelper.GetGridCoordinates(query);
            Assert.That(coordinates, Is.EqualTo(new GridCoordinates(4, 1)));
        }

        [Test]
        public void GetCell_WithValidCell_ReturnsCellAtPosition()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2");
            var cell = GridHelper.GetCell(GridMock.Object, query);
            Assert.That(cell, Is.EqualTo(CellMockRow2Column5.Object));
        }

        [Test]
        public void GetCell_WithUnrecognisedCell_ReturnsNull()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=8 RowNumber=3");
            var cell = GridHelper.GetCell(GridMock.Object, query);
            Assert.That(cell, Is.Null);
        }

        [Test]
        public void EnsureCell_WithUnrecognisedCell_ShouldThrow()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=8 RowNumber=3");
            var exception = Assert.Throws<NoSuchElementException>(() => GridHelper.EnsureCell(GridMock.Object, query));
            Assert.That(exception.Message, Is.EqualTo(UIAutomationErrorResources.NoSuchElementException_TableCellNotFoundMessage));
        }

        [Test]
        public void EnsureCellElement_WithoutElementNumber_ReturnsCellAtPosition()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2 ElementNumber=0");
            var cell = GridHelper.EnsureCellElement(GridMock.Object, query);
            Assert.That(cell, Is.EqualTo(CellMockRow2Column5.Object));
        }

        [Test]
        public void EnsureCellElement_WithElementNumber_ReturnsChildElement()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2 ElementNumber=2");
            var cellElement = GridHelper.EnsureCellElement(GridMock.Object, query);
            Assert.That(cellElement, Is.EqualTo(CellChildMock2.Object));
        }

        [Test]
        public void EnsureCellElement_WithInvalidElementNumber_ShouldThrow()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2 ElementNumber=5");
            var exception = Assert.Throws<NoSuchElementException>(() => GridHelper.EnsureCellElement(GridMock.Object, query));
            Assert.That(exception.Message, Is.EqualTo(UIAutomationErrorResources.NoSuchChildFoundException_CellChildNotFound));
        }

        [Test]
        public void EnsureCellElementPattern_WithoutElementNumberAndCellSupportingPattern_ReturnsPatternFromCell()
        {
            var valuePatternMock = CellMockRow2Column5.MockPattern<IValuePattern>(PatternType.ValuePattern);
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2");
            var pattern = GridHelper.EnsureCellElementPattern<IValuePattern>(GridMock.Object, query, PatternType.ValuePattern);
            Assert.That(pattern, Is.EqualTo(valuePatternMock.Object));
        }

        [Test]
        public void EnsureCellElementPattern_WithoutElementNumberAndChildSupportingPattern_ReturnsPatternFromChild()
        {
            var valuePatternMock = CellChildMock2.MockPattern<IValuePattern>(PatternType.ValuePattern);
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2");
            var pattern = GridHelper.EnsureCellElementPattern<IValuePattern>(GridMock.Object, query, PatternType.ValuePattern);
            Assert.That(pattern, Is.EqualTo(valuePatternMock.Object));
        }

        [Test]
        public void EnsureCellElementPattern_WithElementNumberOfDescendantSupportingPattern_ReturnsDescendant()
        {
            var valuePatternMock = CellChildMock2.MockPattern<IValuePattern>(PatternType.ValuePattern);
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2 ElementNumber=2");
            var pattern = GridHelper.EnsureCellElementPattern<IValuePattern>(GridMock.Object, query, PatternType.ValuePattern);
            Assert.That(pattern, Is.EqualTo(valuePatternMock.Object));
        }

        [Test]
        public void EnsureCellElementPattern_WithElementNumberOfDescendantNotSupportingPattern_ShouldThrow()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2 ElementNumber=2");
            Assert.Throws<PatternNotFoundException<IValuePattern>>(() => GridHelper.EnsureCellElementPattern<IValuePattern>(GridMock.Object, query, PatternType.ValuePattern));
        }

        [Test]
        public void EnsureCellElementPattern_WithInvalidElementNumber_ShouldThrow()
        {
            var query = clsQuery.Parse($"UIATest ColumnNumber=5 RowNumber=2 ElementNumber=5");
            var exception = Assert.Throws<NoSuchElementException>(() => GridHelper.EnsureCellElementPattern<IValuePattern>(GridMock.Object, query, PatternType.ValuePattern));
            Assert.That(exception.Message, Is.EqualTo(UIAutomationErrorResources.NoSuchChildFoundException_CellChildNotFound));
        }
    }
}
