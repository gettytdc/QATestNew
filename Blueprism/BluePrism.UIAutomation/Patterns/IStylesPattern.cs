namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.StylesPattern)]
    public interface IStylesPattern : IAutomationPattern
    {
        string CachedExtendedProperties { get; }
        int CachedFillColor { get; }
        int CachedFillPatternColor { get; }
        string CachedFillPatternStyle { get; }
        string CachedShape { get; }
        int CachedStyleId { get; }
        string CachedStyleName { get; }
        string CurrentExtendedProperties { get; }
        int CurrentFillColor { get; }
        int CurrentFillPatternColor { get; }
        string CurrentFillPatternStyle { get; }
        string CurrentShape { get; }
        int CurrentStyleId { get; }
        string CurrentStyleName { get; }
    }
}