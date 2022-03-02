using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// DataGridViewColumn which supports arbitrary image lists
    /// </summary>
    public class ImageListColumn : DataGridViewColumn
    {

        #region - Member Variables -

        // The list of images which this column will use for its cells
        private ImageList _imgs;

        #endregion

        #region - Properties -

        /// <summary>
        /// The ImageList that this column will use to display its cells
        /// </summary>
        [DefaultValue(null),
         RefreshProperties(RefreshProperties.Repaint),
         Category("Behavior"),
         Description("The ImageList associated with this column")]
        public ImageList ImageList
        {
            get { return _imgs; }
            set
            {
                if (_imgs == value)
                    return;
                _imgs = value;
                if (_imgs == null)
                    return;
                int width = _imgs.ImageSize.Width;
                this.Width = width;
                this.MinimumWidth = width;
            }
        }

        /// <summary>
        /// Gets or sets the cell template for this column. The cell template
        /// for columns of this type must be instances of <see cref="ImageListCell"/>
        /// </summary>
        /// <exception cref="ArgumentException">If setting the cell template to a
        /// cell type other than <see cref="ImageListCell"/></exception>
        public override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set
            {
                if (value == null || !(value is ImageListCell))
                {
                    throw new ArgumentException("Invalid cell template : "+
                        "ImageListColumn only supports ImageListCells");
                }
                base.CellTemplate = value;
            }
        }

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new image list column with no currently set images
        /// </summary>
        public ImageListColumn() : this(null) { }

        /// <summary>
        /// Creates a new image list column, initialising it to use the
        /// given image list.
        /// </summary>
        /// <param name="images">The image list that this column is using for
        /// its cells.</param>
        public ImageListColumn(ImageList images)
            : base(new ImageListCell())
        {
            this.ImageList = images;
            this.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.MinimumWidth = 20;
            this.Width = MinimumWidth;
            this.Resizable = DataGridViewTriState.False;
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Clones this image list column.
        /// </summary>
        /// <returns>A clone of this column</returns>
        /// <remarks>This method needs to be here to support the ImageList property -
        /// it appears that base.Clone() doesn't actually perform a memberwise clone,
        /// so we have to copy across everything, regardless of ref vs value type.
        /// See http://tinyurl.com/m7k4j77 which is where I attained this knowledge.
        /// </remarks>
        public override object Clone()
        {
            ImageListColumn clone = base.Clone() as ImageListColumn;
            clone.ImageList = this.ImageList;
            return clone;
        }

        /// <summary>
        /// Gets the image corresponding to the given string image key.
        /// </summary>
        /// <param name="key">The image key for the required image.</param>
        /// <returns>The image corresponding to the given key.</returns>
        internal Image GetImage(string key)
        {
            if (_imgs == null)
                return null;
            return _imgs.Images[key];
        }

        /// <summary>
        /// Gets the image corresponding to the given image index.
        /// </summary>
        /// <param name="key">The image index for the required image.</param>
        /// <returns>The image corresponding to the given index.</returns>
        internal Image GetImage(int index)
        {
            if (_imgs == null)
                return null;
            return _imgs.Images[index];
        }

        /// <summary>
        /// Gets the image corresponding to the given object.
        /// </summary>
        /// <param name="key">The object to use to get the image. If it is
        /// a string, it will be assumed to be an image key. If it is an int,
        /// it will be treated as an image index. If it is an image, it will
        /// be returned as is. Otherwise, it is ignored.</param>
        /// <returns>The image corresponding to the given object, or null if
        /// no such image could be found or the given object was unrecognised
        /// as a way of detailing an image.</returns>
        internal Image GetImage(object o)
        {
            if (o is string) return GetImage((string)o);
            if (o is int) return GetImage((int)o);
            if (o is Image) return (Image)o;
            return null;
        }

        #endregion

    }
}
