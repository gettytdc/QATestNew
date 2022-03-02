namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.ExpandCollapsePattern)]
    public interface IExpandCollapsePattern : IAutomationPattern
    {
        ExpandCollapseState CachedExpandCollapseState { get; }
        ExpandCollapseState CurrentExpandCollapseState { get; }

        void Collapse();
        void Expand();
        void ExpandCollapse();
    }
}