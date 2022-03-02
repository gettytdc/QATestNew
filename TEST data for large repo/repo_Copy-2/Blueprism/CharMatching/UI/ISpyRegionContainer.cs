using System;
using System.Collections.Generic;
using System.Drawing;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Interface describing a spy region container. Typically this will just be
    /// a <see cref="RegionMapper"/>, but all the things that a SpyRegion requires
    /// are separated here so that they can be stubbed and tested in unit tests.
    /// </summary>
    public interface ISpyRegionContainer
    {
        /// <summary>
        /// Gets a unique region name, using the specified format string as a
        /// base. The format string should contain a placeholder for an integer,
        /// using the format described by <see cref="String.Format"/> (ie. 
        /// "{0}" - eg. "Region {0}"). The placeholder will be replaced with an
        /// integer until an unused name is found.
        /// </summary>
        /// <param name="formatString">The format string for the region name.
        /// </param>
        /// <returns>A region name unique within this container derived from
        /// the given format string.</returns>
        string GetUniqueRegionName(string nameFormat);

        /// <summary>
        /// Gets a bitmap that represents the entirety of the container's image
        /// </summary>
        Bitmap Bitmap { get; }

        /// <summary>
        /// The currently active region in this container, or null if no region
        /// is active. Active regions are those which are currently being moved
        /// or resized. Note that an active region may or may not be in the
        /// <see cref="SpyRegions"/> collection - if a region is currently being
        /// created, it is active, but not yet confirmed in the container.
        /// </summary>
        SpyRegion ActiveRegion { get; set; }

        /// <summary>
        /// The currently selected region in this container.
        /// </summary>
        SpyRegion SelectedRegion { get; set; }

        /// <summary>
        /// The region currently being hovered over in this container - only
        /// one region may be set as the 'hover' region - typically this would
        /// be the topmost region (ie. the last one placed) if one region
        /// overlaps another, but the only requirement of this interface is
        /// that it is consistent - ie. deterministic. If, at a particular
        /// location, one region is returned as the hover region, then, assuming
        /// no changes to the regions, that region should always be returned as
        /// the hover region at that location.
        /// </summary>
        SpyRegion HoverRegion { get; }

        /// <summary>
        /// The collection of confirmed spy regions in this container - a region
        /// which is currently being created may not appear in this container
        /// until it is confirmed - ie. its initial size / location is set.
        /// </summary>
        ICollection<SpyRegion> SpyRegions { get; }

        /// <summary>
        /// Finds the guides relating to the given region, applies them to this
        /// container and returns them.
        /// </summary>
        /// <param name="reg">The region for which guides should be found and
        /// applied.</param>
        /// <returns>The map of guides set in this region mapper, derived from
        /// the given region.</returns>
        IDictionary<GuideCheck, Guide> ApplyGuides(SpyRegion spyRegion);

        /// <summary>
        /// A readonly collection of the names of the installed fonts available
        /// to the spy region container and its regions.
        /// </summary>
        ICollection<string> InstalledFontNames { get; }

        /// <summary>
        /// Creates an empty font with the given name 
        /// </summary>
        /// <param name="name">The name of the font to be created</param>
        /// <exception cref="AlreadyExistsException">If a font with the given name
        /// already exists</exception>
        void CreateEmptyFont(string name);

        /// <summary>
        /// Gets the font store using which fonts can be loaded and saved
        /// </summary>
        IFontStore Store { get; }

    }

}
