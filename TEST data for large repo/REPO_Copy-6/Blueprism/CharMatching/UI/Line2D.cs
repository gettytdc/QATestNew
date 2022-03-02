using System.Drawing;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Simple class that allows you to store a pair of points that define the start
    /// and end points of a 2D line.
    /// </summary>
    public class Line2D
    {
        /// <summary>
        /// Create a new instance of <see cref="Line2D"/>
        /// </summary>
        /// <param name="start">The Start point of the line</param>
        /// <param name="end">The End point of the line</param>
        public Line2D(Point start, Point end)
        {
            StartPoint = start;
            EndPoint = end;
        }

        /// <summary>
        /// The start point of the line
        /// </summary>
        public Point StartPoint { get; private set; }
        
        /// <summary>
        /// The end point of the line
        /// </summary>
        public Point EndPoint { get; private set; }
        
        /// <summary>
        /// Override the equals function. Returns true if botht the start and end points
        /// are equal
        /// </summary>
        public override bool Equals(object obj)
        {
            var item = obj as Line2D;
            if (item == null) return false;

            return this.StartPoint.Equals(item.StartPoint) && this.EndPoint.Equals(item.EndPoint);
        }

        /// <summary>
        /// Gets an integer hash of this object, by xoring the integer hash of the 
        /// the line's start and end points
        /// </summary>
        public override int GetHashCode()
        {
            return StartPoint.GetHashCode() ^ EndPoint.GetHashCode();
        }

    }
}
