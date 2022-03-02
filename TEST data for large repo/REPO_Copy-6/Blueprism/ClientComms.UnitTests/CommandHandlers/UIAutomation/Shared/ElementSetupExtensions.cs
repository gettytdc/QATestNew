using System.Collections.Generic;
using System.Linq;
using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using Moq;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared
{

    /// <summary>
    /// Utility functionality for working with mock IAutomationElement objects
    /// </summary>
    internal static class ElementMockExtensions
    {

        /// <summary>
        /// Sets up a Mock IAutomationElement object to return a particular pattern
        /// from the GetCurrentPattern(Of T) function. In addition, the mock element
        /// is set up to return true when PatternIsSupported is called with the
        /// specified PatternType.
        /// </summary>
        /// <typeparam name="TPattern">The type of pattern to set up</typeparam>
        /// <param name="elementMock">The mock element object</param>
        /// <param name="patternType">The type of pattern that is supported</param>
        /// <returns>The mock pattern object</returns>
        public static Mock<TPattern> MockPattern<TPattern>(this Mock<IAutomationElement> elementMock, PatternType patternType) where TPattern : class, IAutomationPattern

        {
            elementMock.Setup(e => e.PatternIsSupported(patternType)).Returns(true);
            return elementMock.MockPattern<TPattern>();
        }

        /// <summary>
        /// Sets up a Mock IAutomationElement object to return a particular pattern
        /// from the GetCurrentPattern(Of T) function.
        /// </summary>
        /// <typeparam name="TPattern">The type of pattern to set up</typeparam>
        /// <param name="elementMock">The mock IAutomationElement object</param>
        /// <returns>The mock object created for the pattern</returns>
        public static Mock<TPattern> MockPattern<TPattern>(this Mock<IAutomationElement> elementMock) where TPattern : class, IAutomationPattern

        {
            var patternMock = new Mock<TPattern>();
            elementMock.Setup(e => e.GetCurrentPattern<TPattern>()).Returns(patternMock.Object);
            return patternMock;
        }

        /// <summary>
        /// Sets up a mock IAutomationElement object to return a sequence of
        /// parent element mocks when FindAll is called with Ancestor scope.
        /// The first element in the sequence will also be returned if FindAll
        /// is called with Parent scope.
        /// </summary>
        /// <param name="elementMock">The mock IAutomationElement object</param>
        /// <param name="ancestorMocks">The mock IAutomationElement objects to include
        /// in the sequence returned by FindAll. The first object in this sequence will
        /// be set up as the main element's immediate parent.</param>
        public static void MockAncestors(this Mock<IAutomationElement> elementMock, params Mock<IAutomationElement>[] ancestorMocks)
        {
            var ancestorElements = new List<IAutomationElement>();
            ancestorElements.AddRange(ancestorMocks.Select(c => c.Object));
            elementMock.Setup(e => e.FindAll(TreeScope.Ancestors)).Returns(ancestorElements);
            var parentElements = new List<IAutomationElement>();
            if (ancestorMocks.Any())
            {
                parentElements.Add(ancestorMocks[0].Object);
            }

            elementMock.Setup(e => e.FindAll(TreeScope.Parent)).Returns(parentElements);
        }

        /// <summary>
        /// Sets up a mock IAutomationElement object to return a sequence of
        /// child element mocks when FindAll is called with Children or Descendants
        /// scope. Also sets up the mock to include itself in the sequence when
        /// FindAll is called with Subtree scope.
        /// </summary>
        /// <param name="elementMock">The mock IAutomationElement object</param>
        /// <param name="childMocks">The mock IAutomationElement objects to include
        /// in the sequence returned by FindAll</param>
        public static void MockChildren(this Mock<IAutomationElement> elementMock, params Mock<IAutomationElement>[] childMocks)
        {
            var childElements = new List<IAutomationElement>();
            childElements.AddRange(childMocks.Select(c => c.Object));
            elementMock.Setup(e => e.FindAll(TreeScope.Children)).Returns(childElements);
            elementMock.Setup(e => e.FindAll(TreeScope.Descendants)).Returns(childElements);
            var subtreeElements = new List<IAutomationElement>();
            subtreeElements.Add(elementMock.Object);
            subtreeElements.AddRange(childMocks.Select(c => c.Object));
            elementMock.Setup(e => e.FindAll(TreeScope.Subtree)).Returns(subtreeElements);
        }
    }
}