namespace BluePrism.UIAutomation
{
    using System;
    using System.Drawing;

    using Conditions;
    using UIAutomationClient;
    using IAccessible = Accessibility.IAccessible;

    /// <summary>
    /// Provides factory methods for UIA types
    /// </summary>
    public interface IAutomationFactory
    {
        /// <summary>
        /// Gets the currently focused element.
        /// </summary>
        /// <returns>The currently focused element</returns>
        IAutomationElement GetFocusedElement();
        /// <summary>
        /// Gets the root element.
        /// </summary>
        /// <returns>The root element</returns>
        IAutomationElement GetRootElement();

        /// <summary>
        /// Gets the parent element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element's parent or <c>null</c> if the element has no parent</returns>
        IAutomationElement GetParentElement(IAutomationElement element);
        /// <summary>
        /// Gets the parent element with caching enabled.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>The element's parent or <c>null</c> if the element has no parent</returns>
        IAutomationElement GetParentElement(IAutomationElement element, IAutomationCacheRequest cacheRequest);


        /// <summary>
        /// Gets an automation element from an accessible element
        /// </summary>
        /// <param name="acc">The accessible element from which to draw a
        /// UIAutomation element.</param>
        /// <param name="childId">The child ID of the accessible element.</param>
        /// <returns>An IAutomationElement representing the same UI element as the
        /// given accessible element.</returns>
        IAutomationElement FromIAccessible(IAccessible acc, int childId);
        /// <summary>
        /// Gets the element from a window handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>The element identified by the handle</returns>
        IAutomationElement FromHandle(IntPtr handle);

        /// <summary>
        /// Gets the element from a window handle.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="cache">The cache request to use to retrieve information
        /// about the automation element.</param>
        /// <returns>The element identified by the handle</returns>
        IAutomationElement FromHandle(IntPtr handle, IAutomationCacheRequest cache);

        /// <summary>
        /// Gets the element under the given point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The element under the given point.</returns>
        IAutomationElement FromPoint(Point point);

        /// <summary>
        /// Gets a wrapper element from the given element.
        /// </summary>
        /// <param name="uiAutomationElement">The UI automation element</param>
        /// <returns>A wrapper around the given element or null if the given element
        /// was null.</returns>
        // ReSharper disable once InconsistentNaming
        IAutomationElement FromUIAutomationElement(IUIAutomationElement uiAutomationElement);

        /// <summary>
        /// Creates a cache request.
        /// </summary>
        /// <returns>A new cache request.</returns>
        IAutomationCacheRequest CreateCacheRequest();

        /// <summary>
        /// Creates a text range from a UI automation object.
        /// </summary>
        /// <param name="textRange">The UI Automation text range.</param>
        /// <returns>The created text range</returns>
        // ReSharper disable once InconsistentNaming
        IAutomationTextRange TextRangeFromUIAutomationObject(IUIAutomationTextRange textRange);

        /// <summary>
        /// Creates an and condition.
        /// </summary>
        /// <param name="condition1">The first condition to AND.</param>
        /// <param name="condition2">The second condition to AND.</param>
        /// <returns>A condition which ANDs the given conditions.</returns>
        IAutomationCondition CreateAndCondition(IAutomationCondition condition1, IAutomationCondition condition2);
        /// <summary>
        /// Creates an and condition.
        /// </summary>
        /// <param name="conditions">The conditions to AND.</param>
        /// <returns>A condition which ANDs the given conditions.</returns>
        IAutomationCondition CreateAndCondition(params IAutomationCondition[] conditions);
        /// <summary>
        /// Creates a false condition.
        /// </summary>
        /// <returns>A condition which always returns false</returns>
        IAutomationCondition CreateFalseCondition();
        /// <summary>
        /// Creates a not condition.
        /// </summary>
        /// <param name="condition">The condition to NOT.</param>
        /// <returns>A condition which NOTs the given condition.</returns>
        IAutomationCondition CreateNotCondition(IAutomationCondition condition);
        /// <summary>
        /// Creates an or condition.
        /// </summary>
        /// <param name="condition1">The first condition to OR.</param>
        /// <param name="condition2">The second condition to OR.</param>
        /// <returns>A condition which ORs the given conditions.</returns>
        IAutomationCondition CreateOrCondition(IAutomationCondition condition1, IAutomationCondition condition2);
        /// <summary>
        /// Creates a property condition.
        /// </summary>
        /// <param name="propertyType">The property to inspect.</param>
        /// <param name="value">The value to look for.</param>
        /// <returns>A condition which checks that the given property is equal to the given value.</returns>
        IAutomationCondition CreatePropertyCondition(PropertyType propertyType, object value);
        /// <summary>
        /// Creates a true condition.
        /// </summary>
        /// <returns>A condition which always returns true</returns>
        IAutomationCondition CreateTrueCondition();
        /// <summary>
        /// Creates a custom condition.
        /// </summary>
        /// <param name="evaluate">The function called to evaluate whether an element is a match.</param>
        /// <returns>A condition which performs a custom check on an element.</returns>
        IAutomationCondition CreateCustomCondition(Func<IAutomationElement, bool> evaluate);
        /// <summary>
        /// Creates a custom condition.
        /// </summary>
        /// <param name="evaluate">The function called to evaluate whether an element is a match.</param>
        /// <returns>A condition which performs a custom check on an element.</returns>
        IAutomationCondition CreateCustomCondition(Func<IAutomationElement, IAutomationCacheRequest, bool> evaluate);

        /// <summary>
        /// Creates a tree walker.
        /// </summary>
        /// <param name="condition">The condition to apply to the walker.</param>
        /// <returns>A new tree walker</returns>
        IAutomationTreeWalker CreateTreeWalker(IAutomationCondition condition);

        /// <summary>
        /// Creates a raw tree walker.
        /// </summary>
        /// <returns>A tree walker which only looks for controls.</returns>
        IAutomationTreeWalker CreateRawTreeWalker();

        /// <summary>
        /// Creates a control-only tree walker.
        /// </summary>
        /// <returns>A tree walker which only looks for controls.</returns>
        IAutomationTreeWalker CreateControlTreeWalker();

        /// <summary>
        /// Creates a control-only tree walker.
        /// </summary>
        /// <returns>A tree walker which only looks for controls.</returns>
        IAutomationTreeWalker CreateContentTreeWalker();

        /// <summary>
        /// Gets an IAutomationGridText instance which helps in providing text from 
        /// IAutomationElement cells / children of the IGridPattern.
        /// </summary>
        /// <returns>A implementation of IAutomationGridText.</returns>
        IAutomationGridText GetGridTextProvider();

    }
}