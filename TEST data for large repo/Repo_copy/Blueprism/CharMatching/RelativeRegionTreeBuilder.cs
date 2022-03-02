using BluePrism.CharMatching.UI;
using System.Collections.Generic;

namespace BluePrism.CharMatching
{

    /// <summary>
    /// Used to build a relative Relative Region Tree for a specified region. A 
    /// relative region tree is every possible way of joining up a collection of 
    /// regions using the parent/child relationship defined by the RelativeParent
    /// property of a spy region.
    /// </summary>
    public class RelativeRegionTreeBuilder
    {
        /// <summary>
        /// The spy region the the builder will build the relative region tree for
        /// </summary>
        private readonly SpyRegion _region;

        /// <summary>
        /// The regions available to build the tree from
        /// </summary>
        private readonly ICollection<SpyRegion> _availableRegions;
        
        /// <summary>
        /// Creates a new instance of the builder, which is used to build a relative region tree
        /// for a specified region. All regions that can be connected to the specified region, using
        /// the SpyRegion relative parent property are added to a tree. The relative regions that 
        /// appear in the tree will be the region itself, all descendents and any parents/grandparents etc.
        /// This tree is then used to display relationships in the Reigon Editor.
        /// </summary>
        /// <param name="region">The spy region to build a relative region tree for</param>
        /// <param name="availableRegions">The regions available to build the tree from</param>
        public RelativeRegionTreeBuilder(SpyRegion region, ICollection<SpyRegion> availableRegions)
        {
            _region = region;
            _availableRegions = availableRegions;
        }

        /// <summary>
        /// Build a relative region tree, returning the root i.e. the top level anchor
        /// </summary>
        /// <returns>The root of the tree</returns>
        public RelativeRegionComposite Build()
        {
            var anchor = _region.GetTopLevelAnchor();
            var root = new RelativeRegionComposite(anchor);
            GenerateTree(root);
            return root;
        }

        /// <summary>
        /// For a specified node, add all tree descendents
        /// </summary>
        /// <param name="node">The node to add all descendents for</param>
        private void GenerateTree(RelativeRegionComposite node)
        {
            // Loop through all available regions, and find which regions are relative children of this node
            foreach (var region in _availableRegions)
            {
                if (region.RelativeParent == node.Region)
                {
                    // If the region is a child of this node then add it to the tree
                    var childNode = new RelativeRegionComposite(region);
                    node.Add(childNode);
                    // Add any children from the child node that was just added
                    GenerateTree(childNode);
                }
            }
        }               
    
    }
}
