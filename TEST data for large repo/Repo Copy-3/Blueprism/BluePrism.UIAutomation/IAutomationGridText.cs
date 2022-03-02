namespace BluePrism.UIAutomation
{
    /// <summary>
    /// Provides help in interacting with components in a cell element depending on the
    /// patterns implemented by the Grid
    /// </summary>
    public interface IAutomationGridText
    {
        /// <summary>
        /// Retrieves text from the cell component, depending upon supporting patterns.
        /// </summary>
        /// <param name="cellElement">The cell component that will be read from. </param>
        /// <returns>A string from the cell component. </returns>
        string GetTextFromElement(IAutomationElement cellComponent);
    }
}
