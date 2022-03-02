using System;
using BluePrism.BPCoreLib;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Enumeration of the different 'shift' operations supported by the CharMatching
    /// classes.
    /// </summary>
    public enum ShiftOperation
    {
        None = 0,
        PadTop, PadRight, PadBottom, PadLeft,
        TrimTop, TrimRight, TrimBottom, TrimLeft,
        ShiftUp, ShiftRight, ShiftDown, ShiftLeft
    }

    /// <summary>
    /// Defined delegate for shift operation events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The args detailing the event</param>
    public delegate void ShiftOperationEventHandler(
        object sender, ShiftOperationEventArgs e);

    /// <summary>
    /// Event args class detailing a shift operation event
    /// </summary>
    public class ShiftOperationEventArgs : EventArgs
    {
        // The shift operation which caused this event
        private ShiftOperation _op;

        /// <summary>
        /// Creates a new event args object representing the given operation
        /// </summary>
        /// <param name="op"></param>
        public ShiftOperationEventArgs(ShiftOperation op)
        {
            _op = op;
        }

        /// <summary>
        /// The operation which is represented by this event
        /// </summary>
        public ShiftOperation Operation { get { return _op; } }

        /// <summary>
        /// Gets the direction that this shift operation refers to
        /// </summary>
        public Direction Direction
        {
            get
            {
                switch (_op)
                {
                    case ShiftOperation.PadTop:
                    case ShiftOperation.TrimTop:
                    case ShiftOperation.ShiftUp:
                        return Direction.Top;

                    case ShiftOperation.PadRight:
                    case ShiftOperation.TrimRight:
                    case ShiftOperation.ShiftRight:
                        return Direction.Right;

                    case ShiftOperation.PadBottom:
                    case ShiftOperation.TrimBottom:
                    case ShiftOperation.ShiftDown:
                        return Direction.Bottom;

                    case ShiftOperation.PadLeft:
                    case ShiftOperation.TrimLeft:
                    case ShiftOperation.ShiftLeft:
                        return Direction.Left;

                    default:
                        return Direction.None;
                }
            }
        }
    }

}
