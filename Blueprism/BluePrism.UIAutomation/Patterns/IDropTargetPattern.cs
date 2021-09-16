using System;

namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.DropTargetPattern)]
    public interface IDropTargetPattern : IAutomationPattern
    {
        string CachedDropTargetEffect { get; }
        Array CachedDropTargetEffects { get; }
        string CurrentDropTargetEffect { get; }
        Array CurrentDropTargetEffects { get; }
    }
}