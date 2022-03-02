namespace BluePrism.UIAutomation.Patterns
{
    public class TransformPattern2 : BasePattern, ITransformPattern2
    {
        private readonly UIAutomationClient.IUIAutomationTransformPattern2 _pattern;

        public int CurrentCanMove => _pattern.CurrentCanMove;

        public int CurrentCanResize => _pattern.CurrentCanResize;

        public int CurrentCanRotate => _pattern.CurrentCanRotate;

        public int CachedCanMove => _pattern.CachedCanMove;

        public int CachedCanResize => _pattern.CachedCanResize;

        public int CachedCanRotate => _pattern.CachedCanRotate;

        public int CurrentCanZoom => _pattern.CurrentCanZoom;

        public int CachedCanZoom => _pattern.CachedCanZoom;

        public double CurrentZoomLevel => _pattern.CurrentZoomLevel;

        public double CachedZoomLevel => _pattern.CachedZoomLevel;

        public double CurrentZoomMinimum => _pattern.CurrentZoomMinimum;

        public double CachedZoomMinimum => _pattern.CachedZoomMinimum;

        public double CurrentZoomMaximum => _pattern.CurrentZoomMaximum;

        public double CachedZoomMaximum => _pattern.CachedZoomMaximum;

        public TransformPattern2(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Move(double x, double y) =>
            _pattern.Move(x, y);

        public void Resize(double width, double height) =>
            _pattern.Resize(width, height);

        public void Rotate(double degrees) =>
            _pattern.Rotate(degrees);

        public void Zoom(double zoomValue) =>
            _pattern.Zoom(zoomValue);

        public void ZoomByUnit(ZoomUnit zoomUnit) =>
            _pattern.ZoomByUnit((UIAutomationClient.ZoomUnit)zoomUnit);
    }
}