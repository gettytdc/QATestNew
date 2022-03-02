using System;
using System.Drawing;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Delegate implementing a listener for spy requests.
    /// </summary>
    public delegate void SpyRequestEventHandler(object sender, SpyRequestEventArgs e);

    /// <summary>
    /// Event args detailing a spy operation request and allowing for a placeholder
    /// into which the spied image can be stored.
    /// </summary>
    public class SpyRequestEventArgs : EventArgs
    {
        // The spied image
        private Image _image;

        /// <summary>
        /// Gets or sets the image spied from the requested spy operation
        /// </summary>
        public Image SpiedImage
        {
            get { return _image; }
            set { _image = value; }
        }
    }
}
