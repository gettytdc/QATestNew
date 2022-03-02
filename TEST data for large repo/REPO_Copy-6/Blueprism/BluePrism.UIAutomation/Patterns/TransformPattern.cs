namespace BluePrism.UIAutomation.Patterns
{
    public class TransformPattern : BasePattern, ITransformPattern
    {
        private readonly UIAutomationClient.IUIAutomationTransformPattern _pattern;

        public bool CurrentCanMove => _pattern.CurrentCanMove != 0;

        public bool CurrentCanResize => _pattern.CurrentCanResize != 0;

        public bool CurrentCanRotate => _pattern.CurrentCanRotate != 0;

        public bool CachedCanMove => _pattern.CachedCanMove != 0;

        public bool CachedCanResize => _pattern.CachedCanResize != 0;

        public bool CachedCanRotate => _pattern.CachedCanRotate != 0;

        public TransformPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public void Move(double x, double y) =>
            _pattern.Move(x, y);

        public void Resize(double width, double height) =>
            _pattern.Resize(width, height);

        public void Rotate(double degrees) =>
            _pattern.Rotate(degrees);
    }
}