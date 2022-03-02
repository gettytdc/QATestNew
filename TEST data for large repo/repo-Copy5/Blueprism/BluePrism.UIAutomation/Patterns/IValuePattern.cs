namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.ValuePattern)]
    public interface IValuePattern : IAutomationPattern
    {
        bool CachedIsReadOnly { get; }
        string CachedValue { get; }
        bool CurrentIsReadOnly { get; }
        string CurrentValue { get; }
        void SetValue(string value);
    }
}