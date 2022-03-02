namespace BluePrism.UIAutomation.Conditions
{
    /// <summary>
    /// Represents a UIA condition
    /// </summary>
    public interface IAutomationCondition
    {
        /// <summary>
        /// The base condition object
        /// </summary>
        UIAutomationClient.IUIAutomationCondition Condition { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is custom condition.
        /// </summary>
        bool IsCustomCondition { get; }

        /// <summary>
        /// Evaluates the condition for the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="cacheRequest">The cache request.</param>
        /// <returns>The result of the evaluation.</returns>
        bool Evaluate(IAutomationElement element, IAutomationCacheRequest cacheRequest);
    }
}