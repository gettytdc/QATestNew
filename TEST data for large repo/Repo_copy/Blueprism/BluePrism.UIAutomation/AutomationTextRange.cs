namespace BluePrism.UIAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utilities.Functional;

    public class AutomationTextRange : IAutomationTextRange
    {
        private readonly IAutomationFactory _automationFactory;

        public UIAutomationClient.IUIAutomationTextRange TextRange { get; }

        public AutomationTextRange(
            UIAutomationClient.IUIAutomationTextRange textRange,
            IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
            TextRange = textRange;
        }

        public IAutomationTextRange Clone() =>
            TextRange.Clone()
            .Map(_automationFactory.TextRangeFromUIAutomationObject);

        public int Compare(IAutomationTextRange range) =>
            TextRange.Compare(range.TextRange);

        public int CompareEndpoints(
            TextPatternRangeEndpoint sourceEndPoint,
            IAutomationTextRange range,
            TextPatternRangeEndpoint targetEndPoint)
            =>
            TextRange.CompareEndpoints(
                (UIAutomationClient.TextPatternRangeEndpoint)sourceEndPoint,
                range.TextRange,
                (UIAutomationClient.TextPatternRangeEndpoint)targetEndPoint);

        public void ExpandToEnclosingUnit(TextUnit textUnit) =>
            TextRange.ExpandToEnclosingUnit((UIAutomationClient.TextUnit) textUnit);

        public IAutomationTextRange FindAttribute(int attribute, object value, bool backward) =>
            TextRange.FindAttribute(attribute, value, backward ? 1 : 0)
            .Map(_automationFactory.TextRangeFromUIAutomationObject);

        public IAutomationTextRange FindText(string text, bool backward, bool ignoreCase) =>
            TextRange.FindText(text, backward ? 1 : 0, ignoreCase ? 1 : 0)
            .Map(_automationFactory.TextRangeFromUIAutomationObject);

        public object GetAttributeValue(int attribute) =>
            TextRange.GetAttributeValue(attribute);

        public Array GetBoundingRectangles() =>
            TextRange.GetBoundingRectangles();

        public IAutomationElement GetEnclosingElement() =>
            TextRange.GetEnclosingElement()
            .Map(_automationFactory.FromUIAutomationElement);

        public string GetText(int maxLength) =>
            TextRange.GetText(maxLength);

        public int Move(TextUnit unit, int count) =>
            TextRange.Move((UIAutomationClient.TextUnit) unit, count);

        public int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count) =>
            TextRange.MoveEndpointByUnit(
                (UIAutomationClient.TextPatternRangeEndpoint) endpoint,
                (UIAutomationClient.TextUnit) unit,
                count);

        public void MoveEndpointByRange(
            TextPatternRangeEndpoint sourceEndPoint,
            IAutomationTextRange range,
            TextPatternRangeEndpoint targetEndPoint)
            =>
            TextRange.MoveEndpointByRange(
                (UIAutomationClient.TextPatternRangeEndpoint) sourceEndPoint,
                range.TextRange,
                (UIAutomationClient.TextPatternRangeEndpoint) targetEndPoint);

        public void Select() =>
            TextRange.Select();

        public void AddToSelection() =>
            TextRange.AddToSelection();

        public void RemoveFromSelection() =>
            TextRange.RemoveFromSelection();

        public void ScrollIntoView(bool alignToTop) =>
            TextRange.ScrollIntoView(alignToTop ? 1 : 0);

        public IEnumerable<IAutomationElement> GetChildren() =>
            TextRange.GetChildren()
            .ToEnumerable()
            .Select(_automationFactory.FromUIAutomationElement);
    }
}
