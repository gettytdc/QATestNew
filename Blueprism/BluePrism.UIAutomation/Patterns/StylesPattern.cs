namespace BluePrism.UIAutomation.Patterns
{
    public class StylesPattern : BasePattern, IStylesPattern
    {
        private readonly UIAutomationClient.IUIAutomationStylesPattern _pattern;

        public int CurrentStyleId => _pattern.CurrentStyleId;

        public string CurrentStyleName => _pattern.CurrentStyleName;

        public int CurrentFillColor => _pattern.CurrentFillColor;

        public string CurrentFillPatternStyle => _pattern.CurrentFillPatternStyle;

        public string CurrentShape => _pattern.CurrentShape;

        public int CurrentFillPatternColor => _pattern.CurrentFillPatternColor;

        public string CurrentExtendedProperties => _pattern.CurrentExtendedProperties;

        public int CachedStyleId => _pattern.CachedStyleId;

        public string CachedStyleName => _pattern.CachedStyleName;

        public int CachedFillColor => _pattern.CachedFillColor;

        public string CachedFillPatternStyle => _pattern.CachedFillPatternStyle;

        public string CachedShape => _pattern.CachedShape;

        public int CachedFillPatternColor => _pattern.CachedFillPatternColor;

        public string CachedExtendedProperties => _pattern.CachedExtendedProperties;

        public StylesPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }
    }
}