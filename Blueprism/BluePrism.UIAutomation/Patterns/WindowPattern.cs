namespace BluePrism.UIAutomation.Patterns
{
    public class WindowPattern : BasePattern, IWindowPattern
    {
        private readonly UIAutomationClient.IUIAutomationWindowPattern _pattern;

        public bool CurrentCanMaximize => _pattern.CurrentCanMaximize != 0;

        public bool CurrentCanMinimize => _pattern.CurrentCanMinimize != 0;

        public bool CurrentIsModal => _pattern.CurrentIsModal != 0;

        public bool CurrentIsTopmost => _pattern.CurrentIsTopmost != 0;

        public WindowVisualState CurrentWindowVisualState => (WindowVisualState)_pattern.CurrentWindowVisualState;

        public WindowInteractionState CurrentWindowInteractionState => (WindowInteractionState)_pattern.CurrentWindowInteractionState;

        public bool CachedCanMaximize => _pattern.CachedCanMaximize != 0;

        public bool CachedCanMinimize => _pattern.CachedCanMinimize != 0;

        public bool CachedIsModal => _pattern.CachedIsModal != 0;

        public bool CachedIsTopmost => _pattern.CachedIsTopmost != 0;

        public WindowVisualState CachedWindowVisualState => (WindowVisualState)_pattern.CachedWindowVisualState;

        public WindowInteractionState CachedWindowInteractionState => (WindowInteractionState)_pattern.CachedWindowInteractionState;

        public WindowPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Close() =>
            _pattern.Close();

        public int WaitForInputIdle(int milliseconds) =>
            _pattern.WaitForInputIdle(milliseconds);

        public void SetWindowVisualState(WindowVisualState state) =>
            _pattern.SetWindowVisualState((UIAutomationClient.WindowVisualState) state);
    }
}