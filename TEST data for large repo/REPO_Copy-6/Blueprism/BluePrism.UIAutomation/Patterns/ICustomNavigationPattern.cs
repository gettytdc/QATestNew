namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.CustomNavigationPattern)]
    public interface ICustomNavigationPattern : IAutomationPattern
    {
        IAutomationElement Navigate(NavigateDirection direction);
    }
}