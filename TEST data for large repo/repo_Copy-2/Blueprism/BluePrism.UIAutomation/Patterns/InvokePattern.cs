namespace BluePrism.UIAutomation.Patterns
{
    public class InvokePattern : BasePattern, IInvokePattern
    {
        private readonly UIAutomationClient.IUIAutomationInvokePattern _pattern;

        public InvokePattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Invoke()
        {
            _pattern.Invoke();
        }
    }
}