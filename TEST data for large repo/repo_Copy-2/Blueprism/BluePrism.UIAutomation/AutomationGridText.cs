using System;
using BluePrism.UIAutomation.Patterns;
using System.Linq;

namespace BluePrism.UIAutomation
{
    public class AutomationGridText : IAutomationGridText
    {
        ///<inheritdoc/>
        public string GetTextFromElement(IAutomationElement cellElement) =>
            GetText(cellElement);

        private string GetTextWithPattern<TPattern>(IAutomationElement cellElement, Func<TPattern, string> extractTextFunc)
            where TPattern : IAutomationPattern
        {
            var pattern = cellElement.GetCurrentPattern<TPattern>();
            return extractTextFunc(pattern);
        }

        private string GetText(IAutomationElement cellElement)
        {
            var result = default(string);

            if (cellElement.PatternIsSupported(PatternType.TextPattern))
            {
                // rich text support
                result =
                    GetTextWithPattern<ITextPattern>(
                        cellElement,
                        textPattern => textPattern.DocumentRange.GetText(int.MaxValue));
            }
            else if (cellElement.PatternIsSupported(PatternType.TogglePattern))
            {
                result =
                    GetTextWithPattern<ITogglePattern>(
                        cellElement,
                        togglePattern =>
                            (togglePattern.CurrentToggleState != ToggleState.Off)
                            .ToString());
            }
            else if (cellElement.PatternIsSupported(PatternType.TextChildPattern))
            {
                // Supported by HTML table cells.
                result =
                    GetTextWithPattern<ITextChildPattern>(
                        cellElement,
                        textChildPattern => textChildPattern.TextRange.GetText(int.MaxValue));
            }
            else if (cellElement.PatternIsSupported(PatternType.SelectionPattern) ||
                     cellElement.PatternIsSupported(PatternType.SelectionItemPattern))
            {
                // Supported by comboboxes in table cells
                if (
                    cellElement.PatternIsSupported(PatternType.SelectionItemPattern)
                    && !cellElement.PatternIsSupported(PatternType.SelectionPattern))
                {
                    cellElement = 
                        cellElement.GetCurrentPattern<ISelectionItemPattern>()
                        ?.CurrentSelectionContainer 
                        ?? cellElement;
                }

                result = GetTextWithPattern<ISelectionPattern>(
                    cellElement,
                    selectPattern =>
                        selectPattern?.GetCurrentSelection().SingleOrDefault()?.CurrentName ?? string.Empty);
            }
            else if (cellElement.PatternIsSupported(PatternType.ValuePattern))
            {
                // Supported by Excel cells.
                result =
                    GetTextWithPattern<IValuePattern>(
                        cellElement,
                        valuePattern => valuePattern.CurrentValue);
            }
            else // If all other patterns aren't supported IAutomationElement.CurrentName can return text from the cell.
            {
                result = cellElement.CurrentName;
            }

            return result;
        }
    }
}
