using System;
using System.Runtime.Serialization;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// Exception raised when a data grid view cell template is set into a data grid
    /// view column which does not support that type of cell.
    /// </summary>
    [Serializable]
    public class CellTemplateException : InvalidArgumentException
    {
        /// <summary>
        /// Creates a new exception with no message
        /// </summary>
        public CellTemplateException() : base() { }

        /// <summary>
        /// Creates a new exception with a message
        /// </summary>
        /// <param name="msg">The message describing the exception</param>
        public CellTemplateException(string msg) : base(msg) { }

        /// <summary>
        /// Creates a new exception with a parameterized message
        /// </summary>
        /// <param name="msg">The message with formatting placeholders</param>
        /// <param name="args">The arguments to slot into the format messsage</param>
        public CellTemplateException(string msg, params object[] args)
            : base(msg, args) { }

        /// <summary>
        /// Creates a new exception detailing the supported type and the receieved
        /// type in the column
        /// </summary>
        /// <param name="supported">The type of cell template which is supported by
        /// the raising method/property.</param>
        /// <param name="received">The type of cell template which was received by
        /// the raising method/property.</param>
        internal CellTemplateException(Type supported, Type received) : this(
            "Columns of this type only support cells of type: {0}. " +
            "Value provided was of type: {1}", supported, received) { }

        /// <summary>
        /// Creates a new exception from a serialization stream
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="ctx">The stream context in which this exception is being
        /// deserialized</param>
        protected CellTemplateException(
            SerializationInfo info, StreamingContext ctx)
            : base(info, ctx) { }
    }
}
