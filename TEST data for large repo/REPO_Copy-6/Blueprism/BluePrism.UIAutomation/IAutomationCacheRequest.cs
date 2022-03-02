namespace BluePrism.UIAutomation
{
    /// <summary>
    /// Provides methods for handling UIA cache requests
    /// </summary>
    public interface IAutomationCacheRequest
    {
        /// <summary>
        /// Gets or sets the mode to use for caching element information
        /// </summary>
        AutomationElementMode AutomationElementMode { get; set; }

        /// <summary>
        /// Gets the current cache request for this thread
        /// </summary>
        UIAutomationClient.IUIAutomationCacheRequest CacheRequest { get; }

        /// <summary>
        /// Adds the requested property to the list of properties to cache
        /// </summary>
        /// <param name="propertyType">The property to cache</param>
        void Add(PropertyType propertyType);
    }
}