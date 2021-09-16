namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;
    using System.Linq;

    public class TableItemPattern : BasePattern, ITableItemPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationTableItemPattern _pattern;

        public TableItemPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IEnumerable<IAutomationElement> GetCurrentRowHeaderItems() =>
            _pattern.GetCurrentColumnHeaderItems()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCurrentColumnHeaderItems() =>
            _pattern.GetCurrentColumnHeaderItems()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCachedRowHeaderItems() =>
            _pattern.GetCachedRowHeaderItems()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCachedColumnHeaderItems() =>
            _pattern.GetCachedColumnHeaderItems()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

    }
}