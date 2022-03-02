namespace BluePrism.BrowserAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Data;
    using BPCoreLib;

    /// <summary>
    /// Provides methods for interacting with a web element
    /// </summary>
    /// <seealso cref="BluePrism.BrowserAutomation.IWebElement" />
    /// <seealso cref="System.IEquatable{BluePrism.BrowserAutomation.IWebElement}" />
    public class WebElement : IWebElement, IEquatable<IWebElement>
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the page which contains this element.
        /// </summary>
        public IWebPage Page { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebElement"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="parentPage">The parent page.</param>
        public WebElement(Guid id, IWebPage parentPage)
        {
            Id = id;
            Page = parentPage;
        }

        /// <summary>
        /// Fires the click event on the element.
        /// </summary>
        public void Click() =>
            Page.ClickElement(this);

        /// </inheritdoc>
        public void DoubleClick() =>
            Page.DoubleClickElement(this);

        /// <summary>
        /// Focuses the element.
        /// </summary>
        public void Focus() =>
            Page.FocusElement(this);

        /// <summary>
        /// Hovers over an element.
        /// </summary>
        public void Hover() =>
            Page.HoverElement(this);
        /// <summary>
        /// Hovers the mouse over an element.
        /// </summary>
        public void HoverMouseOverElement()
        {
            var elementScreenBounds = GetElementScreenLocationAndBounds();
            var moveToPoint = new Point(elementScreenBounds.Left + (elementScreenBounds.Width / 2),
                elementScreenBounds.Top + (elementScreenBounds.Height / 2));
            Focus();
            modWin32.SetCursorPos(moveToPoint.X, moveToPoint.Y);
        }

        /// <summary>
        /// Gets the client bounds.
        /// </summary>
        /// <returns>
        /// A rectangle representing the client bounds of the element
        /// </returns>
        public Rectangle GetClientBounds() =>
            Page.GetElementClientBounds(this);

        /// <summary>
        /// Gets the offset bounds.
        /// </summary>
        /// <returns>
        /// A rectangle representing the offset bounds of the element
        /// </returns>
        public Rectangle GetOffsetBounds() =>
            Page.GetElementOffsetBounds(this);

        /// <summary>
        /// Gets the scroll bounds.
        /// </summary>
        /// <returns>
        /// A rectangle representing the scroll bounds of the element
        /// </returns>
        public Rectangle GetScrollBounds() =>
            Page.GetElementScrollBounds(this);

        /// <summary>
        /// Gets the child count.
        /// </summary>
        /// <returns>
        /// The number of direct children of the element
        /// </returns>
        public int GetChildCount() =>
            Page.GetElementChildCount(this);

        /// </inheritdoc>
        public int GetColumnCount(int rowIndex) =>
            Page.GetColumnCount(this, rowIndex);

        /// </inheritdoc>
        public int GetRowCount() =>
            Page.GetRowCount(this);

        /// <summary>
        /// Gets whether the element is editable
        /// </summary>
        /// <returns>
        ///   <c>true</c> if the element is editable; otherwise, <c>false</c>
        /// </returns>
        public bool GetIsEditable() =>
            Page.GetElementIsEditable(this);

        /// </inheritdoc>
        public bool GetCheckedState() =>
            Page.GetCheckedState(this);

        /// </inheritdoc>
        public string GetTableItemText(int rowNumber, int columnNumber) =>
            Page.GetTableItemText(this, rowNumber, columnNumber);

        /// <summary>
        /// Gets the style.
        /// </summary>
        /// <returns>
        /// The style of the element
        /// </returns>
        public string GetStyle() =>
            Page.GetElementStyle(this);

        /// </inheritdoc>
        public string GetSelectedText() =>
            Page.GetSelectedText(this);

        /// </inheritdoc>
        public void SelectTextRange(int startIndex, int length) =>
            Page.SelectTextRange(this, startIndex, length);

        /// <summary>
        /// Gets the tab index.
        /// </summary>
        /// <returns>
        /// The tab index of the element.
        /// </returns>
        public int GetTabIndex() =>
            Page.GetElementTabIndex(this);

        /// <summary>
        /// Highlights with the specified color.
        /// </summary>
        /// <param name="color">The color.</param>
        public void Highlight(Color color) =>
            Page.HighlightElement(this, color);

        /// <summary>
        /// Gets the element bounds relative to the document.
        /// </summary>
        /// <returns>
        /// The bounds of the element relative to the document
        /// </returns>
        public Rectangle GetBounds() =>
            Page.GetElementBounds(this);

       /// <summary>
        /// Gets the element identifier.
        /// </summary>
        /// <returns>
        /// The element's identifier
        /// </returns>
        public string GetElementId() =>
            Page.GetElementId(this);

        /// <summary>
        /// Gets the descendants of this element.
        /// </summary>
        /// <returns>
        /// A collection representing the descendants.
        /// </returns>
        public IReadOnlyCollection<IWebElement> GetDescendants() =>
            Page.GetElementDescendants(this);

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <returns>
        /// The name of the element.
        /// </returns>
        public string GetName() =>
            Page.GetElementName(this);

        /// </inheritdoc>
        public string GetLinkAddressText() =>
            Page.GetLinkAddressText(this);

        /// <summary>
        /// Gets the type of the element.
        /// </summary>
        /// <returns>
        /// The type of the element.
        /// </returns>
        public string GetElementType() =>
            Page.GetElementType(this);

        /// <summary>
        /// Gets the class.
        /// </summary>
        /// <returns>
        /// The class of the element.
        /// </returns>
        public string GetClass() =>
            Page.GetElementClass(this);

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns>
        /// A path expressed as XPath
        /// </returns>
        public string GetPath() =>
            Page.GetElementPath(this);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns>
        /// The element's value
        /// </returns>
        public string GetValue() =>
            Page.GetElementValue(this);

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <returns>
        /// The element's text
        /// </returns>
        public string GetText() =>
            Page.GetElementText(this);

        /// <summary>
        /// Gets the HTML.
        /// </summary>
        /// <returns>
        /// The element's HTML
        /// </returns>
        public string GetHtml() =>
            Page.GetElementHtml(this);

        /// <summary>
        /// Sets the value of the element.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue(string value) =>
            Page.SetElementValue(this, value);

        /// <summary>
        /// Sets the checked state of the element.
        /// </summary>
        /// <param name="value">The value to set state of the element.</param>
        public void SetCheckedState(bool value) =>
            Page.SetCheckedState(this, value);

        /// <summary>
        /// Gets the value of the requested attribute
        /// </summary>
        /// <param name="name">The attribute value to get</param>
        /// <returns>The value of the attribute or <c>null</c> if the attribute can't be found</returns>
        public string GetAttribute(string name) =>
            Page.GetElementAttribute(this, name);

        /// <summary>
        /// Sets the value of the given attribute
        /// </summary>
        /// <param name="attribute">The name of the attribute to set</param>
        /// <param name="value">The value to set the attribute to</param>
        public void SetAttribute(string attribute, string value) =>
            Page.SetAttribute(this, attribute, value);

        /// <summary>
        /// Selects the element and all of its descendants
        /// </summary>
        public void Select() =>
            Page.SelectElement(this);

        /// <summary>
        /// Scrolls the page to the element
        /// </summary>
        public void ScrollTo() =>
            Page.ScrollToElement(this);

        /// <summary>
        /// Submits a Form element
        /// </summary>
        public void Submit() =>
            Page.SubmitElement(this);

        /// <summary>
        /// Gets the label associated with the element.
        /// </summary>
        /// <returns>The text of the label associated with the element.</returns>
        public string GetLabel() =>
            Page.GetElementLabel(this);

        /// <summary>
        /// Gets whether the element is visible.
        /// </summary>
        /// <returns><c>true</c> if the element is visible; otherwise, <c>false</c></returns>
        public bool GetIsVisible() =>
            Page.GetElementIsVisible(this);

        /// <summary>
        /// Gets whether the element is on screen.
        /// </summary>
        /// <returns><c>true</c> if the element is on screen; otherwise, <c>false</c></returns>
        public bool GetIsOnScreen() =>
            Page.GetElementIsOnScreen(this);

        /// <summary>
        /// Gets all items in a list element.
        /// </summary>
        /// <returns>A collection of strings containing the items in the list</returns>
        public IReadOnlyCollection<ListItem> GetListItems() =>
            Page.GetListItems(this);

        /// <summary>
        /// Gets the element access key.
        /// </summary>
        /// <returns>A string representing the access key</returns>
        public string GetAccessKey() =>
            Page.GetElementAccessKey(this);

        /// <summary>
        /// Gets the range of a slider element.
        /// </summary>
        /// <returns>The minimum and maximum values of the slider.</returns>
        public SliderRange GetSliderRange() =>
            Page.GetSliderRange(this);

        /// <summary>
        /// Selects the list item.
        /// </summary>
        /// <param name="index">The index of the item to select.</param>
        /// <param name="name">The name of the item to select.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        public void SelectListItem(int index, string name) =>
            Page.SelectListItem(this, index, name);

        /// <summary>
        /// Adds to list selection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="name">The name.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        public void AddToListSelection(int index, string name) =>
            Page.AddToListSelection(this, index, name);

        /// <summary>
        /// Removes from list selection.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="name">The name.</param>
        /// <remarks>If <paramref name="name"/> is not null or empty then <paramref name="index"/> is ignored.</remarks>
        public void RemoveFromListSelection(int index, string name) =>
            Page.RemoveFromListSelection(this, index, name);

        /// <summary>
        /// Is the list item selected
        /// </summary>
        /// <returns><c>true</c> if the list item is selected; otherwise, <c>false</c></returns>
        public bool GetIsListItemSelected() =>
            Page.GetIsListItemSelected(this);

    
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(IWebElement other) =>
            Id.Equals(other?.Id);

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WebElement) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() =>
            Id.GetHashCode();

        /// <summary>
        /// Checks if the parent document of the element has finished loading
        /// </summary>
        /// <returns><c>true</c> if the parent document has loaded</returns>
        public bool CheckParentDocumentLoaded() =>
            Page.CheckParentDocumentLoaded(this);

        /// <summary>
        /// Returns a rectangle.
        /// </summary>
        /// <returns>
        /// A rectangle representing the element location and bounds on the screen. 
        /// </returns>
        public Rectangle GetElementScreenLocationAndBounds() =>
            Page.GetElementScreenLocationAndBounds(this);
    }
}
