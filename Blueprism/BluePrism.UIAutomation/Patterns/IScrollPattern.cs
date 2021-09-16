namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.ScrollPattern)]
    public interface IScrollPattern : IAutomationPattern
    {
        int CachedHorizontallyScrollable { get; }
        double CachedHorizontalScrollPercent { get; }
        double CachedHorizontalViewSize { get; }
        int CachedVerticallyScrollable { get; }
        double CachedVerticalScrollPercent { get; }
        double CachedVerticalViewSize { get; }
        int CurrentHorizontallyScrollable { get; }
        double CurrentHorizontalScrollPercent { get; }
        double CurrentHorizontalViewSize { get; }
        int CurrentVerticallyScrollable { get; }
        double CurrentVerticalScrollPercent { get; }
        double CurrentVerticalViewSize { get; }

        void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount);
        void SetScrollPercent(double horizontalPercent, double verticalPercent);
    }
}