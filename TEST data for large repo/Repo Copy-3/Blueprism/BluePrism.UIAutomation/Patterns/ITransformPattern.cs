namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.TransformPattern)]
    public interface ITransformPattern : IAutomationPattern
    {
        bool CachedCanMove { get; }
        bool CachedCanResize { get; }
        bool CachedCanRotate { get; }
        bool CurrentCanMove { get; }
        bool CurrentCanResize { get; }
        bool CurrentCanRotate { get; }

        void Move(double x, double y);
        void Resize(double width, double height);
        void Rotate(double degrees);
    }
}