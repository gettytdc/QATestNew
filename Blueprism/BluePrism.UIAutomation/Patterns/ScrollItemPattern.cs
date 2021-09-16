namespace BluePrism.UIAutomation.Patterns
{
    public class ScrollItemPattern : BasePattern, IScrollItemPattern
    {
        private readonly UIAutomationClient.IUIAutomationScrollItemPattern _pattern;

        public ScrollItemPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void ScrollIntoView() =>
            _pattern.ScrollIntoView();
    }
}