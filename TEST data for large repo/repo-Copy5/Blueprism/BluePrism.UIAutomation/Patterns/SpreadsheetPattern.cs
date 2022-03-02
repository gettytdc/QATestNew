namespace BluePrism.UIAutomation.Patterns
{
    using Utilities.Functional;

    public class SpreadsheetPattern : BasePattern, ISpreadsheetPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationSpreadsheetPattern _pattern;

        public SpreadsheetPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IAutomationElement GetItemByName(string name) =>
            _pattern.GetItemByName(name)
            .Map(_automationFactory.FromUIAutomationElement);
    }
}