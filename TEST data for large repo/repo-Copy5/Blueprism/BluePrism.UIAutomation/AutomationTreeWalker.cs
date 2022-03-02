namespace BluePrism.UIAutomation
{
    using Utilities.Functional;

    /// <inheritdoc />
    public class AutomationTreeWalker : IAutomationTreeWalker
    {
        /// <summary>
        /// The automation factory
        /// </summary>
        private readonly IAutomationFactory _automationFactory;

        /// <inheritdoc />
        public UIAutomationClient.IUIAutomationTreeWalker TreeWalker { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationTreeWalker" /> class.
        /// </summary>
        /// <param name="treeWalker">The base UIA tree walker.</param>
        /// <param name="automationFactory">The automation factory.</param>
        /// <param name="patternFactory">The pattern factory.</param>
        public AutomationTreeWalker(
            UIAutomationClient.IUIAutomationTreeWalker treeWalker,
            IAutomationFactory automationFactory)
        {
            TreeWalker = treeWalker;
            _automationFactory = automationFactory;
        }

        /// <inheritdoc />
        public IAutomationElement GetParent(IAutomationElement element) =>
            TreeWalker.GetParentElement(element.Element)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetFirstChild(IAutomationElement element) =>
            TreeWalker.GetFirstChildElement(element.Element)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetLastChild(IAutomationElement element) =>
            TreeWalker.GetLastChildElement(element.Element)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetNextSibling(IAutomationElement element) =>
            TreeWalker.GetNextSiblingElement(element.Element)
                ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetPreviousSibling(IAutomationElement element) =>
            TreeWalker.GetPreviousSiblingElement(element.Element)
                ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement Normalize(IAutomationElement element) =>
            TreeWalker.NormalizeElement(element.Element)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetParent(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            TreeWalker.GetParentElementBuildCache(element.Element, cacheRequest.CacheRequest)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetFirstChild(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            TreeWalker.GetFirstChildElementBuildCache(element.Element, cacheRequest.CacheRequest)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetLastChild(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            TreeWalker.GetLastChildElementBuildCache(element.Element, cacheRequest.CacheRequest)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetNextSibling(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            TreeWalker.GetNextSiblingElementBuildCache(element.Element, cacheRequest.CacheRequest)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetPreviousSibling(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            TreeWalker.GetPreviousSiblingElementBuildCache(element.Element, cacheRequest.CacheRequest)
            ?.Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement Normalize(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            TreeWalker.NormalizeElementBuildCache(element.Element, cacheRequest.CacheRequest)
            ?.Map(_automationFactory.FromUIAutomationElement);

    }
}
