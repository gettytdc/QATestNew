namespace BluePrism.UIAutomation.Patterns
{
    using Utilities.Functional;

    public class SelectionItemPattern : BasePattern, ISelectionItemPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationSelectionItemPattern _pattern;

        public bool CurrentIsSelected => _pattern.CurrentIsSelected != 0;

        public IAutomationElement CurrentSelectionContainer => 
            _pattern.CurrentSelectionContainer?.Map(_automationFactory.FromUIAutomationElement);

        public bool CachedIsSelected => _pattern.CachedIsSelected != 0;

        public IAutomationElement CachedSelectionContainer =>
            _pattern.CachedSelectionContainer.Map(_automationFactory.FromUIAutomationElement);

        public SelectionItemPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Select() =>
            _pattern.Select();

        public void AddToSelection() =>
            _pattern.AddToSelection();

        public void RemoveFromSelection() =>
            _pattern.RemoveFromSelection();

    }
}