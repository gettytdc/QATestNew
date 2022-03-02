using System.ComponentModel;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Defines where a region's stored image is searched for within the containing 
    /// element at run-time
    /// </summary>
    public enum RegionPosition
    {
        /// <summary>
        /// The element is searched for at the position stored against the element
        /// (the image search padding extends the area searched)
        /// </summary>
        [Description("Fixed")]
        Fixed,
        /// <summary>
        /// The region is searched for at the position relative to another element
        /// (the image search padding extends the area searched)
        /// </summary>
        [Description("Relative")]
        Relative,
        /// <summary>
        /// The entire containing element is searched for the region's stored image
        /// </summary>
        [Description("Anywhere (in window)")]
        Anywhere        
    }
}