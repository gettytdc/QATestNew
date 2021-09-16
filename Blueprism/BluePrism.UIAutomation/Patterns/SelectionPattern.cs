namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;
    using System.Linq;

    public class SelectionPattern : BasePattern, ISelectionPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationSelectionPattern _pattern;

        public bool CurrentCanSelectMultiple => _pattern.CurrentCanSelectMultiple != 0;
        public bool CurrentIsSelectionRequired => _pattern.CurrentIsSelectionRequired != 0;
        public bool CachedCanSelectMultiple => _pattern.CachedCanSelectMultiple != 0;
        public bool CachedIsSelectionRequired => _pattern.CachedIsSelectionRequired != 0;

        public SelectionPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IEnumerable<IAutomationElement> GetCurrentSelection() =>
            _pattern.GetCurrentSelection().ToEnumerable().Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCachedSelection() =>
            _pattern.GetCachedSelection().ToEnumerable().Select(_automationFactory.FromUIAutomationElement);
    }
}