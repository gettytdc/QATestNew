namespace BluePrism.UIAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Conditions;
    using Patterns;

    /// <summary>
    /// Provides methods for interacting with UIA elements
    /// </summary>
    public interface IAutomationElement
    {
        /// <summary>
        /// Gets the base UIA element.
        /// </summary>
        UIAutomationClient.IUIAutomationElement Element { get; }

        /// <summary>
        /// Finds the first elements matching the criteria.
        /// </summary>
        /// <param name="scope">The search scope.</param>
        /// <param name="condition">The condition.</param>
        /// <returns>
        /// The first element that matches the criteria or <c>null</c> if no element is found
        /// </returns>
        IAutomationElement FindFirst(TreeScope scope, IAutomationCondition condition);

        /// <summary>
        /// Finds the first elements matching the criteria.
        /// </summary>
        /// <param name="scope">The search scope.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>
        /// The first element that matches the criteria or <c>null</c> if no element is found
        /// </returns>
        IAutomationElement FindFirst(TreeScope scope, IAutomationCondition condition, IAutomationCacheRequest cacheRequest);

        /// <summary>
        /// Finds all elements matching the criteria.
        /// </summary>
        /// <param name="scope">The search scope.</param>
        /// <param name="condition">The condition.</param>
        /// <returns>A collection of elements matching the criteria</returns>
        IEnumerable<IAutomationElement> FindAll(TreeScope scope, IAutomationCondition condition);

        /// <summary>
        /// Finds all elements within the given scope
        /// </summary>
        /// <param name="scope">The search scope.</param>
        /// <returns>A collection of elements</returns>
        IEnumerable<IAutomationElement> FindAll(TreeScope scope);

        /// <summary>
        /// Finds all elements within the given scope
        /// </summary>
        /// <param name="scope">The search scope.</param>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>A collection of elements</returns>
        IEnumerable<IAutomationElement> FindAll(TreeScope scope, IAutomationCacheRequest cacheRequest);

        /// <summary>
        /// Finds all elements of the given control type.
        /// </summary>
        /// <param name="scope">The search scope.</param>
        /// <param name="controlType">The type of control to find.</param>
        /// <returns>A collection of elements matching the criteria</returns>
        IEnumerable<IAutomationElement> FindAll(TreeScope scope, ControlType controlType);

        /// <summary>
        /// Finds the first elements matching the criteria.
        /// </summary>
        /// <param name="scope">The search scope.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>
        /// The first element that matches the criteria or <c>null</c> if no element is found
        /// </returns>
        IEnumerable<IAutomationElement> FindAll(TreeScope scope, IAutomationCondition condition, IAutomationCacheRequest cacheRequest);

        /// <summary>
        /// Gets the current value of a property.
        /// </summary>
        /// <param name="propertyType">The property to get.</param>
        /// <returns>The value of the property</returns>
        object GetCurrentPropertyValue(PropertyType propertyType);

        /// <summary>
        /// Focuses the element if possible.
        /// </summary>
        /// <returns><c>true</c> if the focus was set successfully; otherwise, <c>false</c></returns>
        bool SetFocus();

        /// <summary>
        /// Gets the current pattern of the given type.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>An IAutomationPattern object if one is available.</returns>
        IAutomationPattern GetCurrentPattern(PatternType pattern);

        /// <summary>
        /// Gets the current pattern of the given type.
        /// </summary>
        /// <typeparam name="TPattern">The type of the pattern.</typeparam>
        /// <returns>A pattern of the requested type if one is available.</returns>
        TPattern GetCurrentPattern<TPattern>() where TPattern : IAutomationPattern;

        /// <summary>
        /// Gets the supported patterns for this element.
        /// </summary>
        /// <returns>An enumeration of patterns supported by the element.</returns>
        IEnumerable<PatternType> GetSupportedPatterns();

        /// <summary>
        /// Determines if the given pattern is supported
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns><c>true</c> if the pattern is supported; otherwise, <c>false</c>.</returns>
        bool PatternIsSupported(PatternType pattern);
        
        /// <summary>
        /// Gets the immediate parent element of this element, if there is no parent null is returned.
        /// </summary>
        /// <returns>The automation element's parent</returns>
        IAutomationElement GetCurrentParent();

        /// <summary>
        /// Retrieves from the cache the immediate parent element of this element, 
        /// if there is no parent null is returned.
        /// </summary>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>The automation element's parent</returns>
        IAutomationElement GetCachedParent(IAutomationCacheRequest cacheRequest);
        
        /// <summary>
        /// Gets a simple automation element containing the cached property values
        /// of this element
        /// </summary>
        IAutomationElementFacade Cached { get; }

        /// <summary>
        /// Gets a simple automation element containing the current property values
        /// of this element
        /// </summary>
        IAutomationElementFacade Current { get; }

        /// <summary>
        /// Gets the bounds of the UIA element relative to its parent rather than the screen.
        /// </summary>
        /// <returns></returns>
        Rectangle GetCurrentBoundingRelativeClientRectangle();

        /// <summary>
        /// Gets the current bounding rectangle.
        /// </summary>
        Rectangle CurrentBoundingRectangle { get; }

        /// <summary>
        /// Gets the centre point of the element.
        /// </summary>
        Point CurrentCentrePoint { get; }
        /// <summary>
        /// Gets the name of the current class.
        /// </summary>
        string CurrentClassName { get; }
        /// <summary>
        /// Gets the current automation identifier.
        /// </summary>
        string CurrentAutomationId { get; }
        /// <summary>
        /// Gets the type of the current control as localized text.
        /// </summary>
        string CurrentLocalizedControlType { get; }
        /// <summary>
        /// Gets whether the control is a password.
        /// </summary>
        bool CurrentIsPassword { get; }
        /// <summary>
        /// Gets whether the control is required for form.
        /// </summary>
        bool CurrentIsRequiredForForm { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        string CurrentName { get; }
        /// <summary>
        /// Gets the current item status.
        /// </summary>
        string CurrentItemStatus { get; }
        /// <summary>
        /// Gets the type of the item.
        /// </summary>
        string CurrentItemType { get; }
        /// <summary>
        /// Gets a value indicating whether the control is off-screen.
        /// </summary>
        bool CurrentIsOffscreen { get; }
        /// <summary>
        /// Gets the current native window handle.
        /// </summary>
        IntPtr CurrentNativeWindowHandle { get; }
        /// <summary>
        /// Gets the control this control is labeled by.
        /// </summary>
        IAutomationElement CurrentLabeledBy { get; }
        /// <summary>
        /// Gets a value indicating whether the control is enabled.
        /// </summary>
        bool CurrentIsEnabled { get; }
        /// <summary>
        /// Gets the current accelerator key.
        /// </summary>
        string CurrentAcceleratorKey { get; }
        /// <summary>
        /// Gets the current access key.
        /// </summary>
        string CurrentAccessKey { get; }
        /// <summary>
        /// Gets a value indicating whether the control has keyboard focus.
        /// </summary>
        bool CurrentHasKeyboardFocus { get; }
        /// <summary>
        /// Gets the current help text.
        /// </summary>
        string CurrentHelpText { get; }
        /// <summary>
        /// Gets the current process identifier.
        /// </summary>
        int CurrentProcessId { get; }
        /// <summary>
        /// Gets the current orientation.
        /// </summary>
        OrientationType CurrentOrientation { get; }
        /// <summary>
        /// Gets the current path to this automation element from the root of the
        /// UIAutomation tree which is owned by the same process as this element.
        /// </summary>
        string CurrentPath { get; }
        /// <summary>
        /// Gets the current control type of the element.
        /// </summary>
        ControlType CurrentControlType { get; }

        /// <summary>
        /// Gets the cached bounding rectangle.
        /// </summary>
        Rectangle CachedBoundingRectangle { get; }
        /// <summary>
        /// Gets the cached class name.
        /// </summary>
        string CachedClassName { get; }
        /// <summary>
        /// Gets the cached automation identifier.
        /// </summary>
        string CachedAutomationId { get; }
        /// <summary>
        /// Gets the cached type of the current control as localized text.
        /// </summary>
        string CachedLocalizedControlType { get; }
        /// <summary>
        /// Gets a cached value indicating whether the control is a password
        /// </summary>
        bool CachedIsPassword { get; }
        /// <summary>
        /// Gets a cached value indicating whether the control is required for form
        /// </summary>
        bool CachedIsRequiredForForm { get; }
        /// <summary>
        /// Gets the cached name.
        /// </summary>
        string CachedName { get; }
        /// <summary>
        /// Gets the cached item status.
        /// </summary>
        string CachedItemStatus { get; }
        /// <summary>
        /// Gets the cached item type.
        /// </summary>
        string CachedItemType { get; }
        /// <summary>
        /// Gets a cached value indicating whether the control is off-screen
        /// </summary>
        bool CachedIsOffscreen { get; }
        /// <summary>
        /// Gets the cached native window handle.
        /// </summary>
        IntPtr CachedNativeWindowHandle { get; }
        /// <summary>
        /// Gets the cached control this control is labeled by.
        /// </summary>
        IAutomationElement CachedLabeledBy { get; }
        /// <summary>
        /// Gets a cached value indicating whether the control is enabled
        /// </summary>
        bool CachedIsEnabled { get; }
        /// <summary>
        /// Gets the cached accelerator key.
        /// </summary>
        string CachedAcceleratorKey { get; }
        /// <summary>
        /// Gets the cached access key.
        /// </summary>
        string CachedAccessKey { get; }
        /// <summary>
        /// Gets a cached value indicating whether the control has keyboard focus.
        /// </summary>
        bool CachedHasKeyboardFocus { get; }
        /// <summary>
        /// Gets the cached help text.
        /// </summary>
        string CachedHelpText { get; }
        /// <summary>
        /// Gets the cached process identifier.
        /// </summary>
        int CachedProcessId { get; }
        /// <summary>
        /// Gets the cached orientation.
        /// </summary>
        OrientationType CachedOrientation { get; }
        /// <summary>
        /// Gets the cached path to this automation element from the root of the
        /// UIAutomation tree which is owned by the same process as this element.
        /// </summary>
        string CachedPath { get; }
        /// <summary>
        /// Gets the cached control type of the element.
        /// </summary>
        ControlType CachedControlType { get; }

    }
}