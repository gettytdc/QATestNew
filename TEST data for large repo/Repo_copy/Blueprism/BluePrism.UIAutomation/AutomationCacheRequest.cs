namespace BluePrism.UIAutomation
{

    using UIAutomationClient;

    /// <inheritdoc />
    public class AutomationCacheRequest : IAutomationCacheRequest
    {
        /// <inheritdoc />
        public AutomationElementMode AutomationElementMode
        {
            get => (AutomationElementMode)CacheRequest.AutomationElementMode;
            set => CacheRequest.AutomationElementMode = (UIAutomationClient.AutomationElementMode)value;
        }

        /// <inheritdoc />
        public IUIAutomationCacheRequest CacheRequest { get; }

        /// <inheritdoc />
        public AutomationCacheRequest(IUIAutomationCacheRequest cacheRequest)
        {
            CacheRequest = cacheRequest;
        }

        /// <inheritdoc />
        public void Add(PropertyType propertyType)
        {
            CacheRequest.AddProperty((int) propertyType);
        }
    }
}