namespace BluePrism.UIAutomation.Patterns
{
    public class ExpandCollapsePattern : BasePattern, IExpandCollapsePattern
    {
        private readonly UIAutomationClient.IUIAutomationExpandCollapsePattern _pattern;

        public ExpandCollapseState CurrentExpandCollapseState => (ExpandCollapseState)_pattern.CurrentExpandCollapseState;
        public ExpandCollapseState CachedExpandCollapseState => (ExpandCollapseState)_pattern.CachedExpandCollapseState;

        public ExpandCollapsePattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Expand() =>
            _pattern.Expand();

        public void Collapse() =>
            _pattern.Collapse();

        public void ExpandCollapse()
        {
            switch ((ExpandCollapseState) _pattern.CurrentExpandCollapseState)
            {
                case ExpandCollapseState.Collapsed:
                    Expand();
                    break;

                case ExpandCollapseState.Expanded:
                case ExpandCollapseState.PartiallyExpanded:
                    Collapse();
                    break;

                case ExpandCollapseState.LeafNode:
                    break;
            }
        }
    }
}
