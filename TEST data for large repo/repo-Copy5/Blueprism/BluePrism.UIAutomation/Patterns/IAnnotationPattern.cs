namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.AnnotationPattern)]
    public interface IAnnotationPattern : IAutomationPattern
    {
        int CachedAnnotationTypeId { get; }
        string CachedAnnotationTypeName { get; }
        string CachedAuthor { get; }
        string CachedDateTime { get; }
        IAutomationElement CachedTarget { get; }
        int CurrentAnnotationTypeId { get; }
        string CurrentAnnotationTypeName { get; }
        string CurrentAuthor { get; }
        string CurrentDateTime { get; }
        IAutomationElement CurrentTarget { get; }
    }
}