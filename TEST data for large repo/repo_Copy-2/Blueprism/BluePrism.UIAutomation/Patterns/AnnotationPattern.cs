namespace BluePrism.UIAutomation.Patterns
{
    using Utilities.Functional;

    public class AnnotationPattern : BasePattern, IAnnotationPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationAnnotationPattern _pattern;

        public int CurrentAnnotationTypeId => _pattern.CurrentAnnotationTypeId;

        public string CurrentAnnotationTypeName => _pattern.CurrentAnnotationTypeName;

        public string CurrentAuthor => _pattern.CurrentAuthor;

        public string CurrentDateTime => _pattern.CurrentDateTime;

        public IAutomationElement CurrentTarget => _pattern.CurrentTarget.Map(_automationFactory.FromUIAutomationElement);

        public int CachedAnnotationTypeId => _pattern.CachedAnnotationTypeId;

        public string CachedAnnotationTypeName => _pattern.CachedAnnotationTypeName;

        public string CachedAuthor => _pattern.CachedAuthor;

        public string CachedDateTime => _pattern.CachedDateTime;

        public IAutomationElement CachedTarget => _pattern.CachedTarget.Map(_automationFactory.FromUIAutomationElement);


        public AnnotationPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }
    }
}