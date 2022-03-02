namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Utilities.Functional;

    public class TextPattern2 : BasePattern, ITextPattern2
    {
        private readonly IAutomationFactory _automationFactory;
        private readonly UIAutomationClient.IUIAutomationTextPattern2 _pattern;

        public IAutomationTextRange DocumentRange =>
            _pattern.DocumentRange
                .Map(_automationFactory.TextRangeFromUIAutomationObject);

        public SupportedTextSelection SupportedTextSelection => (SupportedTextSelection)_pattern.SupportedTextSelection;

        public TextPattern2(IAutomationElement element, IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            _pattern = GetPattern(() => _pattern, element);
        }

        public IAutomationTextRange RangeFromPoint(Point point) =>
            point.ToTagPoint()
            .Map(_pattern.RangeFromPoint)
            ?.Map(_automationFactory.TextRangeFromUIAutomationObject);

        public IAutomationTextRange RangeFromChild(IAutomationElement child) =>
            child.Element
            .Map(_pattern.RangeFromChild)
            ?.Map(_automationFactory.TextRangeFromUIAutomationObject);

        public IEnumerable<IAutomationTextRange> GetSelection() =>
            _pattern.GetSelection()
            .ToEnumerable()
            .Select(_automationFactory.TextRangeFromUIAutomationObject);

        public IEnumerable<IAutomationTextRange> GetVisibleRanges() =>
            _pattern.GetVisibleRanges()
            .ToEnumerable()
            .Select(_automationFactory.TextRangeFromUIAutomationObject);

        public IAutomationTextRange RangeFromAnnotation(IAutomationElement annotation) =>
            annotation.Element
            .Map(_pattern.RangeFromChild)
            ?.Map(_automationFactory.TextRangeFromUIAutomationObject);

        public (IAutomationTextRange range, bool isActive) GetCaretRange() =>
            _pattern.GetCaretRange(out int isActive)
            .Map(x => (
                range: _automationFactory.TextRangeFromUIAutomationObject(x),
                isActive: isActive != 0));

    }
}