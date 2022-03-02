namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.InvokePattern)]
    public interface IInvokePattern : IAutomationPattern
    {
        void Invoke();
    }
}