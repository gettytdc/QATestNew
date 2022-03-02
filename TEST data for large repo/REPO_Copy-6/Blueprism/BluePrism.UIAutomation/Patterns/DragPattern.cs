namespace BluePrism.UIAutomation.Patterns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DragPattern : BasePattern, IDragPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationDragPattern _pattern;

        public int CurrentIsGrabbed => _pattern.CurrentIsGrabbed;
        public int CachedIsGrabbed => _pattern.CachedIsGrabbed;
        public string CurrentDropEffect => _pattern.CurrentDropEffect;

        public string CachedDropEffect => _pattern.CachedDropEffect;
        public Array CurrentDropEffects => _pattern.CurrentDropEffects;
        public Array CachedDropEffects => _pattern.CachedDropEffects;

        public DragPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IEnumerable<IAutomationElement> GetCurrentGrabbedItems() =>
            _pattern.GetCurrentGrabbedItems().ToEnumerable().Select(_automationFactory.FromUIAutomationElement);

        public IEnumerable<IAutomationElement> GetCachedGrabbedItems() =>
            _pattern.GetCachedGrabbedItems().ToEnumerable().Select(_automationFactory.FromUIAutomationElement);
    }
}