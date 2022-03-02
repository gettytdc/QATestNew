using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.UIAutomation.Patterns
{
    [RepresentsPatternType(PatternType.TextEditPattern)]
    public interface ITextEditPattern : IAutomationPattern
    {
        IAutomationTextRange DocumentRange { get; }
        SupportedTextSelection SupportedTextSelection { get; }

        IAutomationTextRange GetActiveComposition();
        IAutomationTextRange GetConversionTarget();
        IEnumerable<IAutomationTextRange> GetSelection();
        IEnumerable<IAutomationTextRange> GetVisibleRanges();
        IAutomationTextRange RangeFromChild(IAutomationElement child);
        IAutomationTextRange RangeFromPoint(Point point);
    }
}