using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.TextPattern)]
    public interface ITextPattern : IAutomationPattern
    {
        IAutomationTextRange DocumentRange { get; }
        SupportedTextSelection SupportedTextSelection { get; }

        IEnumerable<IAutomationTextRange> GetSelection();
        IEnumerable<IAutomationTextRange> GetVisibleRanges();
        IAutomationTextRange RangeFromChild(IAutomationElement child);
        IAutomationTextRange RangeFromPoint(Point point);
    }
}