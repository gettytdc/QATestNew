namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.GridItemPattern)]
    public interface IGridItemPattern : IAutomationPattern
    {
        int CachedColumn { get; }
        int CachedColumnSpan { get; }
        IAutomationElement CachedContainingGrid { get; }
        int CachedRow { get; }
        int CachedRowSpan { get; }
        int CurrentColumn { get; }
        int CurrentColumnSpan { get; }
        IAutomationElement CurrentContainingGrid { get; }
        int CurrentRow { get; }
        int CurrentRowSpan { get; }
    }
}