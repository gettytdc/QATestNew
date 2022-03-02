namespace BluePrism.UIAutomation.Patterns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SpreadsheetItemPattern : BasePattern, ISpreadsheetItemPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationSpreadsheetItemPattern _pattern;

        public string CurrentFormula => _pattern.CurrentFormula;

        public string CachedFormula => _pattern.CachedFormula;

        public SpreadsheetItemPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IEnumerable<IAutomationElement> GetCurrentAnnotationObjects() =>
            _pattern.GetCurrentAnnotationObjects()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public Array GetCurrentAnnotationTypes() =>
            _pattern.GetCurrentAnnotationTypes();

        public IEnumerable<IAutomationElement> GetCachedAnnotationObjects() =>
            _pattern.GetCachedAnnotationObjects()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public Array GetCachedAnnotationTypes() =>
            _pattern.GetCachedAnnotationTypes();
    }
}