namespace BluePrism.UIAutomation.Patterns
{
    using Utilities.Functional;

    public class ItemContainerPattern : BasePattern, IItemContainerPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationItemContainerPattern _pattern;

        public ItemContainerPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IAutomationElement FindItemByProperty(
            IAutomationElement startAfter,
            PropertyType propertyId,
            object value)
            =>
            _pattern.FindItemByProperty(startAfter.Element, (int)propertyId, value)
            .Map(_automationFactory.FromUIAutomationElement);

    }
}