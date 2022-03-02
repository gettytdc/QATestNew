using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BluePrism.Images;

namespace AutomateControls
{
    /// <summary>
    /// Link label which contains an image as a bullet point.
    /// The image can be set with the <see cref="Image"/> property - it defaults to
    /// a green boxed right arrow, used throughout the System Manager controls.
    /// </summary>
    public class BulletedLinkLabel : LinkLabel
    {
        #region - Class-scope Declarations -

        /// <summary>
        /// The default image to use on this bulleted link label
        /// </summary>
        private static readonly Bitmap DefaultImage =
            ToolImages.Button_Blue_Forward_16x16;

        #endregion

        #region - Member Vars -

        // The left padding set when the first image was set.
        private int _savedPadding;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new bulleted link label with the default image.
        /// </summary>
        public BulletedLinkLabel()
        {
            this.ImageAlign = ContentAlignment.MiddleLeft;
            _savedPadding = -1;
            ResetImage();
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Sets the bullet image for this link label.
        /// This ensures that the padding is available within the link label for the
        /// image.
        /// </summary>
        public new Image Image
        {
            get { return base.Image; }
            set
            {
                if (value == base.Image)
                {
                    return;
                }

                var p = base.Padding;

                // If we're setting an existing image to null - set the padding to
                // the old value that it was when the image was set.
                if (value == null)
                {
                    if (base.Image != null)
                    {
                        p.Left = _savedPadding;
                    }
                }
                // Otherwise, we're not setting it to null
                else
                {
                    // Save the current padding if there's no image there.
                    if (base.Image == null)
                    {
                        _savedPadding = p.Left;

                    }

                    // Now set the padding to the saved padding (ie. the padding when
                    // the current image was set) plus the space required for the
                    // new image + a little more to allow space between image and
                    // link.
                    p.Left = _savedPadding + value.Width + 2;
                }

                // And set the padding in the base class.
                base.Padding = p;
                base.Image = value;
            }
        }

        /// <summary>
        /// Override of padding to ensure that it doesn't show up in the serializer.
        /// This class needs to be able to handle the padding itself without odd
        /// behaviour popping up if Image and Padding are set in different orders.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding
        {
            get { return base.Padding; }
            set { base.Padding = value; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Resets the image to its default value - a green boxed right arrow.
        /// </summary>
        private void ResetImage()
        {
            this.Image = DefaultImage;
        }

        /// <summary>
        /// Checks if the Image property should be serialized - it should be if it
        /// is not currently a green boxed right arrow.
        /// </summary>
        /// <returns>true if the image has been changed from the default; false if it
        /// is currently set as the default.</returns>
        private bool ShouldSerializeImage()
        {
            return !DefaultImage.Matches(this.Image);
        }

        #endregion
    }
}
