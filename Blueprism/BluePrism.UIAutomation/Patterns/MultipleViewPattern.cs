namespace BluePrism.UIAutomation.Patterns
{
    using System;

    public class MultipleViewPattern : BasePattern, IMultipleViewPattern
    {
        private readonly UIAutomationClient.IUIAutomationMultipleViewPattern _pattern;

        public int CurrentCurrentView => _pattern.CurrentCurrentView;
        public int CachedCurrentView => _pattern.CachedCurrentView;

        public MultipleViewPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public string GetViewName(int view) =>
            _pattern.GetViewName(view);

        public void SetCurrentView(int view) =>
            _pattern.SetCurrentView(view);

        public Array GetCurrentSupportedViews() =>
            _pattern.GetCurrentSupportedViews();

        public Array GetCachedSupportedViews() =>
            _pattern.GetCachedSupportedViews();

    }
}