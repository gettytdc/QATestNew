using System.Windows.Forms;
using System.Drawing;

namespace AutomateControls
{
    /// <summary>
    /// Class to encapsulate a button and image together, with the image preceding
    /// the text.
    /// Really, it's just here to provide a useful constructor which sets the
    /// appropriate properties, rather than adding anything that Button doesn't
    /// already provide in and of itself.
    /// </summary>
    public class ImageButton : Button
    {
        /// <summary>
        /// Creates a new uninitialised image button
        /// </summary>
        public ImageButton() : this(null, null) { }

        /// <summary>
        /// Creates a new image button with the given text and no image.
        /// </summary>
        /// <param name="text">The text to display on the button</param>
        public ImageButton(string text) : this(text, null) { }

        /// <summary>
        /// Creates a new image button with the given text and image.
        /// </summary>
        /// <param name="text">The text to show on the button</param>
        /// <param name="img">The image on the button, null for no image</param>
        public ImageButton(string text, Image img)
        {
            Text = text;
            Image = img;
            TextImageRelation = TextImageRelation.ImageBeforeText;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            AutoSize = true;
        }
    }
}
