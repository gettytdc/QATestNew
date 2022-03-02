namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// The methods used to locate a region within the application window 
    /// at run-time
    /// </summary>
    public enum RegionLocationMethod
    {
        /// <summary>
        /// The region is located in the application window using the exact 
        /// coordinates recorded in the region editor
        /// </summary>
        Coordinates,
        /// <summary>
        /// The region is located by searching in the application window for 
        /// the region's stored image
        /// </summary>
        Image
    }
}