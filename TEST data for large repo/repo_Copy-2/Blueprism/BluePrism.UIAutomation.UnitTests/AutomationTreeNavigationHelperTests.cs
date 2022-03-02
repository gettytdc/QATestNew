#if UNITTESTS

namespace BluePrism.UIAutomation.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Utilities.Functional;

    using Moq;

    using NUnit.Framework;
    using BluePrism.Utilities.Testing;
    using UIAutomationClient;

    internal class AutomationTreeNavigationHelperTests : UnitTestBase<AutomationTreeNavigationHelper>
    {

        [Test]
        public void GetTreeElement()
        {
            var getTreeMethod =
                typeof(AutomationTreeNavigationHelper).GetMethod("GetTree", BindingFlags.NonPublic | BindingFlags.Static);

            IEnumerable<IUIAutomationElement> GetTree(
                TreeScope scope,
                IUIAutomationTreeWalker treeWalker,
                IUIAutomationElement element,
                IUIAutomationCacheRequest cache)
                =>
                (IEnumerable<IUIAutomationElement>)
                getTreeMethod.Invoke(
                    ClassUnderTest,
                    new object[] { scope, treeWalker, element, cache });

            var treeScopeMock = GetMock<IUIAutomationTreeWalker>();

            var elementMock = GetMock<IUIAutomationElement>();
            elementMock
                .Setup(m => m.CurrentName)
                .Returns("TestElement1");

            var cacheMock = GetMock<IUIAutomationCacheRequest>();

            var result = GetTree(
                UIAutomationClient.TreeScope.TreeScope_Element,
                treeScopeMock.Object,
                elementMock.Object,
                cacheMock.Object);

            Assert.AreEqual("TestElement1", result.Single().CurrentName);
        }

        [Test]
        public void GetTreeAncestors()
        {
            var getTreeMethod =
                typeof(AutomationTreeNavigationHelper).GetMethod("GetTree", BindingFlags.NonPublic | BindingFlags.Static);

            IEnumerable<IUIAutomationElement> GetTree(
                TreeScope scope,
                IUIAutomationTreeWalker treeWalker,
                IUIAutomationElement element,
                IUIAutomationCacheRequest cache)
                =>
                    (IEnumerable<IUIAutomationElement>)
                    getTreeMethod.Invoke(
                        ClassUnderTest,
                        new object[] { scope, treeWalker, element, cache });

            Mock<IUIAutomationElement> GetElementMock(int id) =>
                new Mock<IUIAutomationElement>()
                .Tee(x => x
                    .Setup(m => m.CurrentName)
                    .Returns($"TestElement{id}"));

            var elementMocks =
                Enumerable.Range(1, 5)
                .Select(GetElementMock)
                .ToList();

            var treeScopeMock = GetMock<IUIAutomationTreeWalker>();
            treeScopeMock
                .SetupSequence(m => m.GetParentElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[1].Object)
                .Returns(elementMocks[2].Object)
                .Returns(elementMocks[3].Object)
                .Returns(elementMocks[4].Object)
                .Returns((IUIAutomationElement)null);

            var cacheMock = GetMock<IUIAutomationCacheRequest>();

            var result = GetTree(
                    UIAutomationClient.TreeScope.TreeScope_Ancestors,
                    treeScopeMock.Object,
                    elementMocks[0].Object,
                    cacheMock.Object)
                .ToList();

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(elementMocks[1].Object.CurrentName, result[0].CurrentName);
            Assert.AreEqual(elementMocks[2].Object.CurrentName, result[1].CurrentName);
            Assert.AreEqual(elementMocks[3].Object.CurrentName, result[2].CurrentName);
            Assert.AreEqual(elementMocks[4].Object.CurrentName, result[3].CurrentName);
        }

        [Test]
        public void GetTreeElementAndAncestors()
        {
            var getTreeMethod =
                typeof(AutomationTreeNavigationHelper).GetMethod("GetTree", BindingFlags.NonPublic | BindingFlags.Static);

            IEnumerable<IUIAutomationElement> GetTree(
                TreeScope scope,
                IUIAutomationTreeWalker treeWalker,
                IUIAutomationElement element,
                IUIAutomationCacheRequest cache)
                =>
                    (IEnumerable<IUIAutomationElement>)
                    getTreeMethod.Invoke(
                        ClassUnderTest,
                        new object[] { scope, treeWalker, element, cache });

            Mock<IUIAutomationElement> GetElementMock(int id) =>
                new Mock<IUIAutomationElement>()
                .Tee(x => x
                    .Setup(m => m.CurrentName)
                    .Returns($"TestElement{id}"));

            var elementMocks =
                Enumerable.Range(1, 5)
                .Select(GetElementMock)
                .ToList();

            var treeScopeMock = GetMock<IUIAutomationTreeWalker>();
            treeScopeMock
                .SetupSequence(m => m.GetParentElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[1].Object)
                .Returns(elementMocks[2].Object)
                .Returns(elementMocks[3].Object)
                .Returns(elementMocks[4].Object)
                .Returns((IUIAutomationElement)null);

            var cacheMock = GetMock<IUIAutomationCacheRequest>();

            var result = GetTree(
                    UIAutomationClient.TreeScope.TreeScope_Ancestors | UIAutomationClient.TreeScope.TreeScope_Element,
                    treeScopeMock.Object,
                    elementMocks[0].Object,
                    cacheMock.Object)
                .ToList();

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(elementMocks[0].Object.CurrentName, result[0].CurrentName);
            Assert.AreEqual(elementMocks[1].Object.CurrentName, result[1].CurrentName);
            Assert.AreEqual(elementMocks[2].Object.CurrentName, result[2].CurrentName);
            Assert.AreEqual(elementMocks[3].Object.CurrentName, result[3].CurrentName);
            Assert.AreEqual(elementMocks[4].Object.CurrentName, result[4].CurrentName);
        }

        [Test]
        public void GetTreeChildren()
        {
            var getTreeMethod =
                typeof(AutomationTreeNavigationHelper).GetMethod("GetTree", BindingFlags.NonPublic | BindingFlags.Static);

            IEnumerable<IUIAutomationElement> GetTree(
                TreeScope scope,
                IUIAutomationTreeWalker treeWalker,
                IUIAutomationElement element,
                IUIAutomationCacheRequest cache)
                =>
                    (IEnumerable<IUIAutomationElement>)
                    getTreeMethod.Invoke(
                        ClassUnderTest,
                        new object[] { scope, treeWalker, element, cache });

            Mock<IUIAutomationElement> GetElementMock(int id) =>
                new Mock<IUIAutomationElement>()
                .Tee(x => x
                    .Setup(m => m.CurrentName)
                    .Returns($"TestElement{id}"));

            var elementMocks =
                Enumerable.Range(1, 5)
                .Select(GetElementMock)
                .ToList();

            var treeScopeMock = GetMock<IUIAutomationTreeWalker>();
            treeScopeMock
                .Setup(m => m.GetFirstChildElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[1].Object);
            treeScopeMock
                .SetupSequence(m => m.GetNextSiblingElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[2].Object)
                .Returns(elementMocks[3].Object)
                .Returns(elementMocks[4].Object)
                .Returns((IUIAutomationElement)null);

            var cacheMock = GetMock<IUIAutomationCacheRequest>();

            var result = GetTree(
                    UIAutomationClient.TreeScope.TreeScope_Children,
                    treeScopeMock.Object,
                    elementMocks[0].Object,
                    cacheMock.Object)
                .ToList();

            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(elementMocks[1].Object.CurrentName, result[0].CurrentName);
            Assert.AreEqual(elementMocks[2].Object.CurrentName, result[1].CurrentName);
            Assert.AreEqual(elementMocks[3].Object.CurrentName, result[2].CurrentName);
            Assert.AreEqual(elementMocks[4].Object.CurrentName, result[3].CurrentName);
        }

        [Test]
        public void GetTreeDescendants()
        {
            var getTreeMethod =
                typeof(AutomationTreeNavigationHelper).GetMethod("GetTree", BindingFlags.NonPublic | BindingFlags.Static);

            IEnumerable<IUIAutomationElement> GetTree(
                TreeScope scope,
                IUIAutomationTreeWalker treeWalker,
                IUIAutomationElement element,
                IUIAutomationCacheRequest cache)
                =>
                    (IEnumerable<IUIAutomationElement>)
                    getTreeMethod.Invoke(
                        ClassUnderTest,
                        new object[] { scope, treeWalker, element, cache });

            Mock<IUIAutomationElement> GetElementMock(int id) =>
                new Mock<IUIAutomationElement>()
                .Tee(x => x
                    .Setup(m => m.CurrentName)
                    .Returns($"TestElement{id}"));

            var elementMocks =
                Enumerable.Range(1, 10)
                .Select(GetElementMock)
                .ToList();
            /*
                            0
                           / \
                          /   \
                         /     \
                        /       \
                        1       2
                       /|\     / \
                      / | \   /   \
                      3 4 5   6   7
                       / \
                       8 9
            */
            var treeScopeMock = GetMock<IUIAutomationTreeWalker>();
            treeScopeMock
                .SetupSequence(m => m.GetFirstChildElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[1].Object)
                .Returns(elementMocks[3].Object)
                .Returns(elementMocks[6].Object)
                .Returns((IUIAutomationElement)null)
                .Returns(elementMocks[8].Object)
                .Returns((IUIAutomationElement)null)
                .Returns((IUIAutomationElement)null)
                .Returns((IUIAutomationElement)null);

            treeScopeMock
                .SetupSequence(m => m.GetNextSiblingElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[2].Object)
                .Returns((IUIAutomationElement)null)
                .Returns(elementMocks[4].Object)
                .Returns(elementMocks[5].Object)
                .Returns((IUIAutomationElement)null)
                .Returns(elementMocks[7].Object)
                .Returns((IUIAutomationElement)null)
                .Returns(elementMocks[9].Object);

            var cacheMock = GetMock<IUIAutomationCacheRequest>();

            var result = GetTree(
                    UIAutomationClient.TreeScope.TreeScope_Descendants,
                    treeScopeMock.Object,
                    elementMocks[0].Object,
                    cacheMock.Object)
                .ToList();

            Assert.AreEqual(9, result.Count);
            Assert.AreEqual(elementMocks[1].Object.CurrentName, result[0].CurrentName);
            Assert.AreEqual(elementMocks[2].Object.CurrentName, result[1].CurrentName);
            Assert.AreEqual(elementMocks[3].Object.CurrentName, result[2].CurrentName);
            Assert.AreEqual(elementMocks[4].Object.CurrentName, result[3].CurrentName);
            Assert.AreEqual(elementMocks[5].Object.CurrentName, result[4].CurrentName);
            Assert.AreEqual(elementMocks[6].Object.CurrentName, result[5].CurrentName);
            Assert.AreEqual(elementMocks[7].Object.CurrentName, result[6].CurrentName);
        }

        [Test]
        public void GetTreeFullTree()
        {
            var getTreeMethod =
                typeof(AutomationTreeNavigationHelper).GetMethod("GetTree", BindingFlags.NonPublic | BindingFlags.Static);

            IEnumerable<IUIAutomationElement> GetTree(
                TreeScope scope,
                IUIAutomationTreeWalker treeWalker,
                IUIAutomationElement element,
                IUIAutomationCacheRequest cache)
                =>
                    (IEnumerable<IUIAutomationElement>)
                    getTreeMethod.Invoke(
                        ClassUnderTest,
                        new object[] { scope, treeWalker, element, cache });

            Mock<IUIAutomationElement> GetElementMock(int id) =>
                new Mock<IUIAutomationElement>()
                .Tee(x => x
                    .Setup(m => m.CurrentName)
                    .Returns($"TestElement{id}"));

            var elementMocks =
                Enumerable.Range(1, 11)
                .Select(GetElementMock)
                .ToList();
            /*
                            10
                            |
                            9
                            |
                            8
                            |
                            0
                           / \
                          /   \
                         /     \
                        /       \
                        1       2
                       /|\     / \
                      / | \   /   \
                      3 4 5   6   7
            */
            var treeScopeMock = GetMock<IUIAutomationTreeWalker>();
            treeScopeMock
                .SetupSequence(m => m.GetFirstChildElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[1].Object)
                .Returns(elementMocks[3].Object)
                .Returns(elementMocks[6].Object)
                .Returns((IUIAutomationElement)null)
                .Returns((IUIAutomationElement)null)
                .Returns((IUIAutomationElement)null)
                .Returns((IUIAutomationElement)null)
                .Returns((IUIAutomationElement)null);

            treeScopeMock
                .SetupSequence(m => m.GetNextSiblingElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[2].Object)
                .Returns((IUIAutomationElement)null)
                .Returns(elementMocks[4].Object)
                .Returns(elementMocks[5].Object)
                .Returns((IUIAutomationElement)null)
                .Returns(elementMocks[7].Object)
                .Returns((IUIAutomationElement)null);

            treeScopeMock
                .SetupSequence(m => m.GetParentElementBuildCache(
                    It.IsAny<IUIAutomationElement>(),
                    It.IsAny<IUIAutomationCacheRequest>()))
                .Returns(elementMocks[8].Object)
                .Returns(elementMocks[9].Object)
                .Returns(elementMocks[10].Object);

            var cacheMock = GetMock<IUIAutomationCacheRequest>();

            var result = GetTree(
                    UIAutomationClient.TreeScope.TreeScope_Descendants
                    | UIAutomationClient.TreeScope.TreeScope_Ancestors
                    | UIAutomationClient.TreeScope.TreeScope_Element,
                    treeScopeMock.Object,
                    elementMocks[0].Object,
                    cacheMock.Object)
                .ToList();

            Assert.AreEqual(11, result.Count);
            Assert.AreEqual(elementMocks[0].Object.CurrentName, result[0].CurrentName);
            Assert.AreEqual(elementMocks[1].Object.CurrentName, result[1].CurrentName);
            Assert.AreEqual(elementMocks[2].Object.CurrentName, result[2].CurrentName);
            Assert.AreEqual(elementMocks[3].Object.CurrentName, result[3].CurrentName);
            Assert.AreEqual(elementMocks[4].Object.CurrentName, result[4].CurrentName);
            Assert.AreEqual(elementMocks[5].Object.CurrentName, result[5].CurrentName);
            Assert.AreEqual(elementMocks[6].Object.CurrentName, result[6].CurrentName);
            Assert.AreEqual(elementMocks[7].Object.CurrentName, result[7].CurrentName);
            Assert.AreEqual(elementMocks[8].Object.CurrentName, result[8].CurrentName);
            Assert.AreEqual(elementMocks[9].Object.CurrentName, result[9].CurrentName);
            Assert.AreEqual(elementMocks[10].Object.CurrentName, result[10].CurrentName);
        }
    }
}
#endif