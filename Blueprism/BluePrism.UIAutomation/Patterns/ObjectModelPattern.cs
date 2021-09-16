namespace BluePrism.UIAutomation.Patterns
{
    public class ObjectModelPattern : BasePattern, IObjectModelPattern
    {
        private readonly UIAutomationClient.IUIAutomationObjectModelPattern _pattern;

        public ObjectModelPattern(IAutomationElement element)
        {
            _pattern = GetPattern(() => _pattern, element);
        }

        public object GetUnderlyingObjectModel() =>
            _pattern.GetUnderlyingObjectModel();
    }
}