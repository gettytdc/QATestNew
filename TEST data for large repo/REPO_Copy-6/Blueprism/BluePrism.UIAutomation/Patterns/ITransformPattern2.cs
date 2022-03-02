namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.TransformPattern2)]
    public interface ITransformPattern2 : IAutomationPattern
    {
        int CachedCanMove { get; }
        int CachedCanResize { get; }
        int CachedCanRotate { get; }
        int CachedCanZoom { get; }
        double CachedZoomLevel { get; }
        double CachedZoomMaximum { get; }
        double CachedZoomMinimum { get; }
        int CurrentCanMove { get; }
        int CurrentCanResize { get; }
        int CurrentCanRotate { get; }
        int CurrentCanZoom { get; }
        double CurrentZoomLevel { get; }
        double CurrentZoomMaximum { get; }
        double CurrentZoomMinimum { get; }

        void Move(double x, double y);
        void Resize(double width, double height);
        void Rotate(double degrees);
        void Zoom(double zoomValue);
        void ZoomByUnit(ZoomUnit zoomUnit);
    }
}