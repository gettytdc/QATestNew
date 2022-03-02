using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.ApplicationManager.BrowserAutomation;
using BluePrism.BrowserAutomation;
using BluePrism.BrowserAutomation.NativeMessaging;
using BluePrism.Server.Domain.Models;
using BluePrism.Utilities.Testing;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace ClientComms.UnitTests.ClientComms.UnitTests
{
    [TestFixture]
    public class BrowserAutomationIdentifierHelperTests : UnitTestBase<BrowserAutomationIdentifierHelper>
    {
        private static readonly Rectangle BlankBounds = new Rectangle(-1, -1, 0, 0);

        [Test]
        public void GetIdentifiersReturnsExpectedFixedIdentifiers()
        {
            var webPageMock = GetMock<IWebPage>();
            var webElementMock = GetMock<IWebElement>();
            webElementMock.Setup<Rectangle>(m => m.GetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetClientBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetOffsetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetScrollBounds()).Returns(BlankBounds);
            webElementMock.Setup<string>(m => m.GetElementId()).Returns(string.Empty);
            webElementMock.Setup<string>(m => m.GetName()).Returns(string.Empty);
            webElementMock.SetupGet<IWebPage>(m => m.Page).Returns(webPageMock.Object);
            var result = ClassUnderTest.GetIdentifiers(GetMock<IWebElement>().Object);
            var expectedIdentifiers = new[] { clsQuery.IdentifierTypes.wElementType, clsQuery.IdentifierTypes.wXPath, clsQuery.IdentifierTypes.wValue, clsQuery.IdentifierTypes.wPageAddress, clsQuery.IdentifierTypes.wClass, clsQuery.IdentifierTypes.wChildCount, clsQuery.IdentifierTypes.wIsEditable, clsQuery.IdentifierTypes.wStyle, clsQuery.IdentifierTypes.wTabIndex, clsQuery.IdentifierTypes.wInputType }.Select(x => $"{x}");
            expectedIdentifiers.All(x => result.Any(y => y.Contains(x))).Should().BeTrue();
        }

        [Test]
        [TestCaseSource(nameof(GetGetIdentifiersTestCases))]
        public void GetIdentifiersAppliesExpectedValues(GetIdentifiersTestCase testCase)
        {
            var webPageMock = GetMock<IWebPage>();
            var webElementMock = GetMock<IWebElement>();
            testCase.SetupMock(webElementMock, webPageMock);
            webElementMock.Setup<Rectangle>(m => m.GetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetClientBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetOffsetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetScrollBounds()).Returns(BlankBounds);
            webElementMock.Setup<string>(m => m.GetElementId()).Returns(string.Empty);
            webElementMock.Setup<string>(m => m.GetName()).Returns(string.Empty);
            webElementMock.SetupGet<IWebPage>(m => m.Page).Returns(webPageMock.Object);
            var result = ClassUnderTest.GetIdentifiers(webElementMock.Object);
            result.Where(x => x.Contains(testCase.IdentifierType.ToString()) && x.Contains(testCase.ExpectedValueString)).Count().Should().Be(1);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void GetIdentifiersAddsIdIfNotEmpty(bool addId)
        {
            var webPageMock = GetMock<IWebPage>();
            var webElementMock = GetMock<IWebElement>();
            webElementMock.Setup<Rectangle>(m => m.GetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetClientBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetOffsetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetScrollBounds()).Returns(BlankBounds);
            webElementMock.Setup<string>(m => m.GetElementId()).Returns(addId ? "Test" : string.Empty);
            webElementMock.Setup<string>(m => m.GetName()).Returns(string.Empty);
            webElementMock.SetupGet<IWebPage>(m => m.Page).Returns(webPageMock.Object);
            var result = ClassUnderTest.GetIdentifiers(webElementMock.Object);
            result.Any(x => x.Contains("wId")).Should().Be(addId);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void GetIdentifiersAddsNameIfNotEmpty(bool addName)
        {
            var webPageMock = GetMock<IWebPage>();
            var webElementMock = GetMock<IWebElement>();
            webElementMock.Setup<Rectangle>(m => m.GetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetClientBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetOffsetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetScrollBounds()).Returns(BlankBounds);
            webElementMock.Setup<string>(m => m.GetElementId()).Returns(string.Empty);
            webElementMock.Setup<string>(m => m.GetName()).Returns(addName ? "Test" : string.Empty);
            webElementMock.SetupGet<IWebPage>(m => m.Page).Returns(webPageMock.Object);
            var result = ClassUnderTest.GetIdentifiers(webElementMock.Object);
            result.Any(x => x.Contains("wName")).Should().Be(addName);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void GetIdentifiersAddsBoundsObjectsWhenNotEmpty(bool addBounds)
        {
            var webPageMock = GetMock<IWebPage>();
            var bounds = new Rectangle(1, 2, 3, 4);
            var webElementMock = GetMock<IWebElement>();
            webElementMock.Setup<Rectangle>(m => m.GetBounds()).Returns(addBounds ? bounds : BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetClientBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetOffsetBounds()).Returns(BlankBounds);
            webElementMock.Setup<Rectangle>(m => m.GetScrollBounds()).Returns(BlankBounds);
            webElementMock.Setup<string>(m => m.GetElementId()).Returns(string.Empty);
            webElementMock.Setup<string>(m => m.GetName()).Returns(string.Empty);
            webElementMock.SetupGet<IWebPage>(m => m.Page).Returns(webPageMock.Object);
            var result = ClassUnderTest.GetIdentifiers(webElementMock.Object);
            result.Any(x => x.Contains("wX=")).Should().Be(addBounds);
        }

        [Test]
        [TestCaseSource(nameof(GetFindElementsTestCases))]
        public void FindElementsReturnsExpectedElements(FindElementsTestCase testCase)
        {
            var webPageMock = GetMock<IWebPage>();
            testCase.SetupMock(webPageMock);
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse(testCase.Query);
            var result = ClassUnderTest.FindElements(query);
            result.Any().Should().Be(testCase.ShouldMatch);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void FindElementsReturnsExpectedElementsByXPathWithEquals(bool shouldMatch)
        {
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<IWebElement>(m => m.GetElementByPath(It.IsAny<string>())).Returns(shouldMatch ? GetMock<IWebElement>().Object : null);
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse("Test wXPath=Test");
            var result = ClassUnderTest.FindElements(query);
            result.Any().Should().Be(shouldMatch);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void FindElementsReturnsExpectedElementsByXPathWithWildcard(bool shouldMatch)
        {
            var webElementMock = GetMock<IWebElement>();
            webElementMock.Setup<IReadOnlyCollection<IWebElement>>(m => m.GetDescendants()).Returns(new[] { GetMock<IWebElement>().Object });
            webElementMock.Setup<string>(m => m.GetPath()).Returns("Test/Test");
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<IWebElement>(m => m.GetElementByPath(It.IsAny<string>())).Returns(shouldMatch ? webElementMock.Object : null);
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse("Test wXPath%=Te*/Test");
            var result = ClassUnderTest.FindElements(query);
            result.Any().Should().Be(shouldMatch);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void FindElementsReturnsExpectedElementsByXPathWithRegEx(bool shouldMatch)
        {
            var webElementMock = GetMock<IWebElement>();
            webElementMock.Setup<IReadOnlyCollection<IWebElement>>(m => m.GetDescendants()).Returns(new[] { GetMock<IWebElement>().Object });
            webElementMock.Setup<string>(m => m.GetPath()).Returns(shouldMatch ? "Test" : "ThisDoesn'tMatch");
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<IWebElement>(m => m.GetRootElement()).Returns(webElementMock.Object);
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse("Test wXPath$=T.{2}t");
            var result = ClassUnderTest.FindElements(query);
            result.Any().Should().Be(shouldMatch);
        }

        [Test]
        public void FindElementsReturnsCorrectElementByMatchIndex()
        {
            var normalElementMock = GetMock<IWebElement>();
            var markedElementMock = new Mock<IWebElement>();
            markedElementMock.Setup(m => m.GetName()).Returns("Marker");
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<IReadOnlyCollection<IWebElement>>(m => m.GetElementsByType(It.IsAny<string>())).Returns(new[] { normalElementMock.Object, normalElementMock.Object, markedElementMock.Object, normalElementMock.Object });
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse("Test wElementType=Test matchIndex=3");
            string result = ClassUnderTest.FindElements(query).Single().GetName();
            result.Should().Be("Marker");
        }

        [Test]
        public void FindElementsReturnsNoElementsWhenMatchIndexInvalid()
        {
            var elementMock = GetMock<IWebElement>().Object;
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<IReadOnlyCollection<IWebElement>>(m => m.GetElementsByType(It.IsAny<string>())).Returns(Enumerable.Repeat(elementMock, 3).ToArray());
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse("Test wElementType=Test matchIndex=4");
            var result = ClassUnderTest.FindElements(query);
            result.Should().BeEmpty();
        }

        [Test]
        public void FindSingleElementReturnsSingleElement()
        {
            var elementMock = GetMock<IWebElement>().Object;
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<IReadOnlyCollection<IWebElement>>(m => m.GetElementsByType(It.IsAny<string>())).Returns(Enumerable.Repeat(elementMock, 3).ToArray());
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse("Test wElementType=Test matchIndex=1");
            var result = ClassUnderTest.FindSingleElement(query);
            result.Should().NotBeNull();
        }

        [Test]
        public void FindSingleElementThrowsExceptionsOnMultipleResults()
        {
            var elementMock = GetMock<IWebElement>().Object;
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<IReadOnlyCollection<IWebElement>>(m => m.GetElementsByType(It.IsAny<string>())).Returns(Enumerable.Repeat(elementMock, 3).ToArray());
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse("Test wElementType=Test");
            void test() => ClassUnderTest.FindSingleElement(query);
            Assert.Throws<TooManyElementsException>(test);
        }

        [Test]
        public void FindSingleElementThrowsExceptionsOnNoResults()
        {
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<IReadOnlyCollection<IWebElement>>(m => m.GetElementsByType(It.IsAny<string>())).Returns(new IWebElement[] { });
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var query = clsQuery.Parse("Test wElementType=Test");
            void test() => ClassUnderTest.FindSingleElement(query);
            Assert.Throws<NoSuchElementException>(test);
        }

        [Test]
        [TestCaseSource(nameof(FiltersFilterAsExpectedTestCases))]
        public void FiltersFilterAsExpected((string queryString, Action<Mock<IWebElement>> mockSetup) args)
        {
            var webElementMock = GetMock<IWebElement>();
            webElementMock.Setup<IReadOnlyCollection<IWebElement>>(m => m.GetDescendants()).Returns(new[] { GetMock<IWebElement>().Object });
            webElementMock.Setup<string>(m => m.GetPath()).Returns("Test");
            args.mockSetup(webElementMock);
            var query = clsQuery.Parse(args.queryString + " wXPath%=*");
            var webPageMock = GetMock<IWebPage>();
            webPageMock.Setup<string>(m => m.GetAddress()).Returns("This is a test");
            webPageMock.Setup<IWebElement>(m => m.GetElementByPath(It.IsAny<string>())).Returns(webElementMock.Object);
            var webPageProviderMock = GetMock<IWebPageProvider>();
            webPageProviderMock.Setup<IReadOnlyCollection<IWebPage>>(m => m.GetActiveWebPages("")).Returns(new[] { webPageMock.Object });
            var result = ClassUnderTest.FindSingleElement(query);
            result.Should().NotBeNull();
        }

        [Test]
        public void GetActiveWebPagesWithTrackingIdReturnsCorrectPages()
        {
            //We need to mock some webpages
            var webPage1 = new NativeMessagingWebPage(Guid.NewGuid());
            var webPage2 = new NativeMessagingWebPage(Guid.NewGuid());
            var webPage3 = new NativeMessagingWebPage(Guid.NewGuid());
            var webPage4 = new NativeMessagingWebPage(Guid.NewGuid());
            var webPage5 = new NativeMessagingWebPage(Guid.NewGuid());
            var webPage6 = new NativeMessagingWebPage(Guid.NewGuid());
            var webPage7 = new NativeMessagingWebPage(Guid.NewGuid());
            var webPage8 = new NativeMessagingWebPage(Guid.NewGuid());
            var webPage9 = new NativeMessagingWebPage(Guid.NewGuid());

            var activeWebPages = new ConcurrentDictionary<Guid, IWebPage>();
            activeWebPages[webPage1.Id] = webPage1;
            activeWebPages[webPage2.Id] = webPage2;
            activeWebPages[webPage3.Id] = webPage3;
            activeWebPages[webPage4.Id] = webPage4;
            activeWebPages[webPage5.Id] = webPage5;
            activeWebPages[webPage6.Id] = webPage6;
            activeWebPages[webPage7.Id] = webPage7;
            activeWebPages[webPage8.Id] = webPage8;
            activeWebPages[webPage9.Id] = webPage9;
            //Now we need some tracking Ids

            var trackingId1 = Guid.NewGuid().ToString();
            var trackingId2 = Guid.NewGuid().ToString();


            var trackedPages = new ConcurrentDictionary<string, List<Guid>>();
            trackedPages[trackingId1] = new List<Guid>() { webPage1.Id, webPage2.Id, webPage3.Id };
            trackedPages[trackingId2] = new List<Guid>() { webPage4.Id, webPage5.Id, webPage6.Id };

            var webPageProvider = new NativeMessagingWebPageProvider(activeWebPages, trackedPages);

            var result = webPageProvider.GetActiveWebPages(trackingId1).ToList();
            var expected = new List<IWebPage>() { webPage1, webPage2, webPage3, webPage7, webPage8, webPage9 };
            CollectionAssert.AreEquivalent(expected, result);

            
            var result2 = webPageProvider.GetActiveWebPages(trackingId2).ToList();
            var expected2 = new List<IWebPage>() { webPage4, webPage5, webPage6, webPage7, webPage8, webPage9 };
            CollectionAssert.AreEquivalent(expected2, result2);
        }

        private static IEnumerable<(string queryString, Action<Mock<IWebElement>> elementMockSetup)> FiltersFilterAsExpectedTestCases()
        {
            return new []
            { 
                new { Id = "wId",    Setup = (Action<Mock<IWebElement>>) (x => x.Setup(m => m.GetElementId()).Returns("Test")) },
                new { Id = "wClass", Setup = (Action<Mock<IWebElement>>) (x => x.Setup(m => m.GetClass()).Returns("Test")) },
                new { Id = "wElementType", Setup = (Action<Mock<IWebElement>>)(x => x.Setup(m => m.GetElementType()).Returns("Test"))} 
            }
            .SelectMany(x => new[] { ($"Test {x.Id}=Test", x.Setup), ($"Test {x.Id}%=T*t", x.Setup), ($"Test {x.Id}$=T[es]{{2}}t", x.Setup) });
        }

        private static IEnumerable<GetIdentifiersTestCase> GetGetIdentifiersTestCases()
        {
            var webPageMock = new Mock<IWebPage>();
            webPageMock.Setup(m => m.GetAddress()).Returns("Test");
            return new[] { new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetElementType()).Returns("Test"), clsQuery.IdentifierTypes.wElementType, "Test"), new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetPath()).Returns("Test"), clsQuery.IdentifierTypes.wXPath, "Test"), new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetValue()).Returns("Test"), clsQuery.IdentifierTypes.wValue, "Test"), new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetClass()).Returns("Test"), clsQuery.IdentifierTypes.wClass, "Test"), new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetChildCount()).Returns(123), clsQuery.IdentifierTypes.wChildCount, "123"), new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetIsEditable()).Returns(true), clsQuery.IdentifierTypes.wIsEditable, "True"), new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetStyle()).Returns("Test"), clsQuery.IdentifierTypes.wStyle, "Test"), new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetTabIndex()).Returns(123), clsQuery.IdentifierTypes.wTabIndex, "123"), new GetIdentifiersTestCase((e, p) => e.Setup(m => m.GetAttribute(It.Is<string>(x => x.Equals("type")))).Returns("Test"), clsQuery.IdentifierTypes.wInputType, "Test"), new GetIdentifiersTestCase((e, p) => p.Setup(m => m.GetAddress()).Returns("Test"), clsQuery.IdentifierTypes.wPageAddress, "Test") };
        }

        private static IEnumerable<FindElementsTestCase> GetFindElementsTestCases()
        {
            Action<Mock<IWebPage>> prepareElementMock(Action<Mock<IWebElement>> setupElementMock) => (pageMock) =>
                                    {
                                        var elementMock = new Mock<IWebElement>();
                                        setupElementMock(elementMock);
                                        elementMock.Setup(m => m.GetDescendants()).Returns(new[] { elementMock.Object });
                                        pageMock.Setup(m => m.GetRootElement()).Returns(elementMock.Object);
                                    };
            return new[] { new FindElementsTestCase("Test wClass=Test", p => p.Setup(m => m.GetElementsByClass(It.IsAny<string>())).Returns(new[] { new Mock<IWebElement>().Object }), true), new FindElementsTestCase("Test wClass=Test", p => p.Setup(m => m.GetElementsByClass(It.IsAny<string>())).Returns(new IWebElement[] { }), false), new FindElementsTestCase("Test wElementType=Test", p => p.Setup(m => m.GetElementsByType(It.IsAny<string>())).Returns(new[] { new Mock<IWebElement>().Object }), true), new FindElementsTestCase("Test wElementType=Test", p => p.Setup(m => m.GetElementsByType(It.IsAny<string>())).Returns(new IWebElement[] { }), false), new FindElementsTestCase("Test wClientX=12", prepareElementMock(e => e.Setup(m => m.GetClientBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wClientX=12", prepareElementMock(e => e.Setup(m => m.GetClientBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wClientY=12", prepareElementMock(e => e.Setup(m => m.GetClientBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wClientY=12", prepareElementMock(e => e.Setup(m => m.GetClientBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wClientWidth=12", prepareElementMock(e => e.Setup(m => m.GetClientBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wClientWidth=12", prepareElementMock(e => e.Setup(m => m.GetClientBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wClientHeight=12", prepareElementMock(e => e.Setup(m => m.GetClientBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wClientHeight=12", prepareElementMock(e => e.Setup(m => m.GetClientBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wOffsetX=12", prepareElementMock(e => e.Setup(m => m.GetOffsetBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wOffsetX=12", prepareElementMock(e => e.Setup(m => m.GetOffsetBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wOffsetY=12", prepareElementMock(e => e.Setup(m => m.GetOffsetBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wOffsetY=12", prepareElementMock(e => e.Setup(m => m.GetOffsetBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wOffsetWidth=12", prepareElementMock(e => e.Setup(m => m.GetOffsetBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wOffsetWidth=12", prepareElementMock(e => e.Setup(m => m.GetOffsetBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wOffsetHeight=12", prepareElementMock(e => e.Setup(m => m.GetOffsetBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wOffsetHeight=12", prepareElementMock(e => e.Setup(m => m.GetOffsetBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wScrollX=12", prepareElementMock(e => e.Setup(m => m.GetScrollBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wScrollX=12", prepareElementMock(e => e.Setup(m => m.GetScrollBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wScrollY=12", prepareElementMock(e => e.Setup(m => m.GetScrollBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wScrollY=12", prepareElementMock(e => e.Setup(m => m.GetScrollBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wScrollWidth=12", prepareElementMock(e => e.Setup(m => m.GetScrollBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wScrollWidth=12", prepareElementMock(e => e.Setup(m => m.GetScrollBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wScrollHeight=12", prepareElementMock(e => e.Setup(m => m.GetScrollBounds()).Returns(new Rectangle(12, 12, 12, 12))), true), new FindElementsTestCase("Test wScrollHeight=12", prepareElementMock(e => e.Setup(m => m.GetScrollBounds()).Returns(new Rectangle(-1, -1, 0, 0))), false), new FindElementsTestCase("Test wChildCount=1", prepareElementMock(e => e.Setup(m => m.GetChildCount()).Returns(1)), true), new FindElementsTestCase("Test wChildCount=1", prepareElementMock(e => e.Setup(m => m.GetChildCount()).Returns(0)), false), new FindElementsTestCase("Test wIsEditable=True", prepareElementMock(e => e.Setup(m => m.GetIsEditable()).Returns(true)), true), new FindElementsTestCase("Test wIsEditable=True", prepareElementMock(e => e.Setup(m => m.GetIsEditable()).Returns(false)), false), new FindElementsTestCase("Test wStyle=Test", prepareElementMock(e => e.Setup(m => m.GetStyle()).Returns("Test")), true), new FindElementsTestCase("Test wStyle=Test", prepareElementMock(e => e.Setup(m => m.GetStyle()).Returns("Doesn'tMatch")), false), new FindElementsTestCase("Test wTabIndex=1", prepareElementMock(e => e.Setup(m => m.GetTabIndex()).Returns(1)), true), new FindElementsTestCase("Test wTabIndex=1", prepareElementMock(e => e.Setup(m => m.GetTabIndex()).Returns(0)), false) };
        }

        public class GetIdentifiersTestCase
        {
            public readonly Action<Mock<IWebElement>, Mock<IWebPage>> SetupMock;
            public readonly clsQuery.IdentifierTypes IdentifierType;
            public readonly string ExpectedValueString;

            public GetIdentifiersTestCase(Action<Mock<IWebElement>, Mock<IWebPage>> setupMock, clsQuery.IdentifierTypes identifierType, string expectedValueString)
            {
                SetupMock = setupMock;
                IdentifierType = identifierType;
                ExpectedValueString = expectedValueString;
            }

            public override string ToString()
            {
                return $"{IdentifierType}={ExpectedValueString}";
            }
        }

        public class FindElementsTestCase
        {
            public readonly string Query;
            public readonly Action<Mock<IWebPage>> SetupMock;
            public readonly bool ShouldMatch;

            public FindElementsTestCase(string query, Action<Mock<IWebPage>> setupMock, bool shouldMatch)
            {
                Query = query;
                SetupMock = setupMock;
                ShouldMatch = shouldMatch;
            }
        }
    }
}
