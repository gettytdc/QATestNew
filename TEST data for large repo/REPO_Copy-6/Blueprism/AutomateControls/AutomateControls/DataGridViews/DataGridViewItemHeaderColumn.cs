using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// A DataGridView column which can host cells which display 'item information'
    /// in the form of an implementation of
    /// <see cref="BluePrism.Core.Data.IItemHeader"/>
    /// </summary>
    public class DataGridViewItemHeaderColumn : DataGridViewColumn
    {
        #region - Member Variables -

        // Flag indicating if this column is disposed
        private bool _disposed;

        // The image list used by cells in this column
        private ImageList _images;

        // The cache for use by all cells in this column
        private GDICache _cache;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new data grid view column to display 'item info'
        /// </summary>
        public DataGridViewItemHeaderColumn()
            : base(new DataGridViewItemHeaderCell()) { }

        #endregion

        #region - Properties -

        /// <summary>
        /// The image list associated with this column, used by the cells which are
        /// hosted in the data grid view
        /// </summary>
        [Category("Behavior"), ]
        public ImageList ImageList
        {
            get { return _images; }
            set { _images = value; }
        }

        /// <summary>
        /// Gets or sets the cell template for this column.
        /// </summary>
        /// <exception cref="InvalidArgumentException">If setting with a value of
        /// null or any cell type other than <see cref="DataGridViewItemHeaderCell"/>
        /// or a subclass.</exception>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set
            {
                if (value == null || !(value is DataGridViewItemHeaderCell))
                    throw new InvalidArgumentException(
                        "Invalid cell template: {0}; this column only supports " +
                        "DataGridViewItemInfoCell instances", value);

                base.CellTemplate = value;
            }
        }

        /// <summary>
        /// Gets the GDI cache maintained by this column
        /// </summary>
        internal GDICache Cache { get { return _cache; } }

        #endregion

        #region - Methods -

        /// <summary>
        /// Raises the DataGridViewChanged event.
        /// Also ensures that a new cache is created if this column is set into a
        /// data grid view, and the old one is disposed and removed.
        /// </summary>
        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();
            if (_cache != null)
                _cache.Dispose();
            if (DataGridView == null)
                _cache = null;
            else
                _cache = new GDICache();
        }

        /// <summary>
        /// Gets the image associated with the given key in this column, or null if
        /// there is no image list associated with this column, or there is no image
        /// in that list with the specified key.
        /// </summary>
        /// <param name="key">The key for which the image is required.</param>
        /// <returns>The image associated with the given key in this column or null
        /// if this column has no images associated with it, or no image with the
        /// given key was found within it</returns>
        internal Image GetImage(string key)
        {
            if (_images == null || string.IsNullOrEmpty(key))
                return null;
            return _images.Images[key];
        }

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
            var clone = base.Clone() as DataGridViewItemHeaderColumn;
            clone.ImageList = this.ImageList;
            return clone;
        }

        /// <summary>
        /// Disposes of this column, ensuring that the image list and GDI cache are
        /// disposed of too.
        /// </summary>
        /// <param name="explicitly"></param>
        protected override void Dispose(bool explicitly)
        {
            if (_disposed) return;

            if (explicitly)
            {
                if (_cache != null)
                {
                    _cache.Dispose();
                    _cache = null;
                }
                // Note - we don't dispose of the imagelist; it's set from outside
                // this object, so we are not the owners, so it's not up to use to
                // determine when it has reached its end of life
                _images = null;
            }
            base.Dispose(explicitly);
            _disposed = true;
        }

        #endregion

    }
}
