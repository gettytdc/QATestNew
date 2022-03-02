using System;

namespace BluePrism.Core.Data
{
    /// <summary>
    /// An interface which describes an object with some item information, and which
    /// can thus be used inside a DataGridView as the value set in a
    /// <see cref="DataGridViewItemInfoCell"/>.
    /// </summary>
    public interface IItemHeader : IComparable
    {
        /// <summary>
        /// The main title of the item
        /// </summary>
        string Title { get; }

        /// <summary>
        /// A subtitle of the item
        /// </summary>
        string SubTitle { get; }

        /// <summary>
        /// The image key of the item. This will be used to look up an associated
        /// image from the imagelist in the column which hosts the item info cells.
        /// Has no effect if the column has no image list set in it.
        /// </summary>
        string ImageKey { get; }
    }
}
