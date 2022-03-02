namespace BluePrism.UIAutomation.Patterns
{
    public class VirtualizedItemPattern : BasePattern, IVirtualizedItemPattern
    {
        private readonly UIAutomationClient.IUIAutomationVirtualizedItemPattern _pattern;

        public VirtualizedItemPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Realize() =>
            _pattern.Realize();
    }
}