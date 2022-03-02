using System;

namespace BluePrism.UIAutomation
{
    /// <summary>
    /// Provides helper methods for interacting with UIA objects
    /// </summary>
    public interface IAutomationHelper
    {
         /// <summary>
        /// Gets the handle of the window which contains the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A window handle</returns>
        IntPtr GetWindowHandle(IAutomationElement element);
    }
}