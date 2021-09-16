namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;

    public interface IAutomationPatternFactory
    {
        IAutomationPattern GetCurrentPattern(IAutomationElement element, PatternType patternType);
        TPattern GetCurrentPattern<TPattern>(IAutomationElement element) where TPattern : IAutomationPattern;
        IEnumerable<PatternType> GetSupportedPatterns(IAutomationElement element);
    }
}