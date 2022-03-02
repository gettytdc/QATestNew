namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.RangeValuePattern)]
    public interface IRangeValuePattern : IAutomationPattern
    {
        int CachedIsReadOnly { get; }
        double CachedLargeChange { get; }
        double CachedMaximum { get; }
        double CachedMinimum { get; }
        double CachedSmallChange { get; }
        double CachedValue { get; }
        int CurrentIsReadOnly { get; }
        double CurrentLargeChange { get; }
        double CurrentMaximum { get; }
        double CurrentMinimum { get; }
        double CurrentSmallChange { get; }
        double CurrentValue { get; }

        void SetValue(double value);
    }
}