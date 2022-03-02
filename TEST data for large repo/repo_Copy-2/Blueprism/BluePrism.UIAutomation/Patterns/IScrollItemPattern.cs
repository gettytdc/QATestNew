namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.ScrollItemPattern)]
    public interface IScrollItemPattern : IAutomationPattern
    {
        void ScrollIntoView();
    }
}