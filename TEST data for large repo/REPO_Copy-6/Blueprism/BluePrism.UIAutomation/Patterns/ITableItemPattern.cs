using System.Collections.Generic;

namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.TableItemPattern)]
    public interface ITableItemPattern : IAutomationPattern
    {
        IEnumerable<IAutomationElement> GetCachedColumnHeaderItems();
        IEnumerable<IAutomationElement> GetCachedRowHeaderItems();
        IEnumerable<IAutomationElement> GetCurrentColumnHeaderItems();
        IEnumerable<IAutomationElement> GetCurrentRowHeaderItems();
    }
}