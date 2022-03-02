using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BluePrism.BPCoreLib.Collections;
using AutomateControls.Filters;

namespace AutomateControls
{
    /// <summary>
    /// User control which provides a unified way of specifying filters for the
    /// columns therein.
    /// 
    /// To add a filtered column to this listview, you call one of the overloaded
    /// 'AddColumn' methods.
    /// </summary>
    public partial class FilteredList: UserControl
    {
        public event FilterSetChangedHandler FilterApplied;

        #region - ColumnDefinition class -

        /// <summary>
        /// Class to wrap the full definition of a filtered listview column.
        /// </summary>
        private class ColumnDefinition
        {
            // The listview's column header
            private ColumnHeader _header;
            // The filter applied to this column
            private Filter _filter;
            // The combo box containing the filter for this column
            private ComboBox _combo;

            /// <summary>
            /// Creates a new column definition based around the given values.
            /// </summary>
            /// <param name="header">The listview's column header for the column
            /// that this column definition defines.</param>
            /// <param name="filter">The filter for this column.</param>
            public ColumnDefinition(ColumnHeader header, Filter filter)
            {
                _filter = filter;
                _header = header;

                _combo = filter.Combo;
                _combo.Margin = new Padding(1, 0, 1, 0);
                SetColumnWidth(header.Width);
            }

            /// <summary>
            /// The column header related to this column.
            /// </summary>
            public ColumnHeader Header
            {
                get { return _header; }
            }

            /// <summary>
            /// Checks if this column is an image column or not.
            /// </summary>
            public bool IsImageColumn
            {
                get { return _filter.Definition.RepresentedByImages; }
            }

            /// <summary>
            /// Gets the image represented in this column definition by
            /// the given filter term.
            /// </summary>
            /// <param name="filterTerm">The filter term to retrieve the
            /// image for.</param>
            /// <returns>The image corresponding to the given filter term,
            /// or null if this column is not represented by images, or if
            /// no such filter term was found.
            /// </returns>
            public Image GetImage(string filterTerm)
            {
                if (!IsImageColumn)
                    return null;
                foreach (FilterItem fi in _filter.Definition.Items)
                {
                    if (fi.FilterTerm.Equals(filterTerm))
                        return (fi.DisplayValue as Image);
                }
                return null;
            }

            /// <summary>
            /// The filter applied to this column
            /// </summary>
            public Filter Filter
            {
                get { return _filter; }
            }

            /// <summary>
            /// The combo box representing the filter for this column definition
            /// </summary>
            public ComboBox Combo
            {
                get { return _combo; }
            }

            /// <summary>
            /// Sets the column width to the given value - this ensures that the
            /// combo box representing the filter is set to the same value.
            /// </summary>
            /// <param name="width">The width of the column to use as a guide for
            /// the combo box held herein.</param>
            public void SetColumnWidth(int width)
            {
                // -1 indicates that the current column width should be used.
                if (width == -1)
                    width = _header.Width;
                // combos have a margin-left and margin right of 1 pixel apiece.
                // Ensure that that is taken into account when calculating the
                // required size of the combo
                int hpad = _combo.Margin.Horizontal;
                _combo.Width = (width >= hpad ? width - hpad : width);
            }
        }

        #endregion

        /// <summary>
        /// A map of column definitions keyed on the name of the column
        /// </summary>
        private IDictionary<string, ColumnDefinition> _columns;

        /// <summary>
        /// The set of filters handled by this list.
        /// </summary>
        private FilterSet _filters;

        /// <summary>
        /// Creates a new FilteredListView control
        /// </summary>
        public FilteredList()
        {
            InitializeComponent();
            lview.Scroll += HandleListViewScrolled;
            lview.ColumnWidthChanging += HandleColumnWidthChanging;
            lview.ColumnWidthChanged += HandleColumnWidthChanged;
            lview.OwnerDraw = true;
            lview.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(HandleColumnHeaderDrawing);
            lview.DrawItem += new DrawListViewItemEventHandler(HandleItemDrawing);
            lview.DrawSubItem += new DrawListViewSubItemEventHandler(HandleSubItemDrawing);
            _columns = new clsOrderedDictionary<string, ColumnDefinition>();
            _filters = new FilterSet();
        }

        /// <summary>
        /// Gets the listview which is held inside this filtered list.
        /// </summary>
        public ScrollHandlingListView FilteredView
        {
            get { return lview; }
        }

        /// <summary>
        /// Gets the current applied filters on this list.
        /// </summary>
        public FilterSet CurrentFilters
        {
            get { return _filters; }
        }

        #region - Drawing -

        /// <summary>
        /// Handles the listview item being drawn.
        /// </summary>
        private void HandleItemDrawing(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Item.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Handles the listview subitem being drawn.
        /// </summary>
        private void HandleSubItemDrawing(object sender, DrawListViewSubItemEventArgs e)
        {
            ListView lv = sender as ListView;
            ColumnHeader header = lv.Columns[e.ColumnIndex];
            ColumnDefinition coldef = _columns[header.Name];

            Color col = default(Color);
            if (e.Item.Selected)
            {
                if (coldef != null && coldef.IsImageColumn)
                {
                    col = SystemColors.WindowText;
                    e.Graphics.FillRectangle(new SolidBrush(Color.Wheat), e.Bounds);
                }
                else
                {
                    col = SystemColors.HighlightText;
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
                }
            }
            else
            {
                col = SystemColors.WindowText;
                e.DrawBackground();
            }

            StringFormat sf = new StringFormat();
            Rectangle bounds = e.Bounds;
            switch (e.Header.TextAlign)
            {
                case HorizontalAlignment.Center:
                    sf.Alignment = StringAlignment.Center;
                    break;
                case HorizontalAlignment.Right:
                    sf.Alignment = StringAlignment.Far;
                    bounds.Offset(-3, 0);
                    break;
                default:
                    sf.Alignment = StringAlignment.Near;
                    bounds.Offset(3, 0);
                    break;
            }
            bounds.Inflate(-2, 0);

            e.Graphics.DrawString(e.SubItem.Text, e.Item.Font, new SolidBrush(col), bounds, sf);

            if (coldef!=null && coldef.IsImageColumn)
            {
                Image img = coldef.GetImage(e.Item.ImageKey);
                if (img != null)
                {
                    int vertOffset = Math.Max(0, (lview.ItemHeight - img.Height) / 2);
                    int left = 0;
                    switch (e.Header.TextAlign)
                    {
                        case HorizontalAlignment.Left:
                            left = 2;
                            break;
                        case HorizontalAlignment.Right:
                            left = e.Header.Width - img.Width - 2;
                            break;
                        case HorizontalAlignment.Center:
                            left = (e.Header.Width - img.Width) / 2;
                            break;
                    }

                    e.Graphics.DrawImage(img, new Rectangle(Point.Add(e.Bounds.Location, new Size(left, 0)), img.Size));
                }
            }

        }

        private void HandleColumnHeaderDrawing(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        #endregion

        #region - Add Filter -

        public void AddFilter(IFilterDefinition def)
        {
            AddFilter(def.Name, 100, HorizontalAlignment.Left, def);
        }

        public void AddFilter(IFilterDefinition def, string headerText)
        {
            AddFilter(def.Name, headerText, 100, HorizontalAlignment.Left, def);
        }

        public void AddFilter(int width, IFilterDefinition def)
        {
            AddFilter(def.Name, width, HorizontalAlignment.Left, def);
        }

        public void AddFilter(int width, HorizontalAlignment align, IFilterDefinition def)
        {
            AddFilter(def.Name, width, align, def);
        }

        public void AddFilter(string key, int width, HorizontalAlignment align, IFilterDefinition def)
        {
            AddFilter(def.Name, def.Name, 100, HorizontalAlignment.Left, def);
        }

        public void AddFilter(string key, string headerText, int width, HorizontalAlignment align, IFilterDefinition def)
        {
            // Add the column and create the column definition
            ColumnHeader header = lview.Columns.Add(key, headerText, width, align, -1);
            Filter f = new Filter(def);
            f.FilterChanging += HandleFilterChanged;
            _filters.Add(f);
            ColumnDefinition colDefn = new ColumnDefinition(header, f);
            _columns[key] = colDefn;
            flowPanel.Controls.Add(colDefn.Combo);
        }

        #endregion

        //public void AddRow(params string[] subitems)
        //{
        //    if (subitems.Length > _columns.Count)
        //        throw new IndexOutOfRangeException("Too many subitems or too few columns");
        //    for (int i = 0; i < subitems.Length; i++)
        //    {
        //        string subitem = subitems[i];
        //        ColumnHeader col = lview.Columns[i];
        //        ColumnDefinition defn = _columns[col.Name];
        //    }
        //}

        #region - Filters Changing -

        private void HandleFilterChanged(Filter source, FilterChangingEventArgs args)
        {
            if (FilterApplied != null)
            {
                FilterApplied(_filters);
            }
        }

        #endregion

        #region - Scrolling and Column resizing -

        /// <summary>
        /// Handles the listview being scrolled.
        /// </summary>
        private void HandleListViewScrolled(object sender, ScrollEventArgs e)
        {
        }

        /// <summary>
        /// Handles a column width on the listview changing
        /// </summary>
        private void HandleColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            UpdateColumnWidth(e.ColumnIndex, e.NewWidth);
        }

        /// <summary>
        /// Handles a column width on the listview being changed.
        /// </summary>
        private void HandleColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            UpdateColumnWidth(e.ColumnIndex, -1);
        }

        private void UpdateColumnWidth(int colIndex, int width)
        {
            ColumnHeader header = lview.Columns[colIndex];
            if (_columns.ContainsKey(header.Name))
            {
                _columns[header.Name].SetColumnWidth(width);
                // if the scrollbar is at the far right and the column is reduced,
                // the listview reduces in size... effectively, its virtual (left)
                // position shifts to the right in relation to its parent...
                // the flowPanel with the filters on needs to match that change.
                flowPanel.Left = -lview.ScrollPosition.X;
            }
        }

        #endregion

#if DEBUG
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            using (Form f = new Form())
            {
                f.Size = new Size(400, 300);
                FilteredList flv = new FilteredList();
                Dictionary<string, Image> map = new Dictionary<string, Image>();
                map["locked"] = Properties.Resources.padlock_12x12;
                map["free"] = Properties.Resources.ellipsis_12x12;
                flv.AddFilter(new ImageFilterDefinition("Status", map));
                flv.AddFilter(new StringFilterDefinition("Name"));
                flv.AddFilter(new StringFilterDefinition("Resource"));
                flv.AddFilter(new StringFilterDefinition("Process"));
                flv.AddFilter(new PastDateFilterDefinition("Lock Time"));
                flv.AddFilter(new StringFilterDefinition("Last comment"));
                flv.Dock = DockStyle.Fill;
                flv.FilterApplied += new FilterSetChangedHandler(HandleFilterApplied);
                f.Controls.Add(flv);
                f.ShowDialog();
            }
        }

        static void HandleFilterApplied(FilterSet source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Filter f in source)
            {
                String t = f.FilterTermString;
                if (!string.IsNullOrEmpty(t))
                {
                    if (sb.Length > 0) sb.Append(';');
                    sb.Append(f.Name).Append("=").Append(f.SelectedFilterItem.Value);
                    // sb.Append(t);
                }
                // sb.Append('[').Append(f.Name.Replace("]", "\\]")).Append("]=").Append(
            }
            if (sb.Length > 0)
                Console.WriteLine(sb.ToString());
            else
                Console.WriteLine("No filtering applied");
        }

        

#endif

    }
}
