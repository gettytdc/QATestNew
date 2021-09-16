using System;
using System.Windows.Forms;
using System.ComponentModel;
using BluePrism.Server.Domain.Models;

namespace AutomateControls.DataGridViews
{
    /// <summary>
    /// A data grid view column which supports item statuses.
    /// </summary>
    public class ItemStatusColumn: DataGridViewColumn
    {

        #region - Member Variables -

        /// <summary>
        /// The default value to use for cells in this column, if they do
        /// not have their own item status set.
        /// </summary>
        private ItemStatus _default;

        /// <summary>
        /// Flag to display a person icon rather than a cross for ItemStatus.Failed
        /// </summary>
        private bool _displayPersonForException;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new ItemStatusColumn with some initial properties.
        /// </summary>
        public ItemStatusColumn()
            : base(new ItemStatusCell())
        {
            this.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.Resizable = DataGridViewTriState.False;

            // These are nice to have but can be overridden in the designer, 
            // and we've no way of inhibiting that (lack of overrides rock)
            this.MinimumWidth = 24;
            this.Width = this.MinimumWidth;

            // Set the default to something sensible.
            _default = ItemStatus.Unknown;
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Overrides the cloning of this column. Necessary for the designer to
        /// actually set the properties in this object. Obviously.
        /// </summary>
        public override object Clone()
        {
            ItemStatusColumn col = (ItemStatusColumn) base.Clone();
            col.DefaultItem = _default;
            col.UsePersonIconForException = _displayPersonForException;
            return col;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The default value to use for cells in this column, if they do
        /// not have their own item status set.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(ItemStatus.Unknown)]
        [DisplayName("Default Item Status")]
        [Description("The default ItemStatus value to use for cells in this column")]
        public ItemStatus DefaultItem
        {
            get { return _default; }
            set { _default = value; }
        }

        /// <summary>
        /// Flag to use a 'person' icon for an exception / failed status value.
        /// There is currently no distinction between the two, so changing this
        /// for one will change it for both.
        /// </summary>
        [Browsable(true)]
        [DefaultValue(false)]
        [DisplayName("Exceptions display 'person'")]
        [Description("Flag to use a 'person' icon for exceptions / failures in this column")]
        public bool UsePersonIconForException
        {
            get { return _displayPersonForException; }
            set { _displayPersonForException = value; }
        }

        /// <summary>
        /// Gets or sets the cell template used by this column.
        /// This override checks that the given cell template is a non-null
        /// instance of <see cref="ItemStatusCell"/> before setting it.
        /// </summary>
        /// <exception cref="ArgumentException">If, when setting, the given 
        /// cell template is null or not an <see cref="ItemStatusCell"/>
        /// </exception>
        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                if (value != null && !(value is ItemStatusCell))
                    throw new CellTemplateException(typeof(ItemStatusColumn), value.GetType());
                base.CellTemplate = value;
            }
        }

        #endregion

    }
}
