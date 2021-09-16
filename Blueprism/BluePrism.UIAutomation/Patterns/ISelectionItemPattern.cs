namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.SelectionItemPattern)]
    public interface ISelectionItemPattern : IAutomationPattern
    {
        bool CachedIsSelected { get; }
        IAutomationElement CachedSelectionContainer { get; }
        bool CurrentIsSelected { get; }
        IAutomationElement CurrentSelectionContainer { get; }

        void AddToSelection();
        void RemoveFromSelection();
        void Select();
    }
}