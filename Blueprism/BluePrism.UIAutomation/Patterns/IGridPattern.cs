namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.GridPattern)]
    public interface IGridPattern : IAutomationPattern
    {
        int CachedColumnCount { get; }
        int CachedRowCount { get; }
        int CurrentColumnCount { get; }
        int CurrentRowCount { get; }

        IAutomationElement GetItem(int row, int column);
    }
}