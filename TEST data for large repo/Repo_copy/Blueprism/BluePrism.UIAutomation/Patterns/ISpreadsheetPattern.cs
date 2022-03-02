namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.SpreadsheetPattern)]
    public interface ISpreadsheetPattern : IAutomationPattern
    {
        IAutomationElement GetItemByName(string name);
    }
}