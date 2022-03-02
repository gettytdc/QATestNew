namespace BluePrism.UIAutomation.Patterns
{
    using Utilities.Functional;

    public class TextChildPattern : BasePattern, ITextChildPattern
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationTextChildPattern _pattern;

        public IAutomationElement TextContainer => 
            ComHelper.TryGetComValue(() => _pattern.TextContainer)
            ?.Map(_automationFactory.FromUIAutomationElement);

        public IAutomationTextRange TextRange =>
            ComHelper.TryGetComValue(() => _pattern.TextRange)
            ?.Map(_automationFactory.TextRangeFromUIAutomationObject);

        public TextChildPattern(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }
    }
}