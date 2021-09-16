namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.TogglePattern)]
    public interface ITogglePattern : IAutomationPattern
    {
        ToggleState CachedToggleState { get; }
        ToggleState CurrentToggleState { get; }

        void Toggle();
    }
}