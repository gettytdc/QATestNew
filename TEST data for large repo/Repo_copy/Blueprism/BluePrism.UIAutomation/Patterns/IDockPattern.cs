namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.DockPattern)]
    public interface IDockPattern : IAutomationPattern
    {
        DockPosition CachedDockPosition { get; }
        DockPosition CurrentDockPosition { get; }

        void SetDockPosition(DockPosition dockPosition);
    }
}