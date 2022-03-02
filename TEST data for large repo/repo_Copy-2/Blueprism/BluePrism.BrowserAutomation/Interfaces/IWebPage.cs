namespace BluePrism.BrowserAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using BluePrism.BrowserAutomation.WebMessages;
    using Data;
    using Events;

    /// <summary>
    /// Provides methods for interacting with a web page
    /// </summary>
    public interface IWebPage
    {
        void ReceiveMessage(WebMessageWrapper message);
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the element by path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The element identified by the path; or <c>null</c> if no element can be found.</returns>
        IWebElement GetElementByPath(string path);

        /// <summary>
        /// Gets the element by css selector.
        /// </summary>
        /// <param name="selector">The selector.</param>
        /// <returns>The element identified by the selector; or <c>null</c> if no element can be found.</returns>
        IWebElement GetElementByCssSelector(string selector);

        /// <summary>
        /// Gets a collection of elements with the given type.
        /// </summary>
        /// <param name="elementType">Type of the element.</param>
        /// <returns>A collection of elements.</returns>
        IReadOnlyCollection<IWebElement> GetElementsByType(string elementType);

        /// <summary>
        /// Gets a collection of elements with the given class.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>A collection of elements.</returns>
        IReadOnlyCollection<IWebElement> GetElementsByClass(string className);

        /// <summary>
        /// Gets a collection of elements with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// A collection of elements.
        /// </returns>
        IReadOnlyCollection<IWebElement> GetElementsByName(string name);

        /// <summary>
        /// Gets an element by identifier.
        /// </summary>
        /// <param name="elementId">The element identifier.</param>
        /// <returns>A web element</returns>
        IWebElement GetElementById(string elementId);

        /// <summary>
        /// Gets the element's descendants.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A collection of elements representing the element's descendants.</returns>
        IReadOnlyCollection<IWebElement> GetElementDescendants(IWebElement element);

        /// <summary>
        /// Gets the root element of the page.
        /// </summary>
        /// <returns>The root element.</returns>
        IWebElement GetRootElement();

        /// <summary>
        /// Gets the element bounds.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The bounds of the element relative to the document.</returns>
        Rectangle GetElementBounds(IWebElement element);

        /// <summary>
        /// Gets the cursor position relative to the document.
        /// </summary>
        /// <returns>The cursor position.</returns>
        Point GetCursorPosition();

        /// <summary>
        /// Gets the element identifier.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element's identifier</returns>
        string GetElementId(IWebElement element);

        /// <summary>
        /// Gets the currently selected text.
        /// </summary>
        /// <param name="element">The element. </param>
        /// <returns>The selected text string. </returns>
        string GetSelectedText(IWebElement element);

        /// <summary>
        /// Selects text on the element starting from the startIndex position
        /// to the length.
        /// </summary>
        /// <param name="element">The element. </param>
        /// <param name="startIndex">The start index position. </param>
        /// <param name="length">The length of the selection. </param>
        void SelectTextRange(IWebElement element, int startIndex, int length);

        /// <summary>
        /// Gets the text from the cell within the table at the selected
        /// row and column numbers.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="rowNumber">the row number in the table.</param>
        /// <param name="columnNumber">the column number in the table. </param>
        /// <returns>The text from the cell.</returns>
        string GetTableItemText(IWebElement element, int rowNumber, int columnNumber);

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element name</returns>
        string GetElementName(IWebElement element);

        /// <summary>
        /// Gets the elements link address text.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The link address text. </returns>
        string GetLinkAddressText(IWebElement element);

        /// <summary>
        /// Gets the type of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element type</returns>
        string GetElementType(IWebElement element);

        /// <summary>
        /// Gets the element class.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element's class</returns>
        string GetElementClass(IWebElement element);

        /// <summary>
        /// Gets the path to the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A path expressed as XPath</returns>
        string GetElementPath(IWebElement element);

        /// <summary>
        /// Gets the element value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The value of the element</returns>
        string GetElementValue(IWebElement element);

        /// <summary>
        /// Gets the element text.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The text of the element</returns>
        string GetElementText(IWebElement element);

        /// <summary>
        /// Gets the element HTML.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The HTML of the element</returns>
        string GetElementHtml(IWebElement element);

        /// <summary>
        /// Sets the element value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The value.</param>
        void SetElementValue(IWebElement element, string value);

        /// <summary>
        /// Sets the checked state of the element.
        /// </summary>
        /// <param name="element">The element. </param>
        /// <param name="value">The state to set the element to. </param>
        void SetCheckedState(IWebElement element, bool value);

        /// <summary>
        /// Clicks the element.
        /// </summary>
        /// <param name="element">The element.</param>
        void ClickElement(IWebElement element);

        /// <summary>
        /// Double clicks the element.
        /// </summary>
        /// <param name="element">The element.</param>
        void DoubleClickElement(IWebElement element);

        /// <summary>
        /// Focuses the element.
        /// </summary>
        /// <param name="element">The element.</param>
        void FocusElement(IWebElement element);

        /// <summary>
        /// Hovers over an element.
        /// </summary>
        /// <param name="element">The element.</param>
        void HoverElement(IWebElement element);

        /// <summary>
        /// Navigates to the given web address.
        /// </summary>
        /// <param name="address">The address.</param>
        void NavigateTo(string address);

        /// <summary>
        /// Gets the address of the page.
        /// </summary>
        /// <returns>The current address of the page</returns>
        string GetAddress();

        /// <summary>
        /// Gets the HTML source of the page for use with Application Navigator
        /// </summary>
        /// <returns></returns>
        string GetHTMLSource();

        /// <summary>
        /// Sends a message to close the page
        /// </summary>
        /// <returns></returns>

        void CloseWebPage();
        
        /// <summary>
        /// Highlights the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="color">The color of the highlight border.</param>
        void HighlightElement(IWebElement element, Color color);

        /// <summary>
        /// Removes any highlighting from the page.
        /// </summary>
        void RemoveHighlighting();
        
        /// <summary>
        /// Occurs when the mouse cursor moves over an element.
        /// </summary>
        event ElementHoverDelegate ElementHover;

        /// <summary>
        /// Gets the element client bounds.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns>A rectangle representing the client bounds of the element</returns>
        Rectangle GetElementClientBounds(IWebElement element);

        /// <summary>
        /// Gets the element offset bounds.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns>A rectangle representing the offset bounds of the element</returns>
        Rectangle GetElementOffsetBounds(IWebElement element);

        /// <summary>
        /// Gets the element scroll bounds.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns>A rectangle representing the scroll bounds of the element</returns>
        Rectangle GetElementScrollBounds(IWebElement element);

        /// <summary>
        /// Gets the child count of the element.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns>The number of direct children of the element</returns>
        int GetElementChildCount(IWebElement element);

        /// <summary>
        /// Gets the current column count within a table element.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns>The number of columns within the table.</returns>
        int GetColumnCount(IWebElement element, int rowIndex);

        /// <summary>
        /// Gets the current column count within a table element.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns>The number of columns within the table.</returns>
        int GetRowCount(IWebElement element);

        /// <summary>
        /// Gets whether the element is editable
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns><c>true</c> if the element is editable; otherwise, <c>false</c></returns>
        bool GetElementIsEditable(IWebElement element);

        /// <summary>
        /// Gets whether the element is checked.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns><c>true</c> if the element is checked; otherwise, <c>false</c></returns>
        bool GetCheckedState(IWebElement element);

        /// <summary>
        /// Gets the style of the element.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns>The style of the element</returns>
        string GetElementStyle(IWebElement element);

        /// <summary>
        /// Gets the tab index.
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <returns>The tab index of the element.</returns>
        int GetElementTabIndex(IWebElement element);

        /// <summary>
        /// Gets the value of the requested attribute
        /// </summary>
        /// <param name="element">The web element.</param>
        /// <param name="name">The attribute value to get</param>
        /// <returns>The value of the attribute or <c>null</c> if the attribute can't be found</returns>
        string GetElementAttribute(IWebElement element, string name);

        /// <summary>
        /// Selects the element and all of its descendants
        /// </summary>
        /// <param name="element">The element.</param>
        void SelectElement(IWebElement element);

        /// <summary>
        /// Sets the value of the given attribute
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="attribute">The name of the attribute to set</param>
        /// <param name="value">The value to set the attribute to</param>
        void SetAttribute(IWebElement element, string attribute, string value);

        /// <summary>
        /// Scrolls the page to the element
        /// </summary>
        /// <param name="element">The element.</param>
        void ScrollToElement(IWebElement element);

        /// <summary>
        /// Submits the Form element
        /// </summary>
        /// <param name="element">The Form element to submit</param>
        void SubmitElement(IWebElement element);

        /// <summary>
        /// Gets whether the element is visible.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns><c>true</c> if the element is visible; otherwise, <c>false</c></returns>
        bool GetElementIsVisible(IWebElement element);

        /// Updates the cookie data.
        /// </summary>
        /// <param name="cookie">The cookie data to apply.</param>
        void UpdateCookie(string cookie);

        /// <summary>
        /// Gets whether the element is on screen.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns><c>true</c> if the element is on screen; otherwise, <c>false</c></returns>
        bool GetElementIsOnScreen(IWebElement element);

        void Launch(BrowserType browserType, string urls, string trackingId);
        void Attach(BrowserType browserType, string windowTitle, string trackingId);
        void Detach(string trackingId);

        /// <summary>
        /// Gets all items in a list element.
        /// </summary>
        /// <param name="element">The element identifier.</param>
        /// <returns>A collection of strings containing the items in the list</returns>
        IReadOnlyCollection<ListItem> GetListItems(IWebElement element);

        /// <summary>
        /// Selects the list item.
        /// </summary>
        /// <param name="element">The element identifier.</param>
        /// <param name="index">The index of the item to select.</param>
        /// <param name="name">The name of the item to select.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        void SelectListItem(IWebElement element, int index, string name);

        /// <summary>
        /// Adds to list selection.
        /// </summary>
        /// <param name="element">The element identifier.</param>
        /// <param name="index">The index.</param>
        /// <param name="name">The name.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        void AddToListSelection(IWebElement element, int index, string name);

        /// <summary>
        /// Removes from list selection.
        /// </summary>
        /// <param name="element">The element identifier.</param>
        /// <param name="index">The index.</param>
        /// <param name="name">The name.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        void RemoveFromListSelection(IWebElement element, int index, string name);

        /// <summary>
        /// Injects the given javascript.
        /// </summary>
        /// <param name="code">The code to inject.</param>
        void InjectJavascript(string code);

        /// <summary>
        /// Invokes the given javascript function.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameters">The function parameters.</param>
        void InvokeJavascript(string functionName, string parameters);

        /// <summary>
        /// Gets the label associated with the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The text of the label associated with the element.</returns>
        string GetElementLabel(IWebElement element);

        /// <summary>
        /// Gets the element access key.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A string representing the access key</returns>
        string GetElementAccessKey(IWebElement element);

        /// <summary>
        /// Gets the range of a slider element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The minimum and maximum values of the slider.</returns>
        SliderRange GetSliderRange(IWebElement element);

        /// <summary>
        /// Is the list item selected in this list element
        /// </summary>
        /// <param name="element">The list item element.</param>
        /// <returns><c>true</c> if the list item is selected; otherwise, <c>false</c></returns>
        bool GetIsListItemSelected(IWebElement element);

        /// <summary>
        /// Checks if the parent document of the element has finished loading
        /// </summary>
        /// <param name="element"></param>
        /// <returns><c>true</c> if the parent document has loaded</returns>
        bool CheckParentDocumentLoaded(IWebElement element);

        /// <summary>
        /// Returns a rectangle.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>
        /// A rectangle representing the element location and bounds on the screen. 
        /// </returns>
        Rectangle GetElementScreenLocationAndBounds(IWebElement element);
    }
}
