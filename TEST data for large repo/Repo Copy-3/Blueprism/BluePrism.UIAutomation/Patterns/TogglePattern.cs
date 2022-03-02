namespace BluePrism.UIAutomation.Patterns
{
    public class TogglePattern : BasePattern, ITogglePattern
    {
        private readonly UIAutomationClient.IUIAutomationTogglePattern _pattern;

        public ToggleState CurrentToggleState => (ToggleState)_pattern.CurrentToggleState;
        public ToggleState CachedToggleState => (ToggleState)_pattern.CachedToggleState;

        public TogglePattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Toggle() =>
            _pattern.Toggle();
    }
}
