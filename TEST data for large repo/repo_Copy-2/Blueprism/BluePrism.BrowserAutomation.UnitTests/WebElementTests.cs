#if UNITTESTS

using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.BrowserAutomation.UnitTests
{
    [TestFixture]
    public class WebElementTests : UnitTestBase<WebElement>
    {
        protected override WebElement TestClassConstructor()
        {
            return new WebElement(Guid.NewGuid(), GetMock<IWebPage>().Object);
        }
        
        [Test]
        public void ClickElement_ClicksElementOnPage()
        {
            var wasInvoked = false;
            Action invoke = () => wasInvoked = true;

            GetMock<IWebPage>()
                .Setup(m => m.ClickElement(ClassUnderTest))
                .Callback(invoke);

            ClassUnderTest.Click();

            wasInvoked.Should().BeTrue();
        }

        [Test]
        public void DoubleClickElement_DoubleClicksElementOnPage()
        {
            var wasInvoked = false;
            Action invoke = () => wasInvoked = true;

            GetMock<IWebPage>()
                .Setup(m => m.DoubleClickElement(ClassUnderTest))
                .Callback(invoke);

            ClassUnderTest.DoubleClick();

            wasInvoked.Should().BeTrue();
        }

        [Test]
        public void FocusElement_FocusesElementOnPage()
        {
            var wasInvoked = false;
            Action invoke = () => wasInvoked = true;

            GetMock<IWebPage>()
                .Setup(m => m.FocusElement(ClassUnderTest))
                .Callback(invoke);

            ClassUnderTest.Focus();

            wasInvoked.Should().BeTrue();
        }

        [Test]
        public void HoverElement_HoversElementOnPage()
        {
            var wasInvoked = false;
            Action invoke = () => wasInvoked = true;

            GetMock<IWebPage>()
                .Setup(m => m.HoverElement(ClassUnderTest))
                .Callback(invoke);

            ClassUnderTest.Hover();

            wasInvoked.Should().BeTrue();
        }

        [Test]
        public void GetClientBoundsOfElement_GetsClientBoundsOfElementOnPage()
        {
            var bounds = new Rectangle(10,20,100,150);

            GetMock<IWebPage>()
                .Setup(m => m.GetElementClientBounds(ClassUnderTest))
                .Returns(bounds);

            ClassUnderTest.GetClientBounds().Should().Be(bounds);
        }

        [Test]
        public void GetOffsetBoundsOfElement_GetsOffsetBoundsOfElementOnPage()
        {
            var bounds = new Rectangle(10, 20, 100, 150);

            GetMock<IWebPage>()
                .Setup(m => m.GetElementOffsetBounds(ClassUnderTest))
                .Returns(bounds);

            ClassUnderTest.GetOffsetBounds().Should().Be(bounds);
        }

        [Test]
        public void GetScrollBoundsOfElement_GetsScrollBoundsOfElementOnPage()
        {
            var bounds = new Rectangle(10, 20, 100, 150);

            GetMock<IWebPage>()
                .Setup(m => m.GetElementScrollBounds(ClassUnderTest))
                .Returns(bounds);

            ClassUnderTest.GetScrollBounds().Should().Be(bounds);
        }

        [Test]
        public void GetBoundsOfElement_GetsBoundsOfElementOnPage()
        {
            var bounds = new Rectangle(10, 20, 100, 150);

            GetMock<IWebPage>()
                .Setup(m => m.GetElementBounds(ClassUnderTest))
                .Returns(bounds);

            ClassUnderTest.GetBounds().Should().Be(bounds);
        }

        [Test]
        public void GetChildCountOfElement_GetsChildCountOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementChildCount(ClassUnderTest))
                .Returns(5);

            ClassUnderTest.GetChildCount().Should().Be(5);
        }

        [Test]
        public void GetColumnCountOfElement_GetsColumnCountOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetColumnCount(ClassUnderTest, 0))
                .Returns(5);

            ClassUnderTest.GetColumnCount(0).Should().Be(5);
        }

        [Test]
        public void GetRowCountOfElement_GetsRowCountOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetRowCount(ClassUnderTest))
                .Returns(5);

            ClassUnderTest.GetRowCount().Should().Be(5);
        }

        [Test]
        public void GetElementIsEditable_GetsIsEditableOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementIsEditable(ClassUnderTest))
                .Returns(true);

            ClassUnderTest.GetIsEditable().Should().BeTrue();
        }

        [Test]
        public void GetElementIsChecked_GetsIsCheckedOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetCheckedState(ClassUnderTest))
                .Returns(true);

            ClassUnderTest.GetCheckedState().Should().BeTrue();
        }

        [Test]
        public void GetElementIsVisible_GetsIsVisibleOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementIsVisible(ClassUnderTest))
                .Returns(true);

            ClassUnderTest.GetIsVisible().Should().BeTrue();
        }

        [Test]
        public void GetElementIsOnScreen_GetsIsOnScreenOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementIsOnScreen(ClassUnderTest))
                .Returns(true);

            ClassUnderTest.GetIsOnScreen().Should().BeTrue();
        }        

        [Test]
        public void GetTableItemText_GetsTableItemTextOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetTableItemText(ClassUnderTest, 3,4))
                .Returns("table item");

            GetMock<IWebPage>()
                .Setup(m => m.GetTableItemText(ClassUnderTest, 2, 5))
                .Returns("other table item");

            ClassUnderTest.GetTableItemText(3, 4).Should().Be("table item");
        }

        [Test]
        public void GetStyle_GetsStyleofElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementStyle(ClassUnderTest))
                .Returns("the style");

            ClassUnderTest.GetStyle().Should().Be("the style");
        }

        [Test]
        public void GetSelectedText_GetsSelectedTextOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetSelectedText(ClassUnderTest))
                .Returns("the text");

            ClassUnderTest.GetSelectedText().Should().Be("the text");
        }

        [Test]
        public void SelectsTheText_SelectsTheTextOfElementOnPage()
        {
            var selectedText = "";
            GetMock<IWebPage>()
                .Setup(m => m.SelectTextRange(ClassUnderTest, 1, 2))
                .Callback(() => selectedText = "the text");

            ClassUnderTest.SelectTextRange(1, 2);
                selectedText.Should().Be("the text");
        }

        [Test]
        public void GetsTabIndex_GetsTheTabIndexOfElementOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementTabIndex(ClassUnderTest))
                .Returns(() => 2);

            ClassUnderTest.GetTabIndex().Should().Be(2);
        }

        [Test]
        public void Highlight_HighlightsElementOnPage()
        {
            Color color = Color.White;

            GetMock<IWebPage>()
                .Setup(m => m.HighlightElement(ClassUnderTest, It.IsAny<Color>())).Callback<IWebElement, Color>((_, theColor) => color = theColor);

            ClassUnderTest.Highlight(Color.Blue);
            color.Should().Be(Color.Blue);
        }

        [Test]
        public void GetElementId_GetsElementIdOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementId(ClassUnderTest))
                .Returns("thisIsMyId");

            ClassUnderTest.GetElementId().Should().Be("thisIsMyId");
        }

        [Test]
        public void GetName_GetsElementNamwOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementName(ClassUnderTest))
                .Returns("thisIsMyName");

            ClassUnderTest.GetName().Should().Be("thisIsMyName");
        }

        [Test]
        public void GetElementLinkAddressText_GetsAddressOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetLinkAddressText(ClassUnderTest))
                .Returns("thisIsMyAddress");

            ClassUnderTest.GetLinkAddressText().Should().Be("thisIsMyAddress");
        }

        [Test]
        public void GetElementType_GetsElementTypeOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementType(ClassUnderTest))
                .Returns("type");

            ClassUnderTest.GetElementType().Should().Be("type");
        }

        [Test]
        public void GetElementClass_GetsElementClassOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementClass(ClassUnderTest))
                .Returns("class");

            ClassUnderTest.GetClass().Should().Be("class");
        }

        [Test]
        public void GetElementPath_GetsElementPathOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementPath(ClassUnderTest))
                .Returns("path");

            ClassUnderTest.GetPath().Should().Be("path");
        }

        [Test]
        public void GetElementValue_GetsElementValueOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementValue(ClassUnderTest))
                .Returns("value");

            ClassUnderTest.GetValue().Should().Be("value");
        }

        [Test]
        public void GetText_GetsElementTextOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementText(ClassUnderTest))
                .Returns("text");

            ClassUnderTest.GetText().Should().Be("text");
        }

        [Test]
        public void GetHtml_GetsElementHtmlOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementHtml(ClassUnderTest))
                .Returns("<h1>test</h1>");

            ClassUnderTest.GetHtml().Should().Be("<h1>test</h1>");
        }

        [Test]
        public void GetLabel_GetsElementLabelOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementLabel(ClassUnderTest))
                .Returns("label");

            ClassUnderTest.GetLabel().Should().Be("label");
        }

        [Test]
        public void GetAttribute_GetsElementAttributeOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementAttribute(ClassUnderTest, "attribute1"))
                .Returns("text");

            GetMock<IWebPage>()
                .Setup(m => m.GetElementAttribute(ClassUnderTest, "attribute2"))
                .Returns("different");

            ClassUnderTest.GetAttribute("attribute1").Should().Be("text");
        }

        [Test]
        public void GetAccessKey_GetsElementAccessKeyOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetElementAccessKey(ClassUnderTest))
                .Returns("key");

            ClassUnderTest.GetAccessKey().Should().Be("key");
        }

        [Test]
        public void GetDescendents_GetsElementDescendentsOnPage()
        {
            var elements = new List<IWebElement> { Moq.Mock.Of<IWebElement>(), Moq.Mock.Of<IWebElement>() }.AsReadOnly();

            GetMock<IWebPage>()
                .Setup(m => m.GetElementDescendants(ClassUnderTest))
                .Returns(elements);

            ClassUnderTest.GetDescendants().Should().BeEquivalentTo(elements);
        }

        [Test]
        public void SetValue_SetsValueOnPage()
        {
            var value = "";
            GetMock<IWebPage>()
                .Setup(m => m.SetElementValue(ClassUnderTest, It.IsAny<string>()))
                .Callback<IWebElement, string>((_, newValue) => value = newValue);

            ClassUnderTest.SetValue("value");

            value.Should().Be("value");
        }

        [Test]
        public void SetChecked_SetsCheckedOnPage()
        {
            var value = false;
            GetMock<IWebPage>()
                .Setup(m => m.SetCheckedState(ClassUnderTest, It.IsAny<bool>()))
                .Callback<IWebElement, bool>((_, newValue) => value = newValue);

            ClassUnderTest.SetCheckedState(true);

            value.Should().BeTrue();
        }

        [Test]
        public void SetAttribute_SetsAttributeOnPage()
        {
            var value = "";
            GetMock<IWebPage>()
                .Setup(m => m.SetAttribute(ClassUnderTest, "attr", It.IsAny<string>()))
                .Callback<IWebElement, string, string>((_, __, newValue) => value = newValue);

            ClassUnderTest.SetAttribute("attr", "test");

            value.Should().Be("test");
        }

        [Test]
        public void Select_SelectsElementOnPage()
        {
            var isInvoked = false;
            GetMock<IWebPage>()
                .Setup(m => m.SelectElement(ClassUnderTest))
                .Callback(() => isInvoked = true);

            ClassUnderTest.Select();

            isInvoked.Should().BeTrue();
        }

        [Test]
        public void ScrollTo_ScrollsToElementOnPage()
        {
            var isInvoked = false;
            GetMock<IWebPage>()
                .Setup(m => m.ScrollToElement(ClassUnderTest))
                .Callback(() => isInvoked = true);

            ClassUnderTest.ScrollTo();

            isInvoked.Should().BeTrue();
        }

        [Test]
        public void Submit_SubmitsElementOnPage()
        {
            var isInvoked = false;
            GetMock<IWebPage>()
                .Setup(m => m.SubmitElement(ClassUnderTest))
                .Callback(() => isInvoked = true);

            ClassUnderTest.Submit();

            isInvoked.Should().BeTrue();
        }

        [Test]
        public void SelectListItem_SelectsListItemElementOnPage()
        {
            var isInvoked = false;
            GetMock<IWebPage>()
                .Setup(m => m.SelectListItem(ClassUnderTest, 2, "fred"))
                .Callback(() => isInvoked = true);

            ClassUnderTest.SelectListItem(2, "fred");

            isInvoked.Should().BeTrue();
        }

        [Test]
        public void AddToSelection_AddElementToSelectionOnPage()
        {
            var isInvoked = false;
            GetMock<IWebPage>()
                .Setup(m => m.AddToListSelection(ClassUnderTest, 2, "fred"))
                .Callback(() => isInvoked = true);

            ClassUnderTest.AddToListSelection(2, "fred");

            isInvoked.Should().BeTrue();
        }

        [Test]
        public void RemoveFromListSelection_RemoveElementFromListSelectionOnPage()
        {
            var isInvoked = false;
            GetMock<IWebPage>()
                .Setup(m => m.RemoveFromListSelection(ClassUnderTest, 2, "fred"))
                .Callback(() => isInvoked = true);

            ClassUnderTest.RemoveFromListSelection(2, "fred");

            isInvoked.Should().BeTrue();
        }

        [Test]
        public void GetListItems_GetsListItemsOnPage()
        {
            var collection = new List<Data.ListItem> { new Data.ListItem(), new Data.ListItem()}.AsReadOnly();

            GetMock<IWebPage>()
                .Setup(m => m.GetListItems(ClassUnderTest))
                .Returns(collection);

            ClassUnderTest.GetListItems().Should().BeEquivalentTo(collection);
        }

        [Test]
        public void GetSliderRange_GetSliderRangeOnPage()
        {
            var slider  = new Data.SliderRange { Minimum = 5, Maximum = 10 };

            GetMock<IWebPage>()
                .Setup(m => m.GetSliderRange(ClassUnderTest))
                .Returns(slider);

            ClassUnderTest.GetSliderRange().Should().Be(slider);
        }

        [Test]
        public void GetIsListItemSelectede_GetIsListItemSelectedOnPage()
        {
            GetMock<IWebPage>()
                .Setup(m => m.GetIsListItemSelected(ClassUnderTest))
                .Returns(true);

            ClassUnderTest.GetIsListItemSelected().Should().BeTrue();
        }

        [Test]
        public void ElementsAreEqual_IdIsSame()
        {
            var id = Guid.NewGuid();
            var firstElement = new WebElement(id, GetMock<IWebPage>().Object);
            var secondElement = new WebElement(id, GetMock<IWebPage>().Object);

            firstElement.Equals(secondElement).Should().BeTrue();
            secondElement.Equals(firstElement).Should().BeTrue();
        }

        [Test]
        public void ElementsAreNotEqual_IdIsDifferent()
        {
            var firstElement = new WebElement(Guid.NewGuid(), GetMock<IWebPage>().Object);
            var secondElement = new WebElement(Guid.NewGuid(), GetMock<IWebPage>().Object);

            firstElement.Equals(secondElement).Should().BeFalse();
            secondElement.Equals(firstElement).Should().BeFalse();
        }

        [Test]
        public void ElementsAreNotEqual_IsNull()
        {
            var element = new WebElement(Guid.NewGuid(), GetMock<IWebPage>().Object);
            element.Equals(null).Should().BeFalse();
        }
    }
}

#endif
