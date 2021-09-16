namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;

    [RepresentsPatternType(PatternType.TablePattern)]
    public interface ITablePattern : IAutomationPattern
    {
        RowOrColumnMajor CachedRowOrColumnMajor { get; }
        RowOrColumnMajor CurrentRowOrColumnMajor { get; }

        IEnumerable<IAutomationElement> GetCachedColumnHeaders();
        IEnumerable<IAutomationElement> GetCachedRowHeaders();
        IEnumerable<IAutomationElement> GetCurrentColumnHeaders();
        IEnumerable<IAutomationElement> GetCurrentRowHeaders();
    }
}