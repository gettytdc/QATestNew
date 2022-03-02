using BluePrism.Server.Domain.Models;
using System;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// Exception thrown when an operation on a column cannot be completed because
    /// the column does not have a cell template to work with
    /// </summary>
    [Serializable]
    public class MissingTemplateException : MissingItemException
    {
        /// <summary>
        /// Creates a new exception with the default message.
        /// </summary>
        public MissingTemplateException() : base(
            "Operation cannot be completed because this DataGridViewColumn does " +
            "not have a CellTemplate") { }
    }
}
