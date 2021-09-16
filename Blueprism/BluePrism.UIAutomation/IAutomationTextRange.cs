namespace BluePrism.UIAutomation
{
    using System;
    using System.Collections.Generic;

    public interface IAutomationTextRange
    {
        UIAutomationClient.IUIAutomationTextRange TextRange { get; }

        IAutomationTextRange Clone();

        int Compare(IAutomationTextRange range);

        int CompareEndpoints(TextPatternRangeEndpoint sourceEndPoint, IAutomationTextRange range, TextPatternRangeEndpoint targetEndPoint);

        void ExpandToEnclosingUnit(TextUnit textUnit);

        IAutomationTextRange FindAttribute(int attribute, object value, bool backward);

        IAutomationTextRange FindText(string text, bool backward, bool ignoreCase);

        object GetAttributeValue(int attribute);

        Array GetBoundingRectangles();

        IAutomationElement GetEnclosingElement();

        string GetText(int maxLength);

        int Move(TextUnit unit, int count);

        int MoveEndpointByUnit(TextPatternRangeEndpoint endpoint, TextUnit unit, int count);

        void MoveEndpointByRange(TextPatternRangeEndpoint sourceEndPoint, IAutomationTextRange range, TextPatternRangeEndpoint targetEndPoint);

        void Select();

        void AddToSelection();

        void RemoveFromSelection();

        void ScrollIntoView(bool alignToTop);

        IEnumerable<IAutomationElement> GetChildren();
    }
}