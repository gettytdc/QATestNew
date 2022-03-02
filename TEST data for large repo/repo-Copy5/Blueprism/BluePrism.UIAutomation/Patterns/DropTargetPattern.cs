namespace BluePrism.UIAutomation.Patterns
{
    using System;

    public class DropTargetPattern : BasePattern, IDropTargetPattern
    {
        private readonly UIAutomationClient.IUIAutomationDropTargetPattern _pattern;

        public string CurrentDropTargetEffect => _pattern.CurrentDropTargetEffect;
        public string CachedDropTargetEffect => _pattern.CachedDropTargetEffect;
        public Array CurrentDropTargetEffects => _pattern.CurrentDropTargetEffects;
        public Array CachedDropTargetEffects => _pattern.CachedDropTargetEffects;

        public DropTargetPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }
    }
}