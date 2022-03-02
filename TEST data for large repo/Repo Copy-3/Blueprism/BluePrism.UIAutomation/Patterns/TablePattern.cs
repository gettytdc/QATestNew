namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;
    using System.Linq;

    public class TablePattern : BasePattern, ITablePattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationTablePattern _pattern;

        public RowOrColumnMajor CurrentRowOrColumnMajor => (RowOrColumnMajor)_pattern.CurrentRowOrColumnMajor;
        public RowOrColumnMajor CachedRowOrColumnMajor => (RowOrColumnMajor)_pattern.CachedRowOrColumnMajor;

        public TablePattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IEnumerable<IAutomationElement> GetCurrentRowHeaders() =>
            _pattern.GetCurrentRowHeaders()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCurrentColumnHeaders() =>
            _pattern.GetCurrentColumnHeaders()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCachedRowHeaders() =>
            _pattern.GetCachedRowHeaders()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCachedColumnHeaders() =>
            _pattern.GetCachedColumnHeaders()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);
    }
}