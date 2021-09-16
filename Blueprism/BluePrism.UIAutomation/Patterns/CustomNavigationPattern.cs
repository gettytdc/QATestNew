namespace BluePrism.UIAutomation.Patterns
{
    using Utilities.Functional;

    public class CustomNavigationPattern : BasePattern, ICustomNavigationPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationCustomNavigationPattern _pattern;

        public CustomNavigationPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IAutomationElement Navigate(NavigateDirection direction) =>
            _pattern.Navigate((UIAutomationClient.NavigateDirection) direction)
            .Map(_automationFactory.FromUIAutomationElement);
    }
}