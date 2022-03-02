namespace BluePrism.UIAutomation
{
    using System.Collections.Generic;

    using Conditions;

    /// <summary>
    /// Provides methods for assisting with UI automation element tree navigation
    /// </summary>
    public interface IAutomationTreeNavigationHelper
    {
        /// <summary>
        /// Finds all items matching the condition with a tree walker.
        /// </summary>
        /// <param name="element">The root element of the tree.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>
        /// The items matching the condition.
        /// </returns>
        IEnumerable<IAutomationElement> FindWithTreeWalker(
            IAutomationElement element,
            TreeScope scope,
            IAutomationCondition condition,
            IAutomationCacheRequest cacheRequest);
    }
}