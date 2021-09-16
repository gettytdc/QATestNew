namespace BluePrism.UIAutomation.Patterns
{
    using System.Collections.Generic;
    using System.Drawing;

    [RepresentsPatternType(PatternType.TextPattern2)]
    public interface ITextPattern2 : IAutomationPattern
    {
        IAutomationTextRange DocumentRange { get; }
        SupportedTextSelection SupportedTextSelection { get; }

        IEnumerable<IAutomationTextRange> GetSelection();
        IEnumerable<IAutomationTextRange> GetVisibleRanges();
        IAutomationTextRange RangeFromChild(IAutomationElement child);
        IAutomationTextRange RangeFromPoint(Point point);

        (IAutomationTextRange range, bool isActive) GetCaretRange();
        IAutomationTextRange RangeFromAnnotation(IAutomationElement annotation);
    }
}