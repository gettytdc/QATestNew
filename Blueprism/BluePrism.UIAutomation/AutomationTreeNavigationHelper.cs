using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BluePrism.UIAutomation.Conditions;

namespace BluePrism.UIAutomation
{
    /// <inheritDoc/>
    internal class AutomationTreeNavigationHelper : IAutomationTreeNavigationHelper
    {
        private readonly IAutomationFactory _automationFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationTreeNavigationHelper"/> class.
        /// </summary>
        /// <param name="automationFactory">The automation factory.</param>
        public AutomationTreeNavigationHelper(
            IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
        }

        /// <inheritDoc/>
        public IEnumerable<IAutomationElement> FindWithTreeWalker(
            IAutomationElement element,
            TreeScope scope,
            IAutomationCondition condition,
            IAutomationCacheRequest cacheRequest)
        {
            var treeCondition =
                condition.IsCustomCondition
                    ? _automationFactory.CreateTrueCondition()
                    : condition;

            var treeWalker = _automationFactory.CreateTreeWalker(treeCondition);

            var tree = 
                GetTree(
                    (UIAutomationClient.TreeScope)scope,
                    treeWalker.TreeWalker,
                    element.Element,
                    cacheRequest.CacheRequest)
                .Select(_automationFactory.FromUIAutomationElement)
                .Where(x => condition.Evaluate(x, cacheRequest));

            return tree;
        }

        /// <summary>
        /// Gets the children of a particular tree node.
        /// </summary>
        /// <param name="treeWalker">The tree walker.</param>
        /// <param name="element">The element.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="withDescendants">
        /// If set to <c>true</c>, all of the element's descendants will be returned.
        /// </param>
        /// <returns>An enumeration which can be used to walk the element's subtree</returns>
        private static IEnumerable<UIAutomationClient.IUIAutomationElement> GetTreeNodeChildren(
            UIAutomationClient.IUIAutomationTreeWalker treeWalker,
            UIAutomationClient.IUIAutomationElement element,
            UIAutomationClient.IUIAutomationCacheRequest cache,
            bool withDescendants)
        {
            var children = new List<UIAutomationClient.IUIAutomationElement>();

            UIAutomationClient.IUIAutomationElement child;

            try
            {
                child = treeWalker.GetFirstChildElementBuildCache(element, cache);
            }
            catch (COMException)
            {
                // If we get a COM exception getting children then we'll just assume
                // that there are no children. This fixes an issue where tables in IE11
                // on Windows 7 were throwing a COM exception (US-1578)
                child = null;
            }

            while (child != null)
            {
                children.Add(child);
                yield return child;

                child = treeWalker.GetNextSiblingElementBuildCache(child, cache);
            }

            if (withDescendants)
            {
                while (children.Count > 0)
                {
                    var addedChildren = new List<UIAutomationClient.IUIAutomationElement>();
                    var descendants =
                        children.SelectMany(x => GetTreeNodeChildren(treeWalker, x, cache, false));

                    foreach (var descendant in descendants)
                    {
                        addedChildren.Add(descendant);
                        yield return descendant;
                    }

                    children = addedChildren;
                }
            }
        }

        /// <summary>
        /// Gets the tree of elements in the specified scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="treeWalker">The tree walker.</param>
        /// <param name="element">The element.</param>
        /// <param name="cache">The cache.</param>
        /// <returns>An enumeration which can be used to walk the tree</returns>
        private static IEnumerable<UIAutomationClient.IUIAutomationElement> GetTree(
            UIAutomationClient.TreeScope scope,
            UIAutomationClient.IUIAutomationTreeWalker treeWalker,
            UIAutomationClient.IUIAutomationElement element,
            UIAutomationClient.IUIAutomationCacheRequest cache)
        {
            if (element == null)
                yield break;

            if ((scope & UIAutomationClient.TreeScope.TreeScope_Element) > 0)
                yield return element;

            if (
                (scope & UIAutomationClient.TreeScope.TreeScope_Children) > 0
                || (scope & UIAutomationClient.TreeScope.TreeScope_Descendants) > 0)
            {
                var children =
                    GetTreeNodeChildren(
                        treeWalker,
                        element,
                        cache,
                        (scope & UIAutomationClient.TreeScope.TreeScope_Descendants) > 0);

                foreach (var child in children)
                    yield return child;
            }

            if (
                (scope & UIAutomationClient.TreeScope.TreeScope_Parent) > 0
                && (scope & UIAutomationClient.TreeScope.TreeScope_Ancestors) == 0)
            {
                var parent = treeWalker.GetParentElementBuildCache(element, cache);

                if (parent != null)
                    yield return parent;
            }

            if ((scope & UIAutomationClient.TreeScope.TreeScope_Ancestors) > 0)
            {
                var ancestors =
                    GetTree(
                        UIAutomationClient.TreeScope.TreeScope_Ancestors |
                        UIAutomationClient.TreeScope.TreeScope_Element,
                        treeWalker,
                        treeWalker.GetParentElementBuildCache(element, cache),
                        cache);

                foreach (var ancestor in ancestors)
                    yield return ancestor;
            }
        }
    }
}