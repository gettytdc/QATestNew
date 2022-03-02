namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.ItemContainerPattern)]
    public interface IItemContainerPattern : IAutomationPattern
    {
        IAutomationElement FindItemByProperty(IAutomationElement startAfter, PropertyType propertyId, object value);
    }
}