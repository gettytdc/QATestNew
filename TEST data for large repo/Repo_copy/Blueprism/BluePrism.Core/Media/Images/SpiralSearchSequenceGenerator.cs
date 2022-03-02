using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BluePrism.Core.Media.Images
{
    /// <summary>
    /// Class that can be used to generates a sequence of points that make up a
    /// specified rectangle. The sequence starts at a specified starting point, and 
    /// then spirals out until every point that makes up the rectangle is returned.
    /// Note that the rectangle's top left corner is denoted by (0, 0).
    /// </summary>
    internal class SpiralSearchSequenceGenerator
    {
        /// <summary>
        /// The point at which to start the spiral
        /// </summary>
        private readonly Point _startPoint;

        /// <summary>
        /// The point where the spiral is currently at. Note: this may fall outside
        /// of the bounds of the rectangle.
        /// </summary>
        private Point _currentPoint;

        /// <summary>
        /// The direction that the spiral is currently moving.
        /// </summary>
        private SpiralDirection _currentDirection;

        /// <summary>
        /// The area within which points should be generated
        /// </summary>
        private Rectangle _container;

        /// <summary>
        /// Used to indicate the direction the spiral is travelling in
        /// </summary>
        private enum SpiralDirection
        {
            Down = 0,
            Right = 1,
            Up = 2,
            Left = 3
        }
        
        /// <summary>
        /// Create a new instance of the Spiral Search Sequence Generator, which
        /// creates a sequence of points, starting at a specified point within a 
        /// rectangle, and spirals around around until all points within the bounds 
        /// of the rectangle are returned. The rectangles top-left pointr is denoted 
        /// by (0, 0) and the dimensions of the rectangle are passed in as parameters.
        /// </summary>
        /// <param name="width">The width of the rectangle to provide the sequence for</param>
        /// <param name="height">The height of the rectangle to provide the sequence for</param>
        /// <param name="start">The point at which to start the spiral</param>
        public SpiralSearchSequenceGenerator(int width, int height, Point start): this (new Rectangle(0, 0, width, height), start)
        {
        }

        /// <summary>
        /// Create a new instance of the Spiral Search Sequence Generator, which
        /// creates a sequence of points, starting at a specified point within a 
        /// rectangle, and spirals around around until all points within the bounds 
        /// of the rectangle are returned. The rectangles top-left pointr is denoted 
        /// by (0, 0) and the dimensions of the rectangle are passed in as parameters.
        /// </summary>
        /// <param name="container">A rectangle representing the area within which points should be
        /// generated</param>
        /// <param name="start">The point at which to start the spiral</param>
        public SpiralSearchSequenceGenerator(Rectangle container, Point start)
        {
            _container = container;
            _startPoint = start;
        }

        /// <summary>
        /// Get every a sequence of every point that makes up a rectangle, starting 
        /// the specified starting point and spiralling outwards until all points
        /// are returned.
        /// </summary>
        /// <returns>A spiralled sequence that allows you to navigate all of the 
        /// points that make up a rectangle.
        /// </returns>
        public IEnumerable<Point> GetSequence()
        {
            _currentPoint = _startPoint;
            // Get the area (i.e. desired number of points) of the rectangle
            var area = _container.Width * _container.Height;
            
            /// The number of points that have currently been returned
            var points = 0;

            // Return the centre point of the spiral and add to the point counter
            // (but only if it's within the bounds of the rectangle)
            if (IsPointInRectangleBounds(_currentPoint))
            {
                yield return _currentPoint;
                points++;
            }

            // If the height of the rectangle is greater than its width set the spiral 
            // to move down on its first step. Otherwise, set it to move right.
            _currentDirection = (_container.Height > _container.Width) ? SpiralDirection.Down : SpiralDirection.Right;

            // Spiral until the number of points returned is equal to the number of points
            // that make up the rectangle.
            for (var length = 1; points < area; length++)
            {
                // Essentially the spiral is made up of 2 lines of length = 1, then 2 of length = 2, 
                // then 2 lines of length = 3 etc...So for each length, iterate twice.
                for (var i = 0; i < 2; i++)
                {
                    // Get the valid points across the spiral length moving in the current direction
                    foreach (var point in GetNextPoints(length).Where(x => IsPointInRectangleBounds(x)))
                    {
                        yield return point;
                        points++;
                    }

                    // Change the spiral direction - the order is Down, Right, Left, Up
                    _currentDirection = (SpiralDirection)(((int)_currentDirection + 1) % 4);
                }
            };

        }

        /// <summary>
        /// Get the next X points in the direction the spiral is currently moving. 
        /// </summary>
        /// <param name="moveCount">The number of points to move in the current spiral direction</param>
        /// <returns>The next points in the current direction of the spiral</returns>
        private IEnumerable<Point> GetNextPoints(int moveCount)
        {
            for (var i = 0; i < moveCount; i++)
            {
                MoveCurrentPointByOne();
                yield return _currentPoint;
            };

        }

        /// <summary>
        /// Move the current point to be the adjacent point in the direction that the 
        /// spiral is currently moving.
        /// </summary>
        private void MoveCurrentPointByOne()
        {
            switch (_currentDirection)
            {
                case (SpiralDirection.Down):
                    _currentPoint = new Point(_currentPoint.X - 1, _currentPoint.Y);
                    break;
                case (SpiralDirection.Right):
                    _currentPoint = new Point(_currentPoint.X, _currentPoint.Y + 1);
                    break;
                case (SpiralDirection.Up):
                    _currentPoint = new Point(_currentPoint.X + 1, _currentPoint.Y);
                    break;
                case (SpiralDirection.Left):
                    _currentPoint = new Point(_currentPoint.X, _currentPoint.Y - 1);
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        /// <summary>
        /// Returns True, if the specified point is within the bounds of the rectangle.
        /// </summary>
        /// <param name="p">Point to check</param>
        /// <returns>True if the point is within the rectangle bounds</returns>
        private bool IsPointInRectangleBounds(Point p)
        {
            return _container.Contains(p);
        }                               
        
    }
}