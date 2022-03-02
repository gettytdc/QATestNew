namespace BluePrism.UIAutomation.Patterns
{
    using System;
    using System.Collections.Generic;

    [RepresentsPatternType(PatternType.SpreadsheetItemPattern)]
    public interface ISpreadsheetItemPattern : IAutomationPattern
    {
        string CachedFormula { get; }
        string CurrentFormula { get; }

        IEnumerable<IAutomationElement> GetCachedAnnotationObjects();
        Array GetCachedAnnotationTypes();
        IEnumerable<IAutomationElement> GetCurrentAnnotationObjects();
        Array GetCurrentAnnotationTypes();
    }
}