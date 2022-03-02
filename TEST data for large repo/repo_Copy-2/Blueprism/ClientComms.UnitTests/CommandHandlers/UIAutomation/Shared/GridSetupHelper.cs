using System.Collections.Generic;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using Moq;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared
{

    /// <summary>
    /// Contains shared setup functionality for unit testing grid elements
    /// </summary>
    public static class GridSetupHelper
    {

        /// <summary>
        /// Sets up a mock IGridPattern's GetItem method to return a mock
        /// IAutomationElement object at the specified column and row.
        /// </summary>
        /// <param name="patternMock">The mock IGridPattern object</param>
        /// <param name="columnIndex">The column index for the item</param>
        /// <param name="rowIndex">The row index for the item</param>
        /// <returns>The mock IAutomationElement object that will be returned by
        /// GetItem</returns>
        public static Mock<IAutomationElement> SetupCell(Mock<IGridPattern> patternMock, int columnIndex, int rowIndex)
        {

            // Set up the item that will be found at specified coordinates
            var itemMock = new Mock<IAutomationElement>();

            // Set up element to return item element at specified coordinates
            patternMock.Setup(p => p.GetItem(rowIndex, columnIndex)).Returns(itemMock.Object);
            return itemMock;
        }

        /// <summary>
        /// Sets up a mock IGridPattern's GetItem method to return a mock
        /// IAutomationElement objects for the specified number of columns
        /// for a given row
        /// </summary>
        /// <param name="patternMock">The mock IGridPattern object</param>
        /// <param name="columnCount">The number of columns to support</param>
        /// <param name="rowIndex">The row index for the item</param>
        /// <returns>A sequence of mock IAutomationElement objects that will be
        /// returned by GetItem</returns>
        public static List<Mock<IAutomationElement>> SetUpRow(Mock<IGridPattern> patternMock, int columnCount, int rowIndex)
        {
            var itemMocks = new List<Mock<IAutomationElement>>();
            for (int columnIndex = 0, loopTo = columnCount - 1; columnIndex <= loopTo; columnIndex++)
            {
                // Set up the item that will be found at specified coordinates
                var itemMock = SetupCell(patternMock, columnIndex, rowIndex);
                itemMocks.Add(itemMock);
            }

            return itemMocks;
        }
    }
}