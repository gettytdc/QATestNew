namespace BluePrism.UIAutomation.Patterns
{
    public class SynchronizedInputPattern : BasePattern, ISynchronizedInputPattern
    {
        private readonly UIAutomationClient.IUIAutomationSynchronizedInputPattern _pattern;

        public SynchronizedInputPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void StartListening(SynchronizedInputType inputType) =>
            _pattern.StartListening((UIAutomationClient.SynchronizedInputType) inputType);

        public void Cancel() =>
            _pattern.Cancel();

    }
}