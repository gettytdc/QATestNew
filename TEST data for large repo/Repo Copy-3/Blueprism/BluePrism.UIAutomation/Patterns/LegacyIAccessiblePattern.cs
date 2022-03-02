namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;
    using System.Linq;

    public class LegacyIAccessiblePattern : BasePattern, ILegacyIAccessiblePattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationLegacyIAccessiblePattern _pattern;

        public int CurrentChildId => _pattern.CurrentChildId;
        public string CurrentName => _pattern.CurrentName;
        public string CurrentValue => _pattern.CurrentValue;
        public string CurrentDescription => _pattern.CurrentDescription;
        public uint CurrentRole => _pattern.CurrentRole;
        public uint CurrentState => _pattern.CurrentState;
        public string CurrentHelp => _pattern.CurrentHelp;
        public string CurrentKeyboardShortcut => _pattern.CurrentKeyboardShortcut;
        public string CurrentDefaultAction => _pattern.CurrentDefaultAction;

        public int CachedChildId => _pattern.CachedChildId;
        public string CachedName => _pattern.CachedName;
        public string CachedValue => _pattern.CachedValue;
        public string CachedDescription => _pattern.CachedDescription;
        public uint CachedRole => _pattern.CachedRole;
        public uint CachedState => _pattern.CachedState;
        public string CachedHelp => _pattern.CachedHelp;
        public string CachedKeyboardShortcut => _pattern.CachedKeyboardShortcut;
        public string CachedDefaultAction => _pattern.CachedDefaultAction;

        public LegacyIAccessiblePattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Select(int flagsSelect) =>
            _pattern.Select(flagsSelect);

        public void DoDefaultAction() =>
            _pattern.DoDefaultAction();

        public void SetValue(string value) =>
            _pattern.SetValue(value);

        public IEnumerable<IAutomationElement> GetCurrentSelection() =>
            _pattern.GetCurrentSelection()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCachedSelection() =>
            _pattern.GetCachedSelection()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);

        public UIAutomationClient.IAccessible GetIAccessible() =>
            _pattern.GetIAccessible();
    }
}