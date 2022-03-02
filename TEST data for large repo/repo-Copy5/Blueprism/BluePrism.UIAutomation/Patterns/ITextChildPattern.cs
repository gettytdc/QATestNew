namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.TextChildPattern)]
    public interface ITextChildPattern : IAutomationPattern
    {
        IAutomationElement TextContainer { get; }
        IAutomationTextRange TextRange { get; }
    }
}