using BluePrism.CharMatching.UI;
using System.Collections.Generic;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// A composite class, used to store a region as well as its children 
    /// </summary>
    public class RelativeRegionComposite : RelativeRegionComponent
    {
        /// <summary>
        /// A list of this relative regions children i.e. those regions whose location
        /// is relative to this region.
        /// </summary>
        public List<RelativeRegionComponent> Children = new List<RelativeRegionComponent>();
        
        /// <summary>
        /// Creates a new instance of the relative region composite, which is deferred
        /// to the component class.
        /// </summary>
        /// <param name="region">The underlying SpyRegion</param>
        public RelativeRegionComposite(SpyRegion region) : base(region){}

        /// <summary>
        /// Add a relative region to the children of this region
        /// </summary>
        /// <param name="c">The region to add as a child</param>
        public override void Add(RelativeRegionComponent c)
        {
            Children.Add(c);
        }

        /// <summary>
        /// Get a collection of lines that represent the relative relationships between all
        /// of this region's descendents (including this region itself). Each line
        /// is from the child's top left point to the parent's top left point.
        /// </summary>
        /// <returns>A collection of all of the lines defining relationships between this region
        /// and all of its descendents</returns>
        public override IEnumerable<Line2D> GetLines()
        {
            foreach (RelativeRegionComponent child in Children)
            {
                yield return new Line2D(child.Region.Rectangle.Location, Region.Rectangle.Location);
                foreach (var line in child.GetLines())
                {
                    yield return line;
                };
            }
        }
        
    }
}
