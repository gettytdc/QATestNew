namespace BluePrism.UIAutomation.Patterns
{
    /// <summary>
    /// Interface which describes a pattern - a means by which properties and
    /// behaviours of an automation element can be retrieved or effected.
    /// </summary>
    public interface IAutomationPattern
    {
        /// <summary>
        /// Gets the pattern type for this pattern.
        /// This is retrieved from the <see cref="RepresentsPatternTypeAttribute"/>
        /// defined in the concrete class which extends this base pattern.
        /// </summary>
        PatternType PatternType { get; }

        /// <summary>
        /// Checks if this pattern is supported by the given element.
        /// </summary>
        /// <param name="element">The element to test to see if this pattern is
        /// supported by it</param>
        /// <returns>true if this pattern is supported by the given element; false
        /// otherwise.</returns>
        bool IsSupportedBy(IAutomationElement element);
    }
}