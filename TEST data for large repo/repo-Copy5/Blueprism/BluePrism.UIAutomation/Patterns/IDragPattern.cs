using System;
using System.Collections.Generic;

namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.DragPattern)]
    public interface IDragPattern : IAutomationPattern
    {
        string CachedDropEffect { get; }
        Array CachedDropEffects { get; }
        int CachedIsGrabbed { get; }
        string CurrentDropEffect { get; }
        Array CurrentDropEffects { get; }
        int CurrentIsGrabbed { get; }

        IEnumerable<IAutomationElement> GetCachedGrabbedItems();
        IEnumerable<IAutomationElement> GetCurrentGrabbedItems();
    }
}