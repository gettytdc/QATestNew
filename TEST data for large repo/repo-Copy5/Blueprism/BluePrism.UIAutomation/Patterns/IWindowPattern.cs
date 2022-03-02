namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.WindowPattern)]
    public interface IWindowPattern : IAutomationPattern
    {
        bool CachedCanMaximize { get; }
        bool CachedCanMinimize { get; }
        bool CachedIsModal { get; }
        bool CachedIsTopmost { get; }
        WindowInteractionState CachedWindowInteractionState { get; }
        WindowVisualState CachedWindowVisualState { get; }
        bool CurrentCanMaximize { get; }
        bool CurrentCanMinimize { get; }
        bool CurrentIsModal { get; }
        bool CurrentIsTopmost { get; }
        WindowInteractionState CurrentWindowInteractionState { get; }
        WindowVisualState CurrentWindowVisualState { get; }

        void Close();
        void SetWindowVisualState(WindowVisualState state);
        int WaitForInputIdle(int milliseconds);
    }
}