namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.SynchronizedInputPattern)]
    public interface ISynchronizedInputPattern : IAutomationPattern
    {
        void Cancel();
        void StartListening(SynchronizedInputType inputType);
    }
}