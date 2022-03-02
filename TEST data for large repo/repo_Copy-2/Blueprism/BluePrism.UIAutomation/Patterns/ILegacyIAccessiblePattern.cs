namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;

    [RepresentsPatternType(PatternType.LegacyIAccessiblePattern)]
    public interface ILegacyIAccessiblePattern : IAutomationPattern
    {
        int CachedChildId { get; }
        string CachedDefaultAction { get; }
        string CachedDescription { get; }
        string CachedHelp { get; }
        string CachedKeyboardShortcut { get; }
        string CachedName { get; }
        uint CachedRole { get; }
        uint CachedState { get; }
        string CachedValue { get; }
        int CurrentChildId { get; }
        string CurrentDefaultAction { get; }
        string CurrentDescription { get; }
        string CurrentHelp { get; }
        string CurrentKeyboardShortcut { get; }
        string CurrentName { get; }
        uint CurrentRole { get; }
        uint CurrentState { get; }
        string CurrentValue { get; }

        void DoDefaultAction();
        IEnumerable<IAutomationElement> GetCachedSelection();
        IEnumerable<IAutomationElement> GetCurrentSelection();
        UIAutomationClient.IAccessible GetIAccessible();
        void Select(int flagsSelect);
        void SetValue(string value);
    }
}