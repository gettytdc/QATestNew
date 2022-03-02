using System;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Attribute that can be applied to specify the sort order of property values.
    /// i.e. how they are displayed within a property window.
    /// This is currently only used by <see cref="SpyRegion"/> but could be moved
    /// to a shared location if needed to be used by other properties.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SortOrderAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="SortOrderAttribute"/>, that allows you to specify
        /// the order that a property will appear within the property window
        /// </summary>
        /// <param name="order">The sort order of the property</param>
        public SortOrderAttribute(int order)
        {
            SortOrder = order;
        }
        
        /// <summary>
        /// The sort order value
        /// </summary>
        public int SortOrder { get; private set; }

    }
}
