namespace BluePrism.BrowserAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Data;

    /// <summary>
    /// Provides methods for interacting with a web element.
    /// </summary>
    public interface IWebElement
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the page which contains this element.
        /// </summary>
        IWebPage Page { get; }

        /// <summary>
        /// Fires the click event on the element.
        /// </summary>
        void Click();

        /// <summary>
        /// Fires a double click on the element.
        /// </summary>
        void DoubleClick();

        /// <summary>
        /// Highlights with the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        void Highlight(Color color);

        /// <summary>
        /// Gets the element bounds relative to the document.
        /// </summary>
        /// <returns>The bounds of the element relative to the document</returns>
        Rectangle GetBounds();

        /// <summary>
        /// Gets the element identifier.
        /// </summary>
        /// <returns>The element's identifier</returns>
        string GetElementId();

        /// <summary>
        /// Gets the descendants of this element.
        /// </summary>
        /// <returns>A collection representing the descendants.</returns>
        IReadOnlyCollection<IWebElement> GetDescendants();

        /// <summary>
        /// Gets the text from the selected cell within the table element.
        /// </summary>
        /// <param name="rowNumber">The row index from the table.</param>
        /// <param name="columnNumber">The row index from the table.</param>
        /// <returns>The text within the selected table cell. </returns>
        string GetTableItemText(int rowNumber, int columnNumber);

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <returns>The name of the element.</returns>
        string GetName();

        /// <summary>
        /// Gets the currently selected text.
        /// </summary>
        /// <returns>The selected text string. </returns>
        string GetSelectedText();

        /// <summary>
        /// Selects text on the element starting from the startIndex position
        /// to the length.
        /// </summary>
        /// <param name="startIndex">The start index position. </param>
        /// <param name="length">The length of the selection. </param>
        void SelectTextRange(int startIndex, int length);

        /// <summary>
        /// Gets the links address text.
        /// </summary>
        /// <returns>The link text. </returns>
        string GetLinkAddressText();

        /// <summary>
        /// Gets the type of the element.
        /// </summary>
        /// <returns>The type of the element.</returns>
        string GetElementType();

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <returns>The class of the element.</returns>
        string GetClass();

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns>A path expressed as XPath</returns>
        string GetPath();

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns>The element's value</returns>
        string GetValue();

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <returns>The element's text</returns>
        string GetText();

        /// <summary>
        /// Gets the HTML.
        /// </summary>
        /// <returns>The element's HTML</returns>
        string GetHtml();

        /// <summary>
        /// Sets the value of the element.
        /// </summary>
        /// <param name="value">The value.</param>
        void SetValue(string value);

        /// <summary>
        /// Sets the checked state of the element.
        /// </summary>
        /// <param name="value">The value to set state of the element.</param>
        void SetCheckedState(bool value);

        /// <summary>
        /// Focuses the element.
        /// </summary>
        void Focus();

        /// <summary>
        /// Hovers over an element.
        /// </summary>
        void Hover();

        /// <summary>
        /// Hovers the mouse over an element.
        /// </summary>
        void HoverMouseOverElement();

        /// <summary>
        /// Gets the client bounds.
        /// </summary>
        /// <returns>A rectangle representing the client bounds of the element</returns>
        Rectangle GetClientBounds();

        /// <summary>
        /// Gets the offset bounds.
        /// </summary>
        /// <returns>A rectangle representing the offset bounds of the element</returns>
        Rectangle GetOffsetBounds();

        /// <summary>
        /// Gets the scroll bounds.
        /// </summary>
        /// <returns>A rectangle representing the scroll bounds of the element</returns>
        Rectangle GetScrollBounds();

        /// <summary>
        /// Gets the child count.
        /// </summary>
        /// <returns>The number of direct children of the element</returns>
        int GetChildCount();

        /// <summary>
        /// Gets the current count of the columns within a table.
        /// </summary>
        /// <returns>The column count. </returns>
        int GetColumnCount(int rowIndex);

        /// <summary>
        /// Gets the current count of the rows within a table.
        /// </summary>
        /// <returns>The row count. </returns>
        int GetRowCount();

        /// <summary>
        /// Gets whether the element is editable
        /// </summary>
        /// <returns><c>true</c> if the element is editable; otherwise, <c>false</c></returns>
        bool GetIsEditable();

        /// <summary>
        /// Gets whether the element is Checked
        /// </summary>
        /// <returns><c>true</c> if the element is checked; otherwise, <c>false</c></returns>
        bool GetCheckedState();

        /// <summary>
        /// Gets the style.
        /// </summary>
        /// <returns>The style of the element</returns>
        string GetStyle();

        /// <summary>
        /// Gets the tab index.
        /// </summary>
        /// <returns>The tab index of the element.</returns>
        int GetTabIndex();

        /// <summary>
        /// Gets the value of the requested attribute
        /// </summary>
        /// <param name="name">The attribute value to get</param>
        /// <returns>The value of the attribute or <c>null</c> if the attribute can't be found</returns>
        string GetAttribute(string name);

        /// <summary>
        /// Selects the element and all of its descendants
        /// </summary>
        void Select();

        /// <summary>
        /// Sets the value of the given attribute
        /// </summary>
        /// <param name="attribute">The name of the attribute to set</param>
        /// <param name="value">The value to set the attribute to</param>
        void SetAttribute(string attribute, string value);

        /// <summary>
        /// Scrolls the page to the element
        /// </summary>
        void ScrollTo();

        /// <summary>
        /// Submits a Form element
        /// </summary>
        void Submit();

        /// <summary>
        /// Gets the label associated with the element.
        /// </summary>
        /// <returns>The text of the label associated with the element.</returns>
        string GetLabel();

        /// <summary>
        /// Gets whether the element is visible.
        /// </summary>
        /// <returns><c>true</c> if the element is visible; otherwise, <c>false</c></returns>
        bool GetIsVisible();

        /// <summary>
        /// Gets whether the element is on screen.
        /// </summary>
        /// <returns><c>true</c> if the element is on screen; otherwise, <c>false</c></returns>
        bool GetIsOnScreen();

        /// <summary>
        /// Gets all items in a list element.
        /// </summary>
        /// <returns>A collection of strings containing the items in the list</returns>
        IReadOnlyCollection<ListItem> GetListItems();

        /// <summary>
        /// Gets the element access key.
        /// </summary>
        /// <returns>A string representing the access key</returns>
        string GetAccessKey();

        /// <summary>
        /// Gets the range of a slider element.
        /// </summary>
        /// <returns>The minimum and maximum values of the slider.</returns>
        SliderRange GetSliderRange();
        /// <summary>
        /// Selects the list item.
        /// </summary>
        /// <param name="index">The index of the item to select.</param>
        /// <param name="name">The name of the item to select.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        void SelectListItem(int index, string name);

        /// <summary>
        /// Adds to list selection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="name">The name.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        void AddToListSelection(int index, string name);

        /// <summary>
        /// Removes from list selection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="name">The name.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        void RemoveFromListSelection(int index, string name);

        /// <summary>
        /// Is the list item selected
        /// </summary>
        /// <returns><c>true</c> if the list item is selected; otherwise, <c>false</c></returns>
        bool GetIsListItemSelected();

        /// <summary>
        /// Checks if the parent document of the element has finished loading
        /// </summary>
        /// <returns></returns>
        bool CheckParentDocumentLoaded();

        /// <summary>
        /// Returns a rectangle.
        /// </summary>
        /// <returns>
        /// A rectangle representing the element location and bounds on the screen. 
        /// </returns>
        Rectangle GetElementScreenLocationAndBounds();

    }
}
