using System;

namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.MultipleViewPattern)]
    public interface IMultipleViewPattern : IAutomationPattern
    {
        int CachedCurrentView { get; }
        int CurrentCurrentView { get; }

        Array GetCachedSupportedViews();
        Array GetCurrentSupportedViews();
        string GetViewName(int view);
        void SetCurrentView(int view);
    }
}