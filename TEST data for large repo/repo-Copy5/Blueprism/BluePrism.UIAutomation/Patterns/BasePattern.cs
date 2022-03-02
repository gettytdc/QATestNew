namespace BluePrism.UIAutomation.Patterns
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Base class used by all managed pattern classes. This ensures that the pattern
    /// type is retrieved using the <see cref="RepresentsPatternTypeAttribute"/>
    /// which decorates the pattern implementation that the subclass implements.
    /// </summary>
    public abstract class BasePattern : IAutomationPattern
    {
        /// <summary>
        /// Gets the pattern type for this pattern.
        /// This gets the <see cref="IAutomationPattern"/> subinterface that the
        /// concrete subclass of BasePattern implements, and picks the pattern type
        /// from the <see cref="RepresentsPatternTypeAttribute"/> which decorates
        /// that interface.
        /// </summary>
        public PatternType PatternType =>
            RepresentsPatternTypeAttribute.GetPatternType(GetType());

        /// <summary>
        /// Checks if this pattern is supported by the given element.
        /// </summary>
        /// <param name="element">The element to test to see if this pattern is
        /// supported by it</param>
        /// <returns>true if this pattern is supported by the given element; false
        /// otherwise.</returns>
        public virtual bool IsSupportedBy(IAutomationElement element) =>
            element?.Element.CheckAndGetCurrentPattern(PatternType) != null;

        protected TPattern GetPattern<TPattern>(
            Expression<Func<TPattern>> expression,
            IAutomationElement element)
            where TPattern : class
            =>
            element?.Element.CheckAndGetCurrentPattern<TPattern>(PatternType);
    }
}
