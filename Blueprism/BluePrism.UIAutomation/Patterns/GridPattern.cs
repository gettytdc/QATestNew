namespace BluePrism.UIAutomation.Patterns
{
    using Utilities.Functional;

    public class GridPattern : BasePattern, IGridPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationGridPattern _pattern;

        public int CurrentRowCount => _pattern.CurrentRowCount;

        public int CurrentColumnCount => _pattern.CurrentColumnCount;

        public int CachedRowCount => _pattern.CachedRowCount;

        public int CachedColumnCount => _pattern.CachedColumnCount;

        public GridPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IAutomationElement GetItem(int row, int column) =>
            _pattern.GetItem(row, column).Map(_automationFactory.FromUIAutomationElement);
    }
}
