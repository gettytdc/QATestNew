using System.Drawing;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Defines a class which 'handles' an image - this interface only defines this as
    /// meaning that it has an <see cref="Image"/> property which can be set or
    /// retrieved from.
    /// </summary>
    interface IImageHandler
    {
        /// <summary>
        /// Gets or sets the image in this handler.
        /// </summary>
        Image Image { get; set; }       
    }
}
