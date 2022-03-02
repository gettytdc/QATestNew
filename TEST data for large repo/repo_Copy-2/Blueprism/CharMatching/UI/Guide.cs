namespace BluePrism.CharMatching.UI
{

    /// <summary>
    /// Class representing a guide - an x or y value indicating a guide
    /// to which a region can be aligned.
    /// </summary>
    public abstract class Guide
    {
        // X value of this guide - 0 indicates no vertical guide
        private int _x;
        // Y value of this guide - 0 indicates no horizontal guide
        private int _y;

        /// <summary>
        /// Creates a new guide at the given location.
        /// </summary>
        /// <param name="x">The vertical location of the guide, 0 if none</param>
        /// <param name="y">The horizontal location of the guide, 0 if none</param>
        public Guide(int x, int y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// The X co-ordinate for this guide - ie. the value of the vertical
        /// aspect of this guide. 0 indicates no vertical aspect
        /// </summary>
        public int X { get { return _x; } }

        /// <summary>
        /// The Y co-ordinate for this guide - ie. the value of the horizontal
        /// aspect of this guide. 0 indicates no horizontal aspect
        /// </summary>
        public int Y { get { return _y; } }
    }

    /// <summary>
    /// A horizontal guide set at a specific y co-ordinate.
    /// </summary>
    internal class HorizontalGuide: Guide
    {
        /// <summary>
        /// Creates a new horizontal guide at the given y co-ordinate
        /// </summary>
        /// <param name="y">The vertical offset at which the guide
        /// should be set</param>
        public HorizontalGuide(int y) : base(0, y) { }
    }

    /// <summary>
    /// A vertical guide set at a specific x co-ordinate.
    /// </summary>
    internal class VerticalGuide: Guide
    {
        /// <summary>
        /// Creates a new vertical guide at the given x co-ordinate
        /// </summary>
        /// <param name="x">The horizontal offset at which the guide
        /// should be set</param>
        public VerticalGuide(int x) : base(x, 0) { }
    }
}
