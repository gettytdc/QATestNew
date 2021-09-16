using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using BluePrism.BPCoreLib;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using AutomateControls;
using BluePrism.CharMatching.UI.Designer;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Spy region which describes a 2d grid of columns and rows of varying size.
    /// </summary>
    public class GridSpyRegion : SpyRegion, IComponent
    {
        #region - Constructors -

        /// <summary>
        /// Creates a new spy region within the given container, described
        /// by the specified rectangle.
        /// </summary>
        /// <param name="cont">The container which holds this region</param>
        /// <param name="rect">The rectangle describing this region</param>
        public GridSpyRegion(ISpyRegionContainer cont, Rectangle rect)
            : this(cont, rect, cont.GetUniqueRegionName(Resources.GridRegion0), 2, 2) { }

        /// <summary>
        /// Creates a new spy region within the given container, described
        /// by the specified rectangle.
        /// </summary>
        /// <param name="cont">The container which holds this region</param>
        /// <param name="rect">The rectangle describing this region</param>
        /// <param name="name">The initial name of the region</param>
        public GridSpyRegion(ISpyRegionContainer cont, Rectangle rect, string name)
            : this(cont, rect, name, 2, 2) { }

        /// <summary>
        /// Creates a new spy region within the given container, described
        /// by the specified rectangle.
        /// </summary>
        /// <param name="cont">The container which holds this region</param>
        /// <param name="rect">The rectangle describing this region</param>
        /// <param name="name">The initial name of the region</param>
        /// <param name="cols">The number of initial columns in the region.</param>
        /// <param name="rows">The number of initial rows in the region.</param>
        public GridSpyRegion(
            ISpyRegionContainer cont, Rectangle rect, string name, int cols, int rows)
            : base(cont, rect, name)
        {
            _schema = new GridSpyRegionSchema(rows, cols);
            _schema.SchemaChanged += HandleSchemaChanged;
            RetainImage = false;
            LocationMethod = RegionLocationMethod.Coordinates;
            ImageSearchPadding = new Padding(0);
        }

        #endregion

        #region - Member Variables -

        // The schema of this region - ie. the makeup of rows and columns
        private GridSpyRegionSchema _schema;

        // Event fired when this component is disposed (IComponent implementation)
        private event EventHandler Disposed;

        // The site used to place this component (IComponent implementation)
        private ISite _site;

        #endregion

        #region - Properties -

        /// <summary>
        /// The schema detailing the rows and columns within this grid region
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter)),
         Editor(typeof(GridSpyRegionSchemaTypeEditor), typeof(UITypeEditor)),
         Description("The schema detailing the grid's rows and columns")]
        public GridSpyRegionSchema Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }

        /// <summary>
        /// A readonly collection of absolute sizes of the rows of this region
        /// </summary>
        [Browsable(false)]
        public ICollection<int> RowSizes
        {
            get { return GridVector.GetAbsolutes(_schema.Rows, Rectangle.Height); }
        }

        /// <summary>
        /// A readonly collection of absolute sizes of the columns of this region.
        /// </summary>
        [Browsable(false)]
        public ICollection<int> ColumnSizes
        {
            get { return GridVector.GetAbsolutes(_schema.Columns, Rectangle.Width); }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Paints this region
        /// </summary>
        /// <param name="g">The graphics context to use to paint this region</param>
        /// <param name="cache">The GDI cache containing the cached pens and brushes
        /// to be used in this paint operation</param>
        protected internal override void Paint(Graphics g, GDICache cache)
        {
            base.Paint(g, cache);

            Pen p;
            if (!Visible)
            {
                // we don't paint anything if it's not visible -and- not selected
                if (!Selected)
                    return;

                // It's set to be invisible, but it's selected, so we draw an
                // outline of it with a dashed pen.
                p = cache.GetPen(BorderColor, 1, DashStyle.Dash);
            }
            else
            {
                p = cache.GetPen(BorderColor);
            }

            Rectangle r = this.Rectangle;
            int x = r.X;
            foreach (int colSize in ColumnSizes)
            {
                x += colSize;
                if (x < r.Right)
                    g.DrawLine(p, new Point(x, r.Y), new Point(x, r.Bottom));
            }

            int y = r.Y;
            foreach (int rowSize in RowSizes)
            {
                y += rowSize;
                if (y < r.Bottom)
                    g.DrawLine(p, new Point(r.X, y), new Point(r.Right, y));
            }
        }

        /// <summary>
        /// Handles the schema changing by indicating that the region itself has
        /// changed.
        /// </summary>
        private void HandleSchemaChanged(object sender, EventArgs e)
        {
            OnRegionLayoutChanged(e);
        }

        #endregion

        #region - IComponent/IDisposable Implementation -

        /// <summary>
        /// Custom event used when this component is disposed. Only of interest to
        /// IComponent consumers.
        /// </summary>
        event EventHandler IComponent.Disposed
        {
            add { Disposed += value; }
            remove { Disposed -= value; }
        }

        /// <summary>
        /// The site which is hosting this component
        /// </summary>
        ISite IComponent.Site
        {
            get
            {
                if (_site == null)
                    _site = new GridSpyRegionSite(this);
                return _site;
            }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Disposes of this component. Actually has no effect other than raising the
        /// custom <see cref="Disposed"/> event
        /// </summary>

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);

            _disposed = true;
        }

        ~GridSpyRegion()
        {
            Dispose(false);
        }


        #endregion
    }
}
