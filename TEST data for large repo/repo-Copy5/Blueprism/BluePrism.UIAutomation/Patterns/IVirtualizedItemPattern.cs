namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.VirtualizedItemPattern)]
    public interface IVirtualizedItemPattern : IAutomationPattern
    {
        void Realize();
    }
}