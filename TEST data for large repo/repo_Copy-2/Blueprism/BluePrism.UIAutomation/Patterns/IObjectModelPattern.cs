namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.ObjectModelPattern)]
    public interface IObjectModelPattern : IAutomationPattern
    {
        object GetUnderlyingObjectModel();
    }
}