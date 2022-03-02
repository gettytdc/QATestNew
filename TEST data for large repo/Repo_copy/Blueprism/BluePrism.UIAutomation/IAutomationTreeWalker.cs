namespace BluePrism.UIAutomation
{
    /// <summary>
    /// Provides methods for interacting with UIA tree walkers
    /// </summary>
    public interface IAutomationTreeWalker
    {
        /// <summary>
        /// Gets the base tree walker.
        /// </summary>
        UIAutomationClient.IUIAutomationTreeWalker TreeWalker { get; }

        /// <summary>
        /// Gets the parent of the given element.
        /// </summary>
        /// <param name="child">The child element.</param>
        /// <returns>
        /// The parent of the given element or <c>null</c> if the element doesn't
        /// have a parent
        /// </returns>
        IAutomationElement GetParent(IAutomationElement child);

        /// <summary>
        /// Gets the parent of the given element with caching enabled.
        /// </summary>
        /// <param name="child">The child element.</param>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>
        /// The parent of the given element or <c>null</c> if the element doesn't
        /// have a parent
        /// </returns>
        IAutomationElement GetParent(
            IAutomationElement child, IAutomationCacheRequest cacheRequest);

        /// <summary>
        /// Normalizes the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A normalized element.</returns>
        IAutomationElement Normalize(IAutomationElement element);

        /// <summary>
        /// Normalizes the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>A normalized element.</returns>
        IAutomationElement Normalize(
            IAutomationElement element, IAutomationCacheRequest cacheRequest);

        /// <summary>
        /// Gets the first child of an automation element
        /// </summary>
        /// <param name="element">The element whose first child is required.</param>
        /// <returns>The first child of <paramref name="element"/> or null if it has
        /// no child elements.</returns>
        IAutomationElement GetFirstChild(IAutomationElement element);

        /// <summary>
        /// Gets the last child of an automation element
        /// </summary>
        /// <param name="element">The element whose last child is required.</param>
        /// <returns>The last child of <paramref name="element"/> or null if it has
        /// no child elements.</returns>
        IAutomationElement GetLastChild(IAutomationElement element);

        /// <summary>
        /// Gets the first child of an automation element
        /// </summary>
        /// <param name="element">The element whose first child is required.</param>
        /// <param name="cacheRequest">The cache request to use to get the first
        /// child element</param>
        /// <returns>The first child of <paramref name="element"/> or null if it has
        /// no child elements.</returns>
        IAutomationElement GetFirstChild(
            IAutomationElement element, IAutomationCacheRequest cacheRequest);

        /// <summary>
        /// Gets the last child of an automation element
        /// </summary>
        /// <param name="element">The element whose last child is required.</param>
        /// <param name="cacheRequest">The cache request to use to get the last
        /// child element</param>
        /// <returns>The last child of <paramref name="element"/> or null if it has
        /// no child elements.</returns>
        IAutomationElement GetLastChild(
            IAutomationElement element, IAutomationCacheRequest cacheRequest);

        /// <summary>
        /// Gets the next sibling of a given automation element
        /// </summary>
        /// <param name="element">The element whose next sibling is required.</param>
        /// <returns>The next sibling of the given element or null if it has no
        /// further siblings.</returns>
        IAutomationElement GetNextSibling(IAutomationElement element);

        /// <summary>
        /// Gets the previous sibling of a given automation element
        /// </summary>
        /// <param name="element">The element whose previous sibling is required.
        /// </param>
        /// <returns>The previous sibling of the given element or null if it has no
        /// further siblings.</returns>
        IAutomationElement GetPreviousSibling(IAutomationElement element);

        /// <summary>
        /// Gets the next sibling of a given automation element
        /// </summary>
        /// <param name="element">The element whose next sibling is required.</param>
        /// <param name="cacheRequest">The cache request to use to retrieve the
        /// element</param>
        /// <returns>The next sibling of the given element or null if it has no
        /// further siblings.</returns>
        IAutomationElement GetNextSibling(
            IAutomationElement element, IAutomationCacheRequest cacheRequest);

        /// <summary>
        /// Gets the previous sibling of a given automation element
        /// </summary>
        /// <param name="element">The element whose previous sibling is required.
        /// </param>
        /// <param name="cacheRequest">The cache request to use to retrieve the
        /// element</param>
        /// <returns>The previous sibling of the given element or null if it has no
        /// further siblings.</returns>
        IAutomationElement GetPreviousSibling(
            IAutomationElement element, IAutomationCacheRequest cacheRequest);

    }
}