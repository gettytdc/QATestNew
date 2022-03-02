using BluePrism.CharMatching.UI;
using System.Collections.Generic;


namespace BluePrism.CharMatching
{
    /// <summary>
    /// An abstract component class, that defines a region within a relative region
    /// tree. A relative region tree descibes the relationship between spy regions 
    /// that are located based on where other regions are found.
    /// </summary>
    public abstract class RelativeRegionComponent
    {
        /// <summary>
        /// The component's underlying spy region
        /// </summary>
        public readonly SpyRegion Region;
                
        /// <summary>
        /// Creates a new instance of the relative region component class
        /// </summary>
        /// <param name="region">The underlying spy reigon</param>
        public RelativeRegionComponent(SpyRegion region)
        {
            this.Region = region;
        }

        /// <summary>
        /// If implemented, add a relative region to the children of this component
        /// </summary>
        /// <param name="c">The region to add as a child</param>
        public abstract void Add(RelativeRegionComponent c);

        /// <summary>
        /// Get a collection of lines that represents the relative relationships between all
        /// of this region's descendents (including this region itself). Each line
        /// is from the child's top left point to the parent's top left point.
        /// </summary>
        /// <returns>A collection of all of the lines defining relationships between this region
        /// and all of its descendents</returns>
        public abstract IEnumerable<Line2D> GetLines();

    }
}
