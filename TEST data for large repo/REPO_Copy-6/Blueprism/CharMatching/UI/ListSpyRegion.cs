using BluePrism.CharMatching.Properties;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;
using BluePrism.BPCoreLib;
using AutomateControls;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// List spy region which describes a repeated region in a specified direction
    /// </summary>
    public class ListSpyRegion: SpyRegion
    {
        #region - Member Variables -

        // The direction that the list goes in
        private ListDirection _dirn;

        // The number of pixels between each list element.
        private int _padding;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new list spy region on the given container, defined by the
        /// specified area.
        /// </summary>
        /// <param name="cont">The container which will hold the region</param>
        /// <param name="area">The base area for the region</param>
        public ListSpyRegion(ISpyRegionContainer cont, Rectangle area)
            : this(cont, area, cont.GetUniqueRegionName(Resources.ListRegion0)) { }

        /// <summary>
        /// Creates a new list spy region on the given container, defined by the
        /// specified area.
        /// </summary>
        /// <param name="cont">The container which will hold the region</param>
        /// <param name="area">The base area for the region</param>
        /// <param name="name">The initial name of the region</param>
        public ListSpyRegion(ISpyRegionContainer cont, Rectangle area, string name)
            : this(cont, area, name, ListDirection.TopDown) { }

        /// <summary>
        /// Creates a new list spy region on the given container, defined by the
        /// specified area.
        /// </summary>
        /// <param name="cont">The container which will hold the region</param>
        /// <param name="area">The base area for the region</param>
        /// <param name="name">The initial name of the region</param>
        /// <param name="dirn">The direction in which the region should go, eg. for
        /// a region describing a list which starts at the topmost element and goes
        /// down, this would be <see cref="ListDirection.TopDown"/></param>
        public ListSpyRegion(ISpyRegionContainer cont, Rectangle area, string name, ListDirection dirn)
            : base(cont, area, name)
        {
            _dirn = dirn;
            RetainImage = false;
            LocationMethod = RegionLocationMethod.Coordinates;
            ImageSearchPadding = new Padding(0);
        }

        #endregion

        #region - Browsable Properties -

        /// <summary>
        /// The direction in which the list goes
        /// </summary>
        [CMCategory("Behaviour"),
         CMDescription("The direction in which the list goes"),
         CMDisplayName("ListDirection"),
         DefaultValue(ListDirection.TopDown)]
        public ListDirection ListDirection
        {
            get { return _dirn; }
            set { _dirn = value; OnRegionLayoutChanged(EventArgs.Empty); }
        }

        [CMCategory("Behaviour"),
         CMDescription("The number of pixels between each element in the list"),
         CMDisplayName("Padding")]
        public int Padding
        {
            get { return _padding; }
            set { _padding = value; OnRegionLayoutChanged(EventArgs.Empty); }
        }

        #endregion

        #region - Methods -

        private Rectangle GetRectangle(int index)
        {
            Rectangle r = this.Rectangle;
            r.Width++; r.Height++;
            if (index == 0)
                return r;
            switch (_dirn)
            {
                case ListDirection.LeftToRight:
                    r.Offset((r.Width + _padding) * index, 0);
                    break;
                case ListDirection.RightToLeft:
                    r.Offset((r.Width + _padding) * -index, 0);
                    break;
                case ListDirection.TopDown:
                    r.Offset(0, (r.Height + _padding) * index);
                    break;
                case ListDirection.BottomUp:
                    r.Offset(0, (r.Height + _padding) * -index);
                    break;
            }
            return r;
        }

        /// <summary>
        /// Paints this list spy region
        /// </summary>
        /// <param name="g">The graphics context on which to draw the region</param>
        /// <param name="cache">The cache of GDI objects to use to draw the region
        /// </param>
        protected internal override void Paint(Graphics g, GDICache cache)
        {
            base.Paint(g, cache);

            // If set to be invisible and this region is currently selected, we paint
            // a dashed line. If the region is not selected, then we remain invisible
            Pen p;
            if (!Visible)
            {
                if (!Selected) // Don't paint anything
                    return;
                p = cache.GetPen(Color.Gray, 1, DashStyle.Dash);
            }
            else
            {
                p = cache.GetPen(Color.Gray);
            }

            Rectangle regRect = this.Rectangle;
            if (regRect.Width > 0 && regRect.Height > 0 && (Active || Hovering))
            {
                Rectangle clip = Rectangle.Truncate(g.VisibleClipBounds);
                for (int i = 1; i < 30; i++)
                {
                    Rectangle r = GetRectangle(i);
                    if (!r.IntersectsWith(clip))
                        break;
                    g.DrawRectangle(p, r);
                }
            }
        }

        #endregion

    }

}
