namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;

    [RepresentsPatternType(PatternType.SelectionPattern)]
    public interface ISelectionPattern : IAutomationPattern
    {
        bool CurrentCanSelectMultiple { get; }
        bool CurrentIsSelectionRequired { get; }
        bool CachedCanSelectMultiple { get; }
        bool CachedIsSelectionRequired { get; }
        IEnumerable<IAutomationElement> GetCurrentSelection();
        IEnumerable<IAutomationElement> GetCachedSelection();
    }
}