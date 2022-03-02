namespace BluePrism.UIAutomation.Patterns
{
    public class DockPattern : BasePattern, IDockPattern
    {
        private readonly UIAutomationClient.IUIAutomationDockPattern _pattern;

        public DockPosition CurrentDockPosition => (DockPosition)_pattern.CurrentDockPosition;

        public DockPosition CachedDockPosition => (DockPosition)_pattern.CachedDockPosition;

        public DockPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void SetDockPosition(DockPosition dockPosition) =>
            _pattern.SetDockPosition((UIAutomationClient.DockPosition)dockPosition);
    }
}