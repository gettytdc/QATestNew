namespace BluePrism.UIAutomation.Patterns
{
    using Utilities.Functional;

    public class GridItemPattern : BasePattern, IGridItemPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationGridItemPattern _pattern;

        public IAutomationElement CurrentContainingGrid =>
            _pattern.CurrentContainingGrid.Map(_automationFactory.FromUIAutomationElement);

        public int CurrentRow => _pattern.CurrentRow;

        public int CurrentColumn => _pattern.CurrentColumn;

        public int CurrentRowSpan => _pattern.CurrentRowSpan;

        public int CurrentColumnSpan => _pattern.CurrentColumnSpan;

        public IAutomationElement CachedContainingGrid => 
            _pattern.CachedContainingGrid.Map(_automationFactory.FromUIAutomationElement);

        public int CachedRow => _pattern.CachedRow;

        public int CachedColumn => _pattern.CachedColumn;

        public int CachedRowSpan => _pattern.CachedRowSpan;

        public int CachedColumnSpan => _pattern.CachedColumnSpan;

        public GridItemPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }
    }
}