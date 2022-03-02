namespace BluePrism.UIAutomation.Patterns
{
    public class ValuePattern : BasePattern, IValuePattern
    {
        private readonly UIAutomationClient.IUIAutomationValuePattern _pattern;

        public bool CachedIsReadOnly => _pattern.CachedIsReadOnly != 0;
        public string CachedValue => _pattern.CachedValue;
        public bool CurrentIsReadOnly => _pattern.CurrentIsReadOnly != 0;
        public string CurrentValue => _pattern.CurrentValue;

        public ValuePattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void SetValue(string value)
            => _pattern.SetValue(value);
    }
}