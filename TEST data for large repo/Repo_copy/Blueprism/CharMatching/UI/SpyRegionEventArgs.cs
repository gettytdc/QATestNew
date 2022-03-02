using System;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Delegate to implement a listener for the SpyRegion events
    /// </summary>
    public delegate void SpyRegionEventHandler(object sender, SpyRegionEventArgs e);

    /// <summary>
    /// Event args detailing an event on a spy region
    /// </summary>
    public class SpyRegionEventArgs : EventArgs
    {
        // The affected region
        private SpyRegion _region;

        /// <summary>
        /// Creates a new event args object indicating the given region
        /// </summary>
        /// <param name="reg">The region on which the event occurred</param>
        public SpyRegionEventArgs(SpyRegion reg)
        {
            _region = reg;
        }

        /// <summary>
        /// The region on which the event occurred
        /// </summary>
        public SpyRegion Region { get { return _region; } }
    }
}
