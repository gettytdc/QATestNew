namespace BluePrism.UIAutomation.Patterns
{
    public class RangeValuePattern : BasePattern, IRangeValuePattern
    {
        private readonly UIAutomationClient.IUIAutomationRangeValuePattern _pattern;

        public double CurrentValue => _pattern.CurrentValue;
        public int CurrentIsReadOnly => _pattern.CurrentIsReadOnly;
        public double CurrentMaximum => _pattern.CurrentMaximum;
        public double CurrentMinimum => _pattern.CurrentMinimum;
        public double CurrentLargeChange => _pattern.CurrentLargeChange;
        public double CurrentSmallChange => _pattern.CurrentSmallChange;

        public double CachedValue => _pattern.CachedValue;
        public int CachedIsReadOnly => _pattern.CachedIsReadOnly;
        public double CachedMaximum => _pattern.CachedMaximum;
        public double CachedMinimum => _pattern.CachedMinimum;
        public double CachedLargeChange => _pattern.CachedLargeChange;
        public double CachedSmallChange => _pattern.CachedSmallChange;

        public RangeValuePattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void SetValue(double value) =>
            _pattern.SetValue(value);
    }
}