namespace BluePrism.UIAutomation.Patterns
{
    public class ScrollPattern : BasePattern, IScrollPattern
    {
        private readonly UIAutomationClient.IUIAutomationScrollPattern _pattern;

        public double CurrentHorizontalScrollPercent => _pattern.CurrentHorizontalScrollPercent;
        public double CurrentVerticalScrollPercent => _pattern.CurrentVerticalScrollPercent;
        public double CurrentHorizontalViewSize => _pattern.CurrentHorizontalViewSize;
        public double CurrentVerticalViewSize => _pattern.CurrentVerticalViewSize;
        public int CurrentHorizontallyScrollable => _pattern.CurrentHorizontallyScrollable;
        public int CurrentVerticallyScrollable => _pattern.CurrentVerticallyScrollable;

        public double CachedHorizontalScrollPercent => _pattern.CachedHorizontalScrollPercent;
        public double CachedVerticalScrollPercent => _pattern.CachedVerticalScrollPercent;
        public double CachedHorizontalViewSize => _pattern.CachedHorizontalViewSize;
        public double CachedVerticalViewSize => _pattern.CachedVerticalViewSize;
        public int CachedHorizontallyScrollable => _pattern.CachedHorizontallyScrollable;
        public int CachedVerticallyScrollable => _pattern.CachedVerticallyScrollable;

        public ScrollPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount) =>
            _pattern.Scroll(
                (UIAutomationClient.ScrollAmount)horizontalAmount,
                (UIAutomationClient.ScrollAmount)verticalAmount);


        public void SetScrollPercent(double horizontalPercent, double verticalPercent) =>
            _pattern.SetScrollPercent(horizontalPercent, verticalPercent);
    }
}
